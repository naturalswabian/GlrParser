// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Parser.Table;
using Glr.Tree;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Glr.Parser;

/// <summary>
/// A set of stacks.
/// </summary>
/// <typeparam name="S">The type of the state machine's states.</typeparam>
/// <typeparam name="T">The type of the syntax tree nodes.</typeparam>
public class StackSet<S, T> : IEnumerable<Stack<S, T>>
  where S : ITableState
  where T : class, ITreeNode<T>
{
  /// <summary>
  /// Creates a new stack set.
  /// </summary>
  public StackSet()
  {
    _stacks = new List<Stack<S, T>>();
  }

  /// <summary>
  /// Adds the given stacks to this set.
  /// </summary>
  /// <param name="set">The stacks that should be added.</param>
  public void Add(StackSet<S, T> set)
  {
    foreach (Stack<S, T> stack in set)
      AddStack(stack);
  }

  /// <summary>
  /// Adds the given stack to this set.
  /// </summary>
  /// <param name="newStack">The stack that should be added.</param>
  public void AddStack(Stack<S, T> newStack)
  {
    foreach (Stack<S, T> stack in _stacks)
    {
      if (!(stack.State.Equals(newStack.State) && stack.Node == newStack.Node))
        continue;

      if (newStack.Equals(stack)) return;
      StackSet<S, T> parents = stack.Parents;
      if (parents != null)
      {
        Debug.Assert(newStack.Parents != null);
        parents.Add(newStack.Parents);
        return;
      }
    }
    _stacks.Add(newStack);
  }

  /// <summary>
  /// Removes the given stack from this set.
  /// </summary>
  /// <param name="stack">The stack that should be removed.</param>
  public void RemoveStack(Stack<S, T> stack)
  {
    _stacks.Remove(stack);
  }

  /// <summary>
  /// The number of stacks that are contained within this set.
  /// </summary>
  public int Count => _stacks.Count;

  /// <summary>
  /// Gets the stack at the given index.
  /// </summary>
  /// <param name="index">The index of the desired stack.</param>
  /// <returns>The stack.</returns>
  public Stack<S, T> Get(int index)
  {
    return _stacks[index];
  }

  /// <inheritdoc/>
  public override bool Equals(object obj)
  {
    StackSet<S, T> stackSet = obj as StackSet<S, T>;
    if (stackSet != null) return _stacks.Equals(stackSet._stacks);
    return false;
  }

  /// <inheritdoc/>
  public override int GetHashCode()
  {
    return _stacks.GetHashCode();
  }

  /// <inheritdoc/>
  public override string ToString()
  {
    var result = new StringBuilder();
    foreach (var stack in _stacks)
      result.AppendLine(stack.ToString());
    return result.ToString();
  }

  /// <inheritdoc/>
  public IEnumerator<Stack<S, T>> GetEnumerator()
  {
    return _stacks.GetEnumerator();
  }

  /// <inheritdoc/>
  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }

  private readonly List<Stack<S, T>> _stacks;
}
