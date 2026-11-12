// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using System.Collections.Generic;

namespace Glr.Grammar.Annotations;

/// <summary>
/// A factory that creates instances of <see cref="IAnnotation"/>s.
/// </summary>
public interface IAnnotationFactory
{
  /// <summary>
  /// Creates a new <see cref="IAnnotation"/>.
  /// </summary>
  /// <param name="id">The identifier of the new <see cref="IAnnotation"/>.</param>
  /// <param name="args">Additional arguments for the new <see cref="IAnnotation"/>.</param>
  /// <returns>The new <see cref="IAnnotation"/></returns>
  IAnnotation CreateAnnotation(string id, List<string> args);
}
