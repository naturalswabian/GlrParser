// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Annotations;
using System.Collections.Generic;
using System.Text;

namespace Glr.Grammar.Elements.Symbols;

/// <summary>
/// A non-terminal that has annotations associated to it.
/// </summary>
public class AnnotatedNonTerminal : InjectedNonTerminal
{
  /// <summary>
  /// Creates a new annotated non-terminal.
  /// </summary>
  /// <param name="name">The name of the annotated non-terminal.</param>
  /// <param name="annotations">The set of annotations that are associated to it.</param>
  public AnnotatedNonTerminal(string name, ISet<IAnnotation> annotations)
    : base(name)
  {
    Annotations = annotations;
  }

  /// <summary>
  /// The annotations of this non-terminal.
  /// </summary>
  public ISet<IAnnotation> Annotations { get; }

  /// <inheritdoc/>
  public override string ToString()
  {
    var result = new StringBuilder();
    foreach (var annotation in Annotations)
    {
      result.Append(annotation);
      result.Append(' ');
    }
    result.Append(Name);
    return result.ToString();
  }
}
