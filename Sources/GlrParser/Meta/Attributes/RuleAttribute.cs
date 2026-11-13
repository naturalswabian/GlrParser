// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Meta.SymbolHandlers;
using System;
using System.Reflection;

namespace Glr.Meta.Attributes;

/// <summary>
/// Definition of a production rule.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RuleAttribute : RuleBaseAttribute
{
  /// <summary>
  /// Creates a new production rule annotation.
  /// </summary>
  /// <param name="value">The string that describes the right-hand side of the production rule.</param>
  /// <param name="createInstance">When <see langword="true"/>, an instance should be created of the type to which this attribute is attached to.</param>
  /// <param name="priority">The priority of this rule definition.</param>
  public RuleAttribute(string value, bool createInstance = true, int priority = 0)
  {
    _value = value;
    _createInstance = createInstance;
    Priority = priority;
  }

  /// <inheritdoc/>
  public override Type GetInstanceTypeForType(Type type)
  {
    if (_createInstance) return type;
    else return null;
  }

  /// <inheritdoc/>
  public override string GetDescriptionForType(Type type)
  {
    return _value;
  }

  /// <inheritdoc/>
  public override NonTerminalConverter ConverterForType(Type type)
  {
    NonTerminalConverter converter = null;
    MethodInfo valueOfMethod = type.GetMethod("ValueOf");
    if (valueOfMethod != null) converter = (object value) => valueOfMethod.Invoke(null, new object[] { value });
    return converter;
  }

  /// <summary>
  /// The priority of this rule.
  /// </summary>
  public int Priority { get; }

  private readonly bool _createInstance;
  private readonly string _value;
}
