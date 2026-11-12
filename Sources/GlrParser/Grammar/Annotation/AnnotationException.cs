// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using System;

namespace Glr.Grammar.Annotations;

/// <summary>
/// An exception that is thrown when grammar <see cref="IAnnotation"/>s are not properly configured.
/// </summary>
public sealed class AnnotationException : Exception
{
  /// <summary>
  /// Creates a new exception.
  /// </summary>
  /// <param name="message">The description of the issue.</param>
  public AnnotationException(string message) : base(message) { }
}
