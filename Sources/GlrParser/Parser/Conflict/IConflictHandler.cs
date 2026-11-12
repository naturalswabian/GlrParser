// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Parser.Table;
using Glr.Tree;
using System.Collections.Generic;

namespace Glr.Parser.Conflict;

/// <summary>
/// A handler that is able to resolve a conflict while parsing.
/// </summary>
/// <typeparam name="S">The type of the state machine table.</typeparam>
/// <typeparam name="T">The type of the syntax tree nodes.</typeparam>
public interface IConflictHandler<S, T>
  where S : class, ITableState
  where T : class, ITreeNode<T>
{
  /// <summary>
  /// Resolves the given conflict.
  /// </summary>
  /// <param name="entries">The entries that causes the conflict.</param>
  /// <param name="parser">The parser that is used.</param>
  void ResolveConflict(IList<ParserNodeState<S, T>> entries, GlrParser<S, T> parser);
}
