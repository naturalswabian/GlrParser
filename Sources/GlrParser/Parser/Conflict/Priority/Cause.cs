// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Tree;

namespace Glr.Parser.Conflict.Priority;

/// <summary>
/// The description of conflict cause.
/// </summary>
/// <typeparam name="T">The type of the nodes that caused the conflict.</typeparam>
public class Cause<T> where T : ITreeNode<T>
{
  /// <summary>
  /// Creates a new conflict cause.
  /// </summary>
  /// <param name="type">The kind of the conflict.</param>
  public Cause(ConflictType type) : this(default, default, type) { }

  /// <summary>
  /// Creates a new conflict cause.
  /// </summary>
  /// <param name="sourceNode">The source node that caused the conflict.</param>
  /// <param name="secondaryNode">The secondary node that also caused the conflict.</param>
  /// <param name="type">The kind of the conflict.</param>
  public Cause(T sourceNode, T secondaryNode, ConflictType type)
  {
    SourceNode = sourceNode;
    SecondaryNode = secondaryNode;
    Type = type;
  }

  /// <summary>
  /// The second node that has conflict with <see cref="SourceNode"/>.
  /// </summary>
  public T SecondaryNode { get; }

  /// <summary>
  /// The first node that has conflict with <see cref="SecondaryNode"/>.
  /// </summary>
  public T SourceNode { get; }

  /// <summary>
  /// The kind of the conflict.
  /// </summary>
  public ConflictType Type { get; }

  /// <inheritdoc/>
  public override string ToString()
  {
    return $"{Type}: {SourceNode}, {SecondaryNode}";
  }
}
