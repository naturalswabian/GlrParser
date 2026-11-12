// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements.Symbols;
using Glr.Meta.Actions;
using Glr.Tree;
using System;

namespace Glr.Meta.SymbolHandlers;

/// <summary>
/// Forwards the current object as another object.
/// </summary>
public class ForwardNonTerminalHandler : INonTerminalHandler
{
  /// <summary>
  /// Creates a new forward handler.
  /// </summary>
  /// <param name="nonTerminal">The non-terminal which identifies this handler.</param>
  /// <param name="forwardedType">The target type.</param>
  public ForwardNonTerminalHandler(NonTerminal nonTerminal, Type forwardedType)
  {
    NonTerminal = nonTerminal ?? throw new ArgumentNullException(nameof(nonTerminal));
    _forwardedType = forwardedType ?? throw new ArgumentNullException(nameof(forwardedType));
  }

  /// <inheritdoc/>
  public NonTerminal NonTerminal { get; }

  /// <inheritdoc/>
  public ParserActionContext Init(DefaultTreeNode currentNode, ParserActionContext parentContext)
  {
    // the parent context is simply forwarded
    return parentContext;
  }

  /// <inheritdoc/>
  public void ProcessChildValue(ParserActionContext context)
  {
    // keep track of the last processed child value
    context.Data = context.CurrentItem;
  }

  /// <inheritdoc/>
  public object Finalize(DefaultTreeNode currentNode, ParserActionContext context)
  {
    // return the forwarded value
    return context.Data;
  }

  /// <inheritdoc/>
  public override string ToString() => $"forward as {_forwardedType.Name}";

  private readonly Type _forwardedType;
}
