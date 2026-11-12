// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using System;

namespace Glr.Grammar;

/// <summary>
/// An exception which is thrown when a grammar that is used for parsing contains errors.
/// </summary>
public sealed class GrammarException : Exception
{
  /// <summary>
  /// Creates a new grammar related exception.
  /// </summary>
  /// <param name="message">The description of the issue.</param>
  public GrammarException(string message) : base(message) { }
}
