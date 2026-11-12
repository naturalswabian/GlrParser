// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar;
using Glr.Grammar.Elements;
using Glr.Scanner;
using System.Collections.Generic;

namespace Glr.Tree;

/// <summary>
/// Creates syntax tree nodes for terminals and non-terminals.
/// </summary>
/// <typeparam name="T">The type of the nodes that are created by this factory.</typeparam>
public interface ITreeNodeFactory<T> where T : ITreeNode<T>
{
  /// <summary>
  /// Create a new non-terminal node.
  /// </summary>
  /// <param name="production">The production rule that has been used for the new node.</param>
  /// <param name="children">The children of this node.</param>
  /// <returns>The new non-terminal node.</returns>
  T CreateNonTerminalNode(IProduction<IElement> production, IList<T> children);

  /// <summary>
  /// Create a new terminal node.
  /// </summary>
  /// <param name="token">The (terminal) token of this node.</param>
  /// <returns>The new terminal node.</returns>
  T CreateTerminalNode(Token token);
}
