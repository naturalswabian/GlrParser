// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

namespace Glr.Parser.Conflict.Priority;

/// <summary>
/// The kind of a conflict that occurs while parsing.
/// </summary>
public enum ConflictType
{
  /// <summary>
  /// The same production rule is applied but a different number of tokens (terminals) has been consumed.
  /// </summary>
  Single,

  /// <summary>
  /// Parsing lead to multiple derivation trees using different productions.
  /// </summary>
  Multiple,

  /// <summary>
  /// The cause is not known.
  /// </summary>
  Unknown
}
