// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Parser.Table;
using Glr.Tree;

namespace Glr.Parser;

/// <summary>
/// The state of a parser while parsing.
/// </summary>
/// <typeparam name="S">The type of the underlying state machine states.</typeparam>
/// <typeparam name="T">The type of syntax tree nodes.</typeparam>
public class ParserNodeState<S, T>
  where S : ITableState
  where T : class, ITreeNode<T>
{
  /// <summary>
  /// Creates a new stack.
  /// </summary>
  /// <param name="stack">The current stack.</param>
  /// <param name="node">The current node.</param>
  public ParserNodeState(Stack<S, T> stack, T node)
  {
    Stack = stack;
    Node = node;
  }

  /// <summary>
  /// The current syntax tree node.
  /// </summary>
  public T Node { get; }

  /// <summary>
  /// The current stack of states.
  /// </summary>
  public Stack<S, T> Stack { get; }

  /// <inheritdoc/>
  public override string ToString()
  {
    return Node.ToString();
  }
}
