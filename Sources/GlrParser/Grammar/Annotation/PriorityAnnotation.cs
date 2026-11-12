// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Parser.Conflict.Priority;
using System.Text;

namespace Glr.Grammar.Annotations;

/// <summary>
/// An annotation used for defining priorities. These priorities are used by the <see cref="PriorityConflictHandler{S,T}"/>
/// when a conflict has to be resolved.
/// </summary>
public class PriorityAnnotation : IAnnotation
{
  /// <summary>
  /// The default priority used for all elements that do not have an explicit priority.
  /// </summary>
  public const int DefaultPriority = 0;

  /// <summary>
  /// Creates a new priority annotation of the given type using the given priority.
  /// </summary>
  /// <param name="type">The resoling strategy.</param>
  /// <param name="priority">The priority when considering this annotation.</param>
  public PriorityAnnotation(PriorityAnnotationType type, int priority)
  {
    Type = type;
    Priority = priority;
  }

  /// <summary>
  /// Creates a new priority annotation of the given type using the default priority.
  /// </summary>
  /// <param name="type">The resoling strategy.</param>
  public PriorityAnnotation(PriorityAnnotationType type) : this(type, DefaultPriority) { }

  /// <summary>
  /// The priority of this annotation.
  /// </summary>
  public int Priority { get; }

  /// <summary>
  /// The kind of the annotation.
  /// </summary>
  public PriorityAnnotationType Type { get; }

  /// <inheritdoc/>
  public override string ToString()
  {
    var result = new StringBuilder();
    result.Append('[');
    result.Append(Name);
    if (Type == PriorityAnnotationType.Priority)
      result.Append(Priority);
    result.Append(']');
    return result.ToString();
  }

  private string Name
  {
    get
    {
      string result = null;
      switch (Type)
      {
        case PriorityAnnotationType.Short: result = "@short"; break;
        case PriorityAnnotationType.Long: result = "@long"; break;
        case PriorityAnnotationType.Priority: result = "@prio"; break;
      }
      return result;
    }
  }
}
