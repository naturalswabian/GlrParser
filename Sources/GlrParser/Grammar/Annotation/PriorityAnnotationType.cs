// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

namespace Glr.Grammar.Annotations;

/// <summary>
/// Definition of different conflict resolving strategies.
/// </summary>
public enum PriorityAnnotationType
{
  /// <summary>
  /// The shorter version is preferred.
  /// </summary>
  Short = 1,

  /// <summary>
  /// The longer version is preferred.
  /// </summary>
  Long = 2,

  /// <summary>
  /// The version with the higher priority is preferred.
  /// </summary>
  Priority = 4
}
