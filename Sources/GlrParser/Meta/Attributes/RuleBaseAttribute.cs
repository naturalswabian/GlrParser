// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Meta.SymbolHandlers;
using System;

namespace Glr.Meta.Attributes;

/// <summary>
/// The base for annotations that describe meta grammars in-line in code.
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
public abstract class RuleBaseAttribute : Attribute
{
  /// <summary>
  /// Gets the instance type for the given type.
  /// </summary>
  public abstract Type GetInstanceTypeForType(Type type);

  /// <summary>
  /// Gets the formal grammar description for the given type.
  /// </summary>
  public abstract string GetDescriptionForType(Type type);

  /// <summary>
  /// Gets the converter for the given type.
  /// </summary>
  public abstract NonTerminalConverter ConverterForType(Type type);
}
