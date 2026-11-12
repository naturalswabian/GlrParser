// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements.Symbols;
using Glr.Meta.Actions;
using Glr.Tree;

namespace Glr.Meta.SymbolHandlers;

/// <summary>
/// A non-terminal handler that inherits its context.
/// </summary>
public class InheritContextNonTerminalHandler : INonTerminalHandler
{
  /// <summary>
  /// Creates a new non-terminal handler.
  /// </summary>
  /// <param name="nonTerminal">The non-terminal that identifies this handler.</param>
  public InheritContextNonTerminalHandler(NonTerminal nonTerminal)
  {
    NonTerminal = nonTerminal;
  }

  /// <inheritdoc/>
  public NonTerminal NonTerminal { get; }

  /// <inheritdoc/>
  public ParserActionContext Init(DefaultTreeNode currentNode, ParserActionContext parentContext)
  {
    return parentContext;
  }

  /// <inheritdoc/>
  public void ProcessChildValue(ParserActionContext context)
  {
    // ???
  }

  /// <inheritdoc/>
  public object Finalize(DefaultTreeNode currentNode, ParserActionContext context)
  {
    // return the last result
    return context.CurrentItem;
  }

  /// <inheritdoc/>
  public override string ToString() => "inherited context";
}
