// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using System;

namespace Glr.Meta.Factories;

/// <summary>
/// Default implementation of <see cref="IRuleResultFactory"/> that creates
/// non abstract syntax tree nodes based on a <see cref="Type"/>.
/// </summary>
public class DefaultRuleResultFactory : IRuleResultFactory
{
  /// <summary>
  /// Creates a new factory that creates instances of an object model.
  /// </summary>
  /// <param name="targetType">The type of the instances that are created by this factory.</param>
  public DefaultRuleResultFactory(Type targetType)
  {
    TargetType = targetType;
  }

  /// <inheritdoc/>
  public object CreateInstance(object context)
  {
    if (TargetType != null)
      return Activator.CreateInstance(TargetType);
    return null;
  }

  /// <inheritdoc/>
  public override string ToString() => $"new {TargetType.Name} instance";

  /// <inheritdoc/>
  public Type TargetType { get; }
}
