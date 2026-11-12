// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Annotations;
using Glr.Tree;

namespace Glr.Meta.Actions;

/// <summary>
/// A parser action is an operation that is performed during parsing.
/// </summary>
public interface IParserAction : IAnnotation
{
  /// <summary>
  /// Executes this action on the given context.
  /// </summary>
  void Execute(DefaultTreeNode currentNode, ParserActionContext context);

  /// <summary>
  /// Finalizes the processing the given context.
  /// </summary>
  void Finalize(DefaultTreeNode currentNode, ParserActionContext context);
}
