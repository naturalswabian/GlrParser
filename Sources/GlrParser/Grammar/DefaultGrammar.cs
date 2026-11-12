// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements;
using Glr.Grammar.Elements.Symbols;
using Glr.Grammar.Normalizing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glr.Grammar;

/// <summary>
/// The default implementation of the <see cref="IGrammar"/>.
/// </summary>
public class DefaultGrammar : IGrammar
{
  /// <summary>
  /// Creates a new grammar.
  /// </summary>
  /// <param name="rules">The production rule that contributes to the new grammar.</param>
  /// <param name="startSymbol">The non terminal that identifies the root production rule.</param>
  /// <param name="eofSymbol">The terminal that is used as end-of-file marker.</param>
  /// <param name="skipTerminals">The terminals that are skipped while parsing.</param>
  /// <param name="specialTerminals">The terminals that are skipped while parsing but kept within the syntax tree.</param>
  public DefaultGrammar(IList<Production<IElement>> rules, NonTerminal startSymbol, Terminal eofSymbol, ISet<Terminal> skipTerminals, ISet<Terminal> specialTerminals = null)
  {
    SkipTerminals = skipTerminals;
    SpecialTerminals = specialTerminals;
    CreateGrammar(rules, startSymbol, eofSymbol);
  }

  /// <inheritdoc/>
  public IList<Production<IAbstractSymbol>> Productions { get; private set; }

  /// <inheritdoc/>
  public ISet<Terminal> SkipTerminals { get; }

  /// <inheritdoc/>
  public ISet<Terminal> SpecialTerminals { get; }

  /// <inheritdoc/>
  public NonTerminal StartSymbol { get; private set; }

  /// <inheritdoc/>
  public Terminal EofSymbol { get; private set; }

  /// <inheritdoc/>
  public ISet<Terminal> Terminals { get; private set; }

  /// <inheritdoc/>
  public ISet<NonTerminal> NonTerminals { get; private set; }

  /// <inheritdoc/>
  public IProduction<IAbstractSymbol> StartProduction { get; private set; }

  /// <inheritdoc/>
  public bool AllowsEmptyInput { get; private set; }

  /// <inheritdoc/>
  public ISet<Terminal> First(IList<IAbstractSymbol> symbols)
  {
    var result = new HashSet<Terminal>();
    foreach (var symbol in symbols)
    {
      var terminal = symbol as Terminal;
      var nonTerminal = symbol as NonTerminal;

      if (terminal != null)
        result.Add(terminal);
      else if (nonTerminal != null && _lookAheadCache.TryGetValue(nonTerminal, out ISet<Terminal> lookAhead))
        result.UnionWith(lookAhead);

      if (result.Contains(EpsilonSymbol)) result.Remove(EpsilonSymbol);
      else break;
    }
    if (result.Count == 0)
      result.Add(EpsilonSymbol);
    return result;
  }

  /// <inheritdoc/>
  public Terminal GetTerminal(string name)
  {
    return Terminals.FirstOrDefault(t => t.Name == name);
  }

  /// <inheritdoc/>
  public NonTerminal GetNonTerminal(string name)
  {
    return NonTerminals.FirstOrDefault(t => t.Name == name);
  }

  /// <inheritdoc/>
  public Terminal EpsilonSymbol { get; private set; }

  /// <inheritdoc/>
  public override string ToString()
  {
    var result = new StringBuilder();
    for (int i = 0; i < Productions.Count; i++)
    {
      result.Append(Productions[i]);
      result.AppendLine();
    }
    return result.ToString();
  }

  /// <inheritdoc/>
  public IAbstractSymbol GetSymbol(string name)
  {
    IAbstractSymbol res = GetTerminal(name);
    if (res == null) res = GetNonTerminal(name);
    if (res == null) throw new InvalidOperationException(name);
    return res;
  }

  /// <inheritdoc/>
  public override bool Equals(object obj)
  {
    if (obj == this) return true;
    if (!(obj is DefaultGrammar otherGrammar)) return false;
    if (otherGrammar.Productions.Count != Productions.Count) return false;

    foreach (var production in Productions)
    {
      bool found = otherGrammar.Productions.Any(p => production.Equals(p));
      if (!found) return false;
    }
    return true;
  }

  /// <inheritdoc/>
  public override int GetHashCode()
  {
    int result = 0;
    foreach (var production in Productions)
      result += production.GetHashCode();
    return result;
  }


  /// <summary>
  /// Creates the grammar.
  /// </summary>
  protected void CreateGrammar(IList<Production<IElement>> rules, NonTerminal startSymbol, Terminal eofSymbol)
  {
    // sanity check
    StartSymbol = startSymbol ?? throw new ArgumentException("start symbol not defined");
    EofSymbol = eofSymbol ?? throw new ArgumentException("end of file symbol not defined");
    if (rules == null) throw new ArgumentException("productions not defined");

    // normalize productions
    var normalizedProductions = new List<Production<IAbstractSymbol>>();
    var normalizer = new Normalizer();
    foreach (var production in rules)
      normalizedProductions.AddRange(normalizer.Normalize(production));

    // create the final list of production rules while avoiding duplicates
    Productions = new List<Production<IAbstractSymbol>>();
    foreach (var production in normalizedProductions)
    {
      if (!Productions.Any(p => Equals(p, production)))
        Productions.Add(production);
    }

    AdaptEmptyInput();
    StartProduction = Productions.FirstOrDefault(p => p.LeftSideSymbol == StartSymbol);
    CreateTerminalsAndNonTerminals();
    CreateEpsilon();
    CreateLookAheadCache();
    Validate();
  }

  private void AdaptEmptyInput()
  {
    RemoveEmptyRule(StartSymbol);
    AdaptSecondLevelProduction();
  }

  private void AdaptSecondLevelProduction()
  {
    var nonEmptyStartRule = Productions.FirstOrDefault(p => p.LeftSideSymbol == StartSymbol && p.RightHandSide.Count == 1);
    if (nonEmptyStartRule == null)
      return;

    // do nothing if the actual start production (not the real one) is used
    // any where beside of the start rule
    var secondLevelStartSymbol = nonEmptyStartRule.RightHandSide[0];
    if (!(secondLevelStartSymbol is NonTerminal secondLevelNonTerminal))
      return;
    foreach (var rule in Productions)
    {
      if (rule == nonEmptyStartRule) continue;
      if (rule.RightHandSide.Contains(secondLevelStartSymbol)) return;
    }

    // it is safe here to remove the second level epsilon start production
    RemoveEmptyRule(secondLevelNonTerminal);
  }

  private void RemoveEmptyRule(NonTerminal leftHandSide)
  {
    var emptyStartRule = Productions.FirstOrDefault(p => p.LeftSideSymbol == leftHandSide && p.RightHandSide.Count == 0);
    if (emptyStartRule == null)
      return;

    // mark this grammar that it allows empty inputs
    AllowsEmptyInput = true;

    // remove the start epsilon rule since it is not needed for parsing
    Productions.Remove(emptyStartRule);
  }

  /// <summary>
  /// Creates the epsilon terminal.
  /// </summary>
  private void CreateEpsilon()
  {
    var epsilonName = new StringBuilder("\u03b5");
    bool containsName;
    do
    {
      var currentName = epsilonName.ToString();
      containsName = Terminals.OfType<IAbstractSymbol>().Concat(NonTerminals).Any(e => e.Name == currentName);
      if (containsName) epsilonName.Append('_');
    } while (containsName);

    EpsilonSymbol = new Terminal(epsilonName.ToString(), "");
  }

  private void CreateLookAheadCache()
  {
    _lookAheadCache = new Dictionary<NonTerminal, ISet<Terminal>>();
    foreach (NonTerminal symbol in NonTerminals)
    {
      HashSet<Terminal> firstSet = new HashSet<Terminal>();
      HashSet<NonTerminal> ntSet = new HashSet<NonTerminal>();
      First(symbol, firstSet, ntSet);
      _lookAheadCache.Add(symbol, firstSet);
    }
  }

  private void CreateTerminalsAndNonTerminals()
  {
    NonTerminals = new HashSet<NonTerminal>();
    Terminals = new HashSet<Terminal>();

    foreach (var production in Productions)
    {
      NonTerminals.Add(production.LeftSideSymbol);
      foreach (var terminal in production.RightHandSide.OfType<Terminal>())
        Terminals.Add(terminal);
    }

    Terminals.Add(EofSymbol);
  }

  private void First(IAbstractSymbol symbol, ISet<Terminal> firstSet, ISet<NonTerminal> ntSet)
  {
    switch (symbol)
    {
      case NonTerminal nonTerminal:
        if (ntSet.Add(nonTerminal))
          FirstNonTerminal(nonTerminal, firstSet, ntSet);
        break;
      case Terminal terminal:
        firstSet.Add(terminal);
        break;
    }
  }

  private void FirstNonTerminal(NonTerminal nonTerminal, ISet<Terminal> firstSet, ISet<NonTerminal> ntSet)
  {
    foreach (IElementList<IAbstractSymbol> rightHandSide in Productions
      .Where(production => production.LeftSideSymbol == nonTerminal)
      .Select(production => production.RightHandSide))
    {
      if (rightHandSide.Count > 0)
      {
        for (int i = 0; i < rightHandSide.Count; i++)
        {
          First(rightHandSide[i], firstSet, ntSet);
          if (firstSet.Contains(EpsilonSymbol) && i != rightHandSide.Count - 1)
            firstSet.Remove(EpsilonSymbol);
          else break;
        }
      }
      else firstSet.Add(EpsilonSymbol);
    }
  }

  /// <summary>
  /// Private test. Do not use.
  /// </summary>
  public bool ReportUnreachableProductions { get; } = false;

  #region Validation

  private void Validate()
  {
    ValidateStartProduction();
    ValidateNonEmptyProductions();
    ValidateDuplicateSymbols();
    ValidateUndefinedNonTerminals();
    ValidateUnreachableProductions();
  }

  /// <summary>
  /// Checks if the start production has exactly one single element in the right-hand-side.
  /// </summary>
  private void ValidateStartProduction()
  {
    if (StartProduction.RightHandSide.Count != 1)
      throw new GrammarException("illegal start production rule: " + StartProduction);
  }

  /// <summary>
  /// Checks if all production have non-empty right-hand-side.
  /// </summary>
  private void ValidateNonEmptyProductions()
  {
    foreach (var production in Productions)
      if (production.RightHandSide.Count == 0)
        throw new GrammarException("empty right side in production rule: " + production);
  }

  /// <summary>
  /// Checks if all terminals and symbols have distinct names.
  /// </summary>
  private void ValidateDuplicateSymbols()
  {
    var duplicates = new HashSet<string>(NonTerminals.Select(n => n.Name).Intersect(Terminals.Select(t => t.Name)));
    if (duplicates.Count > 0)
    {
      var symbolNames = string.Join(", ", duplicates);
      throw new GrammarException($"symbol defined as nonterminal and terminal: {symbolNames}");
    }
  }

  /// <summary>
  /// Check if all non terminals are defined by a production rule.
  /// </summary>
  private void ValidateUndefinedNonTerminals()
  {
    var nonTerminals = new HashSet<NonTerminal>();
    foreach (var production in Productions)
      nonTerminals.UnionWith(production.RightHandSide.OfType<NonTerminal>());
    nonTerminals.ExceptWith(Productions.Select(p => p.LeftSideSymbol));
    if (nonTerminals.Count != 0)
      throw new GrammarException("no production rule for non-terminal(s): " + ToString(nonTerminals));
  }

  /// <summary>
  /// Checks if all productions can be reached from the start production.
  /// </summary>
  private void ValidateUnreachableProductions()
  {
    var definedNonTerminals = new HashSet<NonTerminal>(Productions.Select(p => p.LeftSideSymbol));
    var reachableNonTerminals = new HashSet<NonTerminal> { StartSymbol };

    int lastReachableCount = -1;
    while (lastReachableCount != reachableNonTerminals.Count)
    {
      lastReachableCount = reachableNonTerminals.Count;

      IEnumerable<Production<IAbstractSymbol>> reachableProduction = Productions
        .Where(production => reachableNonTerminals.Contains(production.LeftSideSymbol));

      foreach (IProduction<IAbstractSymbol> production in reachableProduction)
        reachableNonTerminals.UnionWith(production.RightHandSide.OfType<NonTerminal>());
    }

    definedNonTerminals.ExceptWith(reachableNonTerminals);
    if (definedNonTerminals.Count != 0 && ReportUnreachableProductions)
      throw new GrammarException("unreachable production rule with left-hand sides: " + ToString(definedNonTerminals));

  }

  #endregion


  private static string ToString<T>(IEnumerable<T> elements)
  {
    StringBuilder result = new StringBuilder();
    bool isFirst = true;
    foreach (T element in elements)
    {
      if (!isFirst) result.Append(", ");
      else isFirst = false;
      result.Append(element);
    }
    return result.ToString();
  }

  private Dictionary<NonTerminal, ISet<Terminal>> _lookAheadCache;
}
