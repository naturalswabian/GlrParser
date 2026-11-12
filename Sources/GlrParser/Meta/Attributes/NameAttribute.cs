// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using System;

namespace Glr.Meta.Attributes;

/// <summary>
/// A name of a terminal that is used for enumeration literals.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class NameAttribute : Attribute
{
  /// <summary>
  /// Creates a new name attribute for an enumeration literal.
  /// </summary>
  /// <param name="value">The name of the literal.</param>
  public NameAttribute(string value)
  {
    Value = value;
  }

  /// <summary>
  /// The name of enumeration literal which is the also the terminal.
  /// </summary>
  public string Value { get; }
}
