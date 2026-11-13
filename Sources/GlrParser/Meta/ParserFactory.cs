// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar;
using Glr.Grammar.Annotations;
using Glr.Grammar.Elements;
using Glr.Grammar.Elements.Sequence;
using Glr.Grammar.Elements.Symbols;
using Glr.Meta.Actions;
using Glr.Meta.Analysis;
using Glr.Meta.Attributes;
using Glr.Meta.Factories;
using Glr.Meta.SymbolHandlers;
using Glr.Meta.Transform;
using Glr.Parser;
using Glr.Parser.Conflict.Priority;
using Glr.Parser.Table;
using Glr.Scanner;
using Glr.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Glr.Meta;

/// <summary>
/// A factory for parsers. The grammar of the parser is defined as in-code annotations.
/// </summary>
public sealed class ParserFactory
{
  private ParserFactory() { }

  static ParserFactory()
  {
    CreateGrammar();
  }

  private static void CreateGrammar()
  {
    List<Production<IElement>> rules = new List<Production<IElement>>();
    rules.Add(new Production<IElement>(Rule, ItemAlternatives));
    rules.Add(new Production<IElement>(ItemAlternatives, ItemSequence, new ZeroOrMoreElement(PipeTerminal, ItemSequence)));
    rules.Add(new Production<IElement>(ItemSequence, new OnceOrMoreElement(QantifiedItem)));
    rules.Add(new Production<IElement>(AnnotatedItem, new OptionalElement(new AlternativeList(ShortAnnotationTerminal, LongAnnotationTerminal)), Item));
    rules.Add(new Production<IElement>(NegatedTerminals, NotTerminal, LeftParenTerminal, Terminal, new ZeroOrMoreElement(PipeTerminal, Terminal), RightParenTerminal));
    rules.Add(new Production<IElement>(NegatedTerminals, NotTerminal, Terminal));
    rules.Add(new Production<IElement>(Item, new AlternativeList(Symbol, Group, Assignment, NegatedTerminals)));
    rules.Add(new Production<IElement>(QantifiedItem, AnnotatedItem, new OptionalElement(Quantifier)));
    rules.Add(new Production<IElement>(Assignment, FieldName, new AlternativeList(EqualsTerminal, PlusEqualsTerminal, OptionalEqualsTerminal), Symbol));
    rules.Add(new Production<IElement>(Group, LeftParenTerminal, ItemAlternatives, RightParenTerminal));
    rules.Add(new Production<IElement>(Symbol, new AlternativeList(Terminal, NonTerminal)));
    rules.Add(new Production<IElement>(FieldName, IdentifierTerminal));
    rules.Add(new Production<IElement>(Quantifier, new AlternativeList(QestionmarkTerminal, PlusTerminal, AsteriskTerminal)));
    rules.Add(new Production<IElement>(NonTerminal, IdentifierTerminal));
    rules.Add(new Production<IElement>(Terminal, StrTerminal));
    rules.Add(new Production<IElement>(Terminal, LessTerminal, IdentifierTerminal, GreaterTerminal));

    // define skip terminals
    _skipTerminals.Add(WhitespaceTerminal);

    _grammar = new DefaultGrammar(rules, Rule, EofTerminal, _skipTerminals);
    _table = new DynamicCachingTable(_grammar);
  }

  /// <summary>
  /// Creates a new parser. The grammar is derived from in-code annotations.
  /// </summary>
  /// <typeparam name="T">The type of the object model's root node.</typeparam>
  /// <param name="skipTerminals">The terminals that shall be skipped while parsing.</param>
  /// <param name="isRecoveryEnabled">When <see langword="true"/>, the created parser will recover from parsing errors.</param>
  /// <returns>The new parser.</returns>
  public static IParser<T> CreateParser<T>(bool isRecoveryEnabled = false, IEnumerable<Terminal> skipTerminals = null) where T : class
  {
    var parserFactory = new ParserFactory();
    if (skipTerminals == null) skipTerminals = Array.Empty<Terminal>();
    return parserFactory.InternalCreateParser<T>(isRecoveryEnabled, skipTerminals);
  }

  private ClientParser<T> InternalCreateParser<T>(bool isRecoveryEnabled, IEnumerable<Terminal> skipTerminals) where T : class
  {
    // start symbol
    var rootType = typeof(T);
    var startSymbolName = rootType.Name;

    // parse all rules
    var parseTrees = new Dictionary<string, DefaultTreeNode>();
    var typeResolver = new TypeResolver(typeof(T));
    CreateRuleParseTrees(startSymbolName, parseTrees, typeResolver);

    // extract terminal names
    var simpleTerminalNames = new HashSet<string>();
    var complexTerminalNames = new HashSet<string>();
    foreach (var nonTerminalName in parseTrees.Keys)
      GetTerminals(parseTrees[nonTerminalName], simpleTerminalNames, complexTerminalNames);

    // create simple terminals
    var terminals = new Dictionary<string, Terminal>();
    CreateSimpleTerminals(simpleTerminalNames, terminals);

    // create complex terminals
    var valueConverters = new Dictionary<IAbstractSymbol, ValueConverter>();
    CreateComplexTerminals(complexTerminalNames, typeResolver, terminals, valueConverters);

    // create non terminals      
    var nonTerminals = new Dictionary<string, NonTerminal>();
    var nonTerminalHandlers = new Dictionary<NonTerminal, INonTerminalHandler>();
    CreateNonTerminals(parseTrees, typeResolver, nonTerminals, nonTerminalHandlers);

    // create production rules
    var symbols = new Symbols(terminals, nonTerminals);
    var rules = CreateRules(parseTrees, symbols, typeResolver);

    // create extra start rule (for GLR convention)
    var clientStartSymbol = new NonTerminal("@" + startSymbolName);
    var startProduction = new Production<IElement>(clientStartSymbol, nonTerminals[startSymbolName]);
    rules.Add(startProduction);

    // add dummy handler for extra start rule
    nonTerminalHandlers[clientStartSymbol] = new ForwardNonTerminalHandler(clientStartSymbol, rootType);

    var conflictHandler = new PriorityConflictHandler<ItemState, DefaultTreeNode>(false);

    var clientGrammar = new DefaultGrammar(rules, clientStartSymbol, EofTerminal, new HashSet<Terminal>(skipTerminals));
    AddInjectedNonTerminalHandlers(nonTerminalHandlers, clientGrammar.NonTerminals);
    CheckCompletness(nonTerminalHandlers, clientGrammar.NonTerminals);

    var clientTable = new DynamicCachingTable(clientGrammar);
    var parser = new ClientParser<T>(clientGrammar, clientTable, isRecoveryEnabled, conflictHandler);
    parser.TreeTransformer = TreeTransformerFactory.Create(clientGrammar, valueConverters, nonTerminalHandlers);
    return parser;
  }

  private static void AddInjectedNonTerminalHandlers(Dictionary<NonTerminal, INonTerminalHandler> handlers, IEnumerable<NonTerminal> allNonTerminals)
  {
    foreach (var nt in allNonTerminals.OfType<AnnotatedNonTerminal>())
    {
      if (!handlers.ContainsKey(nt))
      {
        var handler = new InheritContextNonTerminalHandler(nt);
        handlers[nt] = handler;
      }
    }
  }

  private static void CheckCompletness(Dictionary<NonTerminal, INonTerminalHandler> handlers, IEnumerable<NonTerminal> allNonTerminals)
  {
    foreach (var nt in allNonTerminals)
    {
      if (nt is DerivedNonTerminal) continue;
      if (!handlers.ContainsKey(nt))
        throw new GrammarException($"missing non terminal handler for {nt}");
    }
  }

  // -----------------------------------
  // Terminal and Nonterminal Extraction
  // -----------------------------------

  private static void CreateSimpleTerminals(ISet<string> simpleTerminalNames, Dictionary<string, Terminal> terminals)
  {
    var sortedSimpleTerminals = simpleTerminalNames.OrderBy(t => t.Length).ToList();
    for (int i = 0; i < sortedSimpleTerminals.Count; i++)
    {
      var terminalName = sortedSimpleTerminals[i];
      var priority = i + 1;
      terminals[terminalName] = new Terminal(terminalName, null, true, priority);
    }
  }

  private static void CreateComplexTerminals(ISet<string> complexTerminalNames, ITypeResolver typeResolver, Dictionary<string, Terminal> terminals, Dictionary<IAbstractSymbol, ValueConverter> valueConverters)
  {
    foreach (var terminalName in complexTerminalNames)
    {
      var terminalType = typeResolver.GetType(terminalName);
      if (terminalType == null)
        throw new GrammarException($"no type found for complex terminal '{terminalName}'");
      var terminalPattern = terminalType.GetCustomAttribute<PatternAttribute>();
      if (terminalPattern == null)
        throw new GrammarException($"terminal class '{terminalType.Name}' has no pattern attribute.");
      var currentTerminal = new Terminal(terminalName, terminalPattern.Value, terminalPattern.Priority);
      terminals[terminalName] = currentTerminal;
      var valueOfMethod = terminalType.GetMethod("ValueOf");
      if (valueOfMethod != null)
        valueConverters[currentTerminal] = (symbol, lexeme) => valueOfMethod.Invoke(null, new object[] { lexeme });
    }
  }

  private static void CreateNonTerminals(IDictionary<string, DefaultTreeNode> parseTrees, ITypeResolver typeResolver, Dictionary<string, NonTerminal> nonTerminals, Dictionary<NonTerminal, INonTerminalHandler> nonTerminalHandlers)
  {
    foreach (string nonTerminalName in parseTrees.Keys)
    {
      var nonTerminalType = typeResolver.GetType(nonTerminalName);
      var ruleBaseAttribute = nonTerminalType.GetCustomAttribute<RuleBaseAttribute>();

      // determine the priority (if defined)
      int priority = 0;
      if (ruleBaseAttribute is RuleAttribute ruleAttribute)
        priority = ruleAttribute.Priority;

      // determine the non terminal (either an annotated or a normal non terminal)
      NonTerminal currentNonTerminal;
      if (priority != 0)
      {
        var annotations = new HashSet<IAnnotation> { new PriorityAnnotation(PriorityAnnotationType.Priority, priority) };
        currentNonTerminal = new AnnotatedNonTerminal(nonTerminalName, annotations);
      }
      else
        currentNonTerminal = new NonTerminal(nonTerminalName);
      nonTerminals[nonTerminalName] = currentNonTerminal;

      // install handler for non terminal
      var ruleResultFactory = new DefaultRuleResultFactory(ruleBaseAttribute.GetInstanceTypeForType(nonTerminalType));
      var handler = new CreateInstanceNonTerminalHandler(currentNonTerminal, ruleResultFactory, ruleBaseAttribute.ConverterForType(nonTerminalType));
      nonTerminalHandlers[currentNonTerminal] = handler;
    }

  }

  private static void GetTerminals(DefaultTreeNode node, ISet<string> simpleTerminals, ISet<string> complexTerminals)
  {
    if (node.Symbol == Terminal)
    {
      string terminalName = GetTerminalName(node);
      if (node.Children.Count == 3) complexTerminals.Add(terminalName);
      else simpleTerminals.Add(terminalName);
    }
    else
    {
      if (node.Children != null)
      {
        foreach (DefaultTreeNode childNode in node.Children)
          GetTerminals(childNode, simpleTerminals, complexTerminals);
      }
    }
  }

  private sealed class TypeResolver : ITypeResolver
  {
    public TypeResolver(Type rootType)
    {
      _rootType = rootType;
    }

    public Type GetType(string symbolName)
    {
      Type depType = null;
      try
      {
        string fullName = null;
        if (_rootType.DeclaringType != null) fullName = $"{_rootType.DeclaringType.FullName}+{symbolName}";
        else fullName = $"{_rootType.Namespace}.{symbolName}";
        depType = _rootType.Assembly.GetTypes().FirstOrDefault(t => t.FullName == fullName);
      }
      catch (Exception)
      {
        // fail silently
      }

      if (depType == null)
        throw new GrammarException($"referenced non terminal '{symbolName}' not found");

      return depType;
    }

    private readonly Type _rootType;
  }


  // -------------

  private static void CreateRuleParseTrees(string startNonTerminal, Dictionary<string, DefaultTreeNode> parseTrees, ITypeResolver typeResolver)
  {
    HashSet<string> remainingTypes = new HashSet<string>();
    remainingTypes.Add(startNonTerminal);
    while (remainingTypes.Count > 0)
    {
      // get an unprocessed type from the to-do list
      string ntName = remainingTypes.First();
      remainingTypes.Remove(ntName);
      Type type = typeResolver.GetType(ntName);

      // get grammar rule from custom attribute (input for meta parser)
      var ruleAttr = type.GetCustomAttribute<RuleBaseAttribute>();
      if (ruleAttr == null) throw new GrammarException($"type '{type}' of nonterminal '{ntName}' has no rule attribute");
      string ruleDescription = ruleAttr.GetDescriptionForType(type);

      // parse meta rule
      var scannerSource = new ReaderScannerSource(type.Name, ruleDescription);
      var scanner = new KeywordScanner(EofTerminal, _grammar.Terminals, _skipTerminals);
      var treeNodeFactory = new DefaultTreeNodeFactory();
      var parser = new GlrParser<ItemState, DefaultTreeNode>(_table, treeNodeFactory);
      var result = parser.Parse(scanner.GetTokenSequence(scannerSource));

      // process parse result
      if (result.Trees.Length == 1)
      {
        // remember parse tree of this rule
        parseTrees[ntName] = result.Trees[0];

        // parse sub rules
        HashSet<string> subNtNames = new HashSet<string>();
        GetDependentNonterminals(result.Trees[0], subNtNames);
        foreach (string subNtName in subNtNames)
        {
          if (!parseTrees.ContainsKey(subNtName))
            remainingTypes.Add(subNtName);
        }
      }
    }
  }

  private static void GetDependentNonterminals(DefaultTreeNode node, ISet<string> nonTerminals)
  {
    if (node.Symbol == NonTerminal)
    {
      nonTerminals.Add(node.Children[0].Lexeme);
    }
    else
    {
      if (node.Children != null)
      {
        foreach (DefaultTreeNode childNode in node.Children)
          GetDependentNonterminals(childNode, nonTerminals);
      }
    }
  }

  private sealed class Symbols
  {
    public Symbols(IDictionary<string, Terminal> terminals, IDictionary<string, NonTerminal> nonTerminals)
    {
      _terminals = terminals;
      _nonTerminals = nonTerminals;
    }

    public IEnumerable<Terminal> Terminals => _terminals.Values;

    public Terminal GetTerminal(string name)
    {
      if (!_terminals.TryGetValue(name, out Terminal terminal))
        throw new GrammarException($"terminal '{name}' is not defined");

      return terminal;
    }

    public NonTerminal GetNonTerminal(string name)
    {
      if (!_nonTerminals.TryGetValue(name, out NonTerminal nonTerminal))
        throw new GrammarException($"nonterminal '{name}' is not defined");

      return nonTerminal;
    }

    private readonly IDictionary<string, Terminal> _terminals;
    private readonly IDictionary<string, NonTerminal> _nonTerminals;
  }



  // -----------------------
  // Context Grammar Handler
  // -----------------------

  private List<Production<IElement>> CreateRules(IDictionary<string, DefaultTreeNode> parseTrees, Symbols symbols, ITypeResolver typeResolver)
  {
    // create production rules
    List<Production<IElement>> rules = [];
    foreach (var nonTerminalName in parseTrees.Keys)
    {
      var ruleType = typeResolver.GetType(nonTerminalName);
      var production = CreateRule(parseTrees[nonTerminalName], ruleType, symbols, typeResolver);
      rules.Add(production);
    }
    return rules;
  }

  private Production<IElement> CreateRule(DefaultTreeNode node, Type ntType, Symbols symbols, ITypeResolver typeResolver)
  {
    AssertNodeSymbol(node, Rule);
    DefaultTreeNode ntNode = node.Children[0];
    IElement rhs = CreateItemAlternatives(ntNode, ntType, symbols, typeResolver);
    NonTerminal lhs = symbols.GetNonTerminal(ntType.Name);
    Production<IElement> production = new Production<IElement>(lhs, rhs);
    return production;
  }

  private ElementList<IElement> CreateItemSequence(DefaultTreeNode node, Type ntType, Symbols symbols, ITypeResolver typeResolver)
  {
    AssertNodeSymbol(node, ItemSequence);
    ElementList<IElement> result = new ElementList<IElement>();
    foreach (DefaultTreeNode childNode in node.Children)
      result.Add(CreateQuantifiedItem(childNode, ntType, symbols, typeResolver));
    return result;
  }

  private IElement CreatePrioItem(DefaultTreeNode node, Type ntType, Symbols symbols, ITypeResolver typeResolver)
  {
    AssertNodeSymbol(node, AnnotatedItem);
    if (node.Children.Count == 1)
      return CreateItem(node.Children[0], ntType, symbols, typeResolver);
    else
    {
      var quantifiedItemElement = CreateItem(node.Children[1], ntType, symbols, typeResolver);
      var annoSymbol = node.Children[0].Symbol;
      PriorityAnnotationType type;
      if (annoSymbol == ShortAnnotationTerminal) type = PriorityAnnotationType.Short;
      else if (annoSymbol == LongAnnotationTerminal) type = PriorityAnnotationType.Long;
      else throw new GrammarException($"unhandled priority symbol {annoSymbol}");
      var annotatedElementName = GetAnnotatedElementName(ntType);
      return new AnnotatedElement(annotatedElementName, quantifiedItemElement, new PriorityAnnotation(type));
    }
  }

  private string GetAnnotatedElementName(Type type)
  {
    _annotatedElementCount.TryGetValue(type, out int counter);
    counter++;
    _annotatedElementCount[type] = counter;
    return $"{type.Name}_A{counter}";
  }

  private IElement CreateItemAlternatives(DefaultTreeNode node, Type ntType, Symbols symbols, ITypeResolver typeResolver)
  {
    AssertNodeSymbol(node, ItemAlternatives);
    ElementList<IElement> result = new ElementList<IElement>();
    ElementList<IElement> seq = null;
    foreach (DefaultTreeNode childNode in node.Children)
    {
      if (childNode.Symbol == ItemSequence)
      {
        if (seq == null)
        {
          seq = new ElementList<IElement>();
          result.Add(seq);
        }
        seq.Add(CreateItemSequence(childNode, ntType, symbols, typeResolver));
      }
      else seq = null;
    }
    if (result.Count == 1) return result;
    return new AlternativeList(result.ToArray());
  }

  private IElement CreateItem(DefaultTreeNode node, Type ntType, Symbols symbols, ITypeResolver typeResolver)
  {
    AssertNodeSymbol(node, Item);
    DefaultTreeNode childNode = node.Children[0];
    IElement result = null;
    if (childNode.Symbol == Symbol) result = CreateSymbol(childNode, symbols);
    else if (childNode.Symbol == Assignment) result = CreateAssignment(childNode, ntType, symbols);
    else if (childNode.Symbol == Group) result = CreateGroup(childNode, ntType, symbols, typeResolver);
    else if (childNode.Symbol == NegatedTerminals) result = CreateNegatedTerminals(childNode, symbols);
    else throw new GrammarException($"unhandled node with symbol '{childNode.Symbol}'");
    return result;
  }

  private static AlternativeList CreateNegatedTerminals(DefaultTreeNode node, Symbols symbols)
  {
    var terminalNode = node.Children[1];
    var currentNegatedTerminals = new HashSet<Terminal>();
    if (terminalNode.Symbol == LeftParenTerminal)
    {
      for (int i = 2; i < node.Children.Count; i += 2)
      {
        terminalNode = node.Children[i];
        currentNegatedTerminals.Add(CreateTerminal(terminalNode, symbols));
      }
    }
    else
    {
      currentNegatedTerminals.Add(CreateTerminal(terminalNode, symbols));
    }

    // create list of terminals that are not negated
    var terminals = new List<IElement>();
    foreach (var currentTerminal in symbols.Terminals)
      if (!currentNegatedTerminals.Contains(currentTerminal))
        terminals.Add(currentTerminal);
    return new AlternativeList(terminals);
  }

  private static IElement CreateSymbol(DefaultTreeNode node, Symbols symbols)
  {
    AssertNodeSymbol(node, Symbol);
    DefaultTreeNode childNode = node.Children[0];
    IElement result = null;
    if (childNode.Symbol == Terminal) result = CreateTerminal(childNode, symbols);
    else if (childNode.Symbol == NonTerminal) result = CreateNonTerminal(childNode, symbols);
    return result;
  }

  private AnnotatedElement CreateAssignment(DefaultTreeNode node, Type ntType, Symbols symbols)
  {
    AssertNodeSymbol(node, Assignment);

    var subElement = CreateSymbol(node.Children[2], symbols);
    var targetFieldName = GetFieldName(node.Children[0]);
    var assignmentOperator = GetAssignmentOperator(node.Children[1]);

    var ruleAttribute = ntType.GetCustomAttribute<RuleBaseAttribute>();
    if (ruleAttribute == null) throw new GrammarException($"type '{ntType.FullName}' has no rule");
    var customType = ruleAttribute.GetInstanceTypeForType(ntType);

    var fieldInfo = customType.GetField(targetFieldName);
    var propertyInfo = customType.GetProperty(targetFieldName);
    if (fieldInfo == null && propertyInfo == null)
      throw new GrammarException($"type '{customType.Name}' has no field or property named '{targetFieldName}'");

    IAnnotation annotation = null;
    switch (assignmentOperator)
    {
      case AssignmentOperator.Equal:
        if (fieldInfo != null) annotation = new AssignmentParserAction(fieldInfo);
        else annotation = new AssignmentParserAction(propertyInfo);
        break;
      case AssignmentOperator.PlusEqual:
        if (fieldInfo != null) annotation = new AddParserAction(fieldInfo);
        else annotation = new AddParserAction(propertyInfo);
        break;
      case AssignmentOperator.OptionalEqual:
        if (fieldInfo != null) annotation = new ExistsParserAction(fieldInfo);
        else annotation = new ExistsParserAction(propertyInfo);
        break;
    }

    var annotatedElementName = GetAnnotatedElementName(ntType);
    return new AnnotatedElement(annotatedElementName, subElement, annotation);
  }

  private static AssignmentOperator GetAssignmentOperator(DefaultTreeNode node)
  {
    AssertNodeSymbol(node, PlusEqualsTerminal, EqualsTerminal, OptionalEqualsTerminal);
    if (node.Symbol == PlusEqualsTerminal) return AssignmentOperator.PlusEqual;
    if (node.Symbol == OptionalEqualsTerminal) return AssignmentOperator.OptionalEqual;
    return AssignmentOperator.Equal;
  }

  private IElement CreateQuantifiedItem(DefaultTreeNode node, Type ntType, Symbols symbols, ITypeResolver typeResolver)
  {
    AssertNodeSymbol(node, QantifiedItem);

    var result = CreatePrioItem(node.Children[0], ntType, symbols, typeResolver);
    if (node.Children.Count == 2)
    {
      var ruleQuantifier = GetQuantifier(node.Children[1]);
      switch (ruleQuantifier)
      {
        case QuantifierKind.OnceOrMore:
          result = new OnceOrMoreElement(result);
          break;
        case QuantifierKind.ZeroOrMore:
          result = new ZeroOrMoreElement(result);
          break;
        case QuantifierKind.Optional:
          result = new OptionalElement(result);
          break;
      }
    }
    return result;
  }

  private IElement CreateGroup(DefaultTreeNode node, Type ntType, Symbols symbols, ITypeResolver typeResolver)
  {
    AssertNodeSymbol(node, Group);
    return CreateItemAlternatives(node.Children[1], ntType, symbols, typeResolver);
  }

  private static QuantifierKind GetQuantifier(DefaultTreeNode node)
  {
    AssertNodeSymbol(node, Quantifier);
    var quantifierSymbol = node.Children[0].Symbol;
    if (quantifierSymbol == QestionmarkTerminal) return QuantifierKind.Optional;
    if (quantifierSymbol == AsteriskTerminal) return QuantifierKind.ZeroOrMore;
    if (quantifierSymbol == PlusTerminal) return QuantifierKind.OnceOrMore;
    throw new GrammarException($"invalid quantifier '{quantifierSymbol}'");
  }

  private static string GetFieldName(DefaultTreeNode node)
  {
    AssertNodeSymbol(node, FieldName);
    var name = node.Children[0].Lexeme;
    return name;
  }

  private static Terminal CreateTerminal(DefaultTreeNode node, Symbols symbols)
  {
    AssertNodeSymbol(node, Terminal);
    var name = GetTerminalName(node);
    var result = symbols.GetTerminal(name);
    return result;
  }

  private static string GetTerminalName(DefaultTreeNode node)
  {
    AssertNodeSymbol(node, Terminal);
    string name = null;
    if (node.Children.Count == 1)
    {
      name = node.Children[0].Lexeme;
      name = name.Substring(1, name.Length - 2);
    }
    else if (node.Children.Count == 3)
    {
      name = node.Children[1].Lexeme;
    }
    return name;
  }

  private static NonTerminal CreateNonTerminal(DefaultTreeNode node, Symbols symbols)
  {
    AssertNodeSymbol(node, NonTerminal);
    var name = node.Children[0].Lexeme;
    return symbols.GetNonTerminal(name);
  }

  private static void AssertNodeSymbol(DefaultTreeNode node, params IAbstractSymbol[] symbols)
  {
    foreach (var currentSymbol in symbols)
      if (node.Symbol == currentSymbol) return;
    throw new GrammarException("?");
  }

  private enum AssignmentOperator { Equal, PlusEqual, OptionalEqual };
  private enum QuantifierKind { Optional, OnceOrMore, ZeroOrMore }

  private readonly Dictionary<Type, int> _annotatedElementCount = [];

  private static DefaultGrammar _grammar;
  private static DynamicTable _table;
  private static readonly HashSet<Terminal> _skipTerminals = [];

  private static readonly NonTerminal Rule = new NonTerminal("rule");
  private static readonly NonTerminal FieldName = new NonTerminal("field-name");
  private static readonly NonTerminal Symbol = new NonTerminal("symbol");
  private static readonly NonTerminal NonTerminal = new NonTerminal("non-terminal");
  private static readonly NonTerminal Group = new NonTerminal("group");
  private static readonly NonTerminal Quantifier = new NonTerminal("quantifier");
  private static readonly NonTerminal Item = new NonTerminal("item");
  private static readonly NonTerminal QantifiedItem = new NonTerminal("quantified-item");
  private static readonly NonTerminal AnnotatedItem = new NonTerminal("annotated-item");
  private static readonly NonTerminal Terminal = new NonTerminal("terminal");
  private static readonly NonTerminal Assignment = new NonTerminal("assignment");
  private static readonly NonTerminal ItemSequence = new NonTerminal("item-sequence");
  private static readonly NonTerminal ItemAlternatives = new NonTerminal("item-alternatives");
  private static readonly NonTerminal NegatedTerminals = new NonTerminal("negated-terminals");

  private static readonly Terminal StrTerminal = new Terminal("string", "\'[^\']*\'");
  private static readonly Terminal IdentifierTerminal = new Terminal("identifier", "[_a-zA-Z][_a-zA-Z0-9]*", -2);
  private static readonly Terminal WhitespaceTerminal = new Terminal("whitespace", "[ \t\r\n]");
  private static readonly Terminal LeftParenTerminal = new Terminal("(", "\\(");
  private static readonly Terminal RightParenTerminal = new Terminal(")", "\\)");
  private static readonly Terminal AsteriskTerminal = new Terminal("*", "\\*");
  private static readonly Terminal PlusTerminal = new Terminal("+", "\\+");
  private static readonly Terminal LessTerminal = new Terminal("<", "\\<");
  private static readonly Terminal GreaterTerminal = new Terminal(">", "\\>");
  private static readonly Terminal PlusEqualsTerminal = new Terminal("+=", "\\+\\=");
  private static readonly Terminal OptionalEqualsTerminal = new Terminal("?=", "\\?\\=");
  private static readonly Terminal EqualsTerminal = new Terminal("=", "\\=");
  private static readonly Terminal QestionmarkTerminal = new Terminal("?", "\\?");
  private static readonly Terminal EofTerminal = new Terminal("eof");
  private static readonly Terminal PipeTerminal = new Terminal("|", "\\|");
  private static readonly Terminal NotTerminal = new Terminal("!", "\\!");
  private static readonly Terminal ShortAnnotationTerminal = new Terminal("@short");
  private static readonly Terminal LongAnnotationTerminal = new Terminal("@long");
}
