// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Parser.Table;
using Glr.Tree;
using System.Collections.Generic;
using System.Diagnostics;

namespace Glr.Parser;

/// <summary>
/// A stack of table states.
/// </summary>
/// <typeparam name="S">The type of the table states.</typeparam>
/// <typeparam name="T">The type of the syntax tree nodes.</typeparam>
public class Stack<S, T>
    where S : ITableState
    where T : class, ITreeNode<T>
{
  /// <summary>
  /// Creates a new stack with the given state.
  /// </summary>
  /// <param name="state">The state.</param>
  public Stack(S state) : this(state, default, null) { }

  private Stack(S state, T node, StackSet<S, T> parents)
  {
    State = state;
    Parents = parents;
    Node = node;

    // calculate the hash code
    const int hash = 47;
    _hashCode = State.GetHashCode();
    if (Node != null) _hashCode = _hashCode * hash + Node.GetHashCode();
    if (Parents != null) _hashCode = _hashCode * hash + Parents.GetHashCode();
  }

  /// <summary>
  /// The parents of this stack.
  /// </summary>
  public StackSet<S, T> Parents { get; }

  /// <summary>
  /// The underlying state.
  /// </summary>
  public S State { get; }

  /// <summary>
  /// The syntax tree node of this stack.
  /// </summary>
  public T Node { get; }

  /// <summary>
  /// Pushes the given state and node into a new the stack that is based on this stack.
  /// </summary>
  /// <param name="state">The new state.</param>
  /// <param name="node">The new node.</param>
  /// <returns>The new stack that has the state and node pushed to it.</returns>
  public Stack<S, T> Push(S state, T node)
  {
    StackSet<S, T> newParents = new StackSet<S, T>();
    newParents.AddStack(new Stack<S, T>(State, node, Parents));
    return new Stack<S, T>(state, default(T), newParents);
  }

  /// <summary>
  /// Pops the given number of states from this stack (reduce).
  /// </summary>
  /// <param name="count">The number of states that should be popped.</param>
  /// <returns>The resulting stack trees.</returns>
  public IList<StackTree<S, T>> Pop(int count)
  {
    Debug.Assert(count >= 1);
    List<StackTree<S, T>> result = new List<StackTree<S, T>>();
    foreach (Stack<S, T> stack in Parents)
    {
      if (count > 1)
      {
        var list = stack.Pop(count - 1);
        foreach (StackTree<S, T> tree in list)
        {
          StackTree<S, T> newTree = new StackTree<S, T>(stack, tree);
          result.Add(newTree);
        }
      }
      else
      {
        result.Add(new StackTree<S, T>(stack));
      }
    }
    return result;
  }

  /// <inheritdoc/>
  public override bool Equals(object obj)
  {
    if (obj == null) return false;
    Stack<S, T> stack = obj as Stack<S, T>;
    if (stack.GetHashCode() != _hashCode) return false;
    bool parentsEqual =
        (Parents == null && stack.Parents == null) ||
        ((Parents != null) && Parents.Equals(stack.Parents));
    bool nodesEqual = stack.Node == (Node);
    return State.Equals(stack.State) && parentsEqual && nodesEqual;
  }

  /// <inheritdoc/>
  public override int GetHashCode()
  {
    return _hashCode;
  }

  /// <inheritdoc/>
  public override string ToString()
  {
    return State.ToString();
  }

  private readonly int _hashCode;
}
