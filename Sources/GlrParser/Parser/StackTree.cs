// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Parser.Table;
using Glr.Tree;

namespace Glr.Parser;

/// <summary>
/// A tree of stacks.
/// </summary>
/// <typeparam name="S">The type of the state machine's states.</typeparam>
/// <typeparam name="T">The type of the syntax tree nodes.</typeparam>
public class StackTree<S, T>
  where S : ITableState
  where T : class, ITreeNode<T>
{
  /// <summary>
  /// Creates a new child stack tree.
  /// </summary>
  /// <param name="stack">The current stack - the value of the tree node.</param>
  /// <param name="parent">The parent stack tree.</param>
  public StackTree(Stack<S, T> stack, StackTree<S, T> parent)
  {
    Stack = stack;
    Parent = parent;
  }

  /// <summary>
  /// Creates a new root stack tree.
  /// </summary>
  /// <param name="stack">The current stack - the value of the tree node.</param>
  public StackTree(Stack<S, T> stack) : this(stack, null) { }

  /// <summary>
  /// The parent tree of this tree node.
  /// </summary>
  public StackTree<S, T> Parent { get; }

  /// <summary>
  /// The current stack, the tree node's value.
  /// </summary>
  public Stack<S, T> Stack { get; }
}
