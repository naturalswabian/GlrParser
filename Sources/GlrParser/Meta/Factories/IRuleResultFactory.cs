// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using System;

namespace Glr.Meta.Factories;

/// <summary>
/// Factory that creates non abstract syntax tree nodes.
/// </summary>
public interface IRuleResultFactory
{
  /// <summary>
  /// Creates a new node for the corresponding non terminal.
  /// Returns <see langword="null"/>, if a new node could not be created.
  /// </summary>
  object CreateInstance(object context);

  /// <summary>
  /// The type of the elements that are created by this factory.
  /// </summary>
  Type TargetType { get; }
}
