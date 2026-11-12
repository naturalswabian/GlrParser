// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using System;

namespace Glr.Meta.Attributes;

/// <summary>
/// The pattern defines a regular expression pattern for terminals.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PatternAttribute : Attribute
{
  /// <summary>
  /// Creates a new pattern attribute.
  /// </summary>
  /// <param name="value">The regular expression pattern.</param>
  /// <param name="prio">The priority of this pattern.</param>
  public PatternAttribute(string value, int prio = 0)
  {
    Value = value;
    Priority = prio;
  }

  /// <summary>
  /// The regular expression of this pattern.
  /// </summary>
  public string Value { get; }

  /// <summary>
  /// The priority of this pattern.
  /// </summary>
  public int Priority { get; }
}
