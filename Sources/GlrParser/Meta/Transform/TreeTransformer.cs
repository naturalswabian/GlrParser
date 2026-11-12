// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements.Symbols;
using Glr.Meta.Actions;
using Glr.Meta.SymbolHandlers;
using Glr.Parser;
using Glr.Tree;
using System.Collections.Generic;
using System.Linq;

namespace Glr.Meta.Transform;

/// <summary>
/// Default, internal implementation of the <see cref="ITreeTransformer"/>.
/// </summary>
internal class TreeTransformer : ITreeTransformer
{
  public TreeTransformer(IDictionary<IAbstractSymbol, ValueConverter> valueConverters, IDictionary<NonTerminal, INonTerminalHandler> nonTerminalHandlers)
  {
    _valueConverters = valueConverters;
    _nonTerminalHandlers = nonTerminalHandlers;
  }

  /// <inheritdoc/>
  public object Transform(string resourceUri, DefaultTreeNode treeNode, IDictionary<object, IList<DefaultTreeNode>> mapping, object factoryContext)
  {
    var context = new ParserActionContext
    {
      Mapping = mapping,
      ResourceUri = resourceUri,
      RuleResultFactoryContext = factoryContext,
    };
    if (treeNode.IsTerminalNode) return ProcessTerminalNode(treeNode);
    else return ProcessNonTerminalNode(context, treeNode);
  }

  private object ProcessNonTerminalNode(ParserActionContext parentContext, DefaultTreeNode node)
  {
    // get non terminal handler
    NonTerminal nonTerminal = (NonTerminal)node.Symbol;
    if (!_nonTerminalHandlers.TryGetValue(nonTerminal, out INonTerminalHandler handler))
      throw new ParserException($"no non terminal handler defined for {nonTerminal}");

    // create child container
    var context = handler.Init(node, parentContext);

    // process children
    foreach (DefaultTreeNode childNode in node.Children)
      ProcessChildNode(context, childNode, handler);

    // final processing of the result
    return handler.Finalize(node, context);
  }

  private void ProcessChildNode(ParserActionContext context, DefaultTreeNode childNode, INonTerminalHandler handler)
  {
    var annotatedNonTerminal = childNode.Symbol as AnnotatedNonTerminal;
    if (annotatedNonTerminal != null)
      foreach (var parserAction in annotatedNonTerminal.Annotations.OfType<IParserAction>())
        parserAction.Execute(childNode, context);

    if (childNode.IsTerminalNode) context.CurrentItem = ProcessTerminalNode(childNode);
    else context.CurrentItem = ProcessNonTerminalNode(context, childNode);
    handler?.ProcessChildValue(context);

    if (annotatedNonTerminal != null)
      foreach (var parserAction in annotatedNonTerminal.Annotations.OfType<IParserAction>())
        parserAction.Finalize(childNode, context);
  }

  private object ProcessTerminalNode(DefaultTreeNode terminalNode)
  {
    object result = null;
    Terminal terminal = (Terminal)terminalNode.Symbol;
    if (_valueConverters.TryGetValue(terminal, out ValueConverter converter))
      result = converter(terminalNode.Symbol, terminalNode.Token.Lexeme);
    else
      result = terminalNode.Lexeme;
    return result;
  }

  private readonly IDictionary<NonTerminal, INonTerminalHandler> _nonTerminalHandlers;
  private readonly IDictionary<IAbstractSymbol, ValueConverter> _valueConverters;
}
