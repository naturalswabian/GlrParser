// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Parser.Table;
using Glr.Tree;
using System.Collections.Generic;

namespace Glr.Parser.Conflict;

/// <summary>
/// The default implementation of the <see cref="IConflictHandler{S, T}"/> which does no
/// resolving at all but reports an error.
/// </summary>
/// <typeparam name="S">The type of the state machine table.</typeparam>
/// <typeparam name="T">The type of the syntax tree nodes.</typeparam>
public class DefaultConflictHandler<S, T> : IConflictHandler<S, T>
    where S : class, ITableState
    where T : class, ITreeNode<T>
{
  /// <inheritdoc/>
  public void ResolveConflict(IList<ParserNodeState<S, T>> entries, GlrParser<S, T> parser)
  {
    var nodes = new T[entries.Count];
    for (int i = 0; i < entries.Count; i++)
      nodes[i] = entries[i].Node;
    throw new ConflictException<T>("multiple derivation trees", nodes, null);
  }
}
