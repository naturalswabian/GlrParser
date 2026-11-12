// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements.Symbols;
using Glr.Meta.Actions;
using Glr.Tree;

namespace Glr.Meta;

/// <summary>
/// When an abstract tree node is processed the following steps are performed:
/// 1. <see cref="Init(DefaultTreeNode,ParserActionContext)"/>
/// 2. Multiple <see cref="ProcessChildValue(ParserActionContext)"/> calls
/// 3. <see cref="Finalize(DefaultTreeNode,ParserActionContext)"/>
/// </summary>
public interface INonTerminalHandler
{
  /// <summary>
  /// The non terminal for which this handler is applied to.
  /// </summary>
  NonTerminal NonTerminal { get; }

  /// <summary>
  /// Initializes the context that should be used for the given parent context.
  /// </summary>
  ParserActionContext Init(DefaultTreeNode currentNode, ParserActionContext parentContext);

  /// <summary>
  /// Handles the child value.
  /// </summary>
  void ProcessChildValue(ParserActionContext context);

  /// <summary>
  /// Finalizes this context and returns the value that shall be passed to the parent context.
  /// </summary>
  object Finalize(DefaultTreeNode currentNode, ParserActionContext context);
}
