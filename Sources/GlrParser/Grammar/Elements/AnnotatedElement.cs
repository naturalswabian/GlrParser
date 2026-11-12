// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Annotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Glr.Grammar.Elements;

/// <summary>
/// A grammar element that is annotated by some meta information.
/// </summary>
public class AnnotatedElement : IElement
{
  /// <summary>
  /// Creates a new annotated element.
  /// </summary>
  /// <param name="name">The name of the annotated element.</param>
  /// <param name="child">The element that is annotated.</param>
  /// <param name="annotation">The annotation of this element.</param>
  public AnnotatedElement(string name, IElement child, IAnnotation annotation)
  {
    Name = name;
    Element = child ?? throw new ArgumentNullException(nameof(child));
    Annotations = new HashSet<IAnnotation>();
    Annotations.Add(annotation);
    NormalizeAnnotations();
  }

  /// <inheritdoc/>
  public string Name { get; }

  /// <inheritdoc/>
  public override string ToString()
  {
    StringBuilder result = new StringBuilder();
    result.Append(Name);
    foreach (IAnnotation annotation in Annotations)
    {
      result.Append(' ');
      result.Append(annotation.ToString());
    }
    return result.ToString();
  }


  private void NormalizeAnnotations()
  {
    if (Element is AnnotatedElement annotatedElement)
    {
      Annotations.UnionWith(annotatedElement.Annotations);
      Element = annotatedElement.Element;
    }
  }

  /// <summary>
  /// The set of <see cref="IAnnotation"/>s of this element.
  /// </summary>
  public ISet<IAnnotation> Annotations { get; }

  /// <summary>
  /// The underlying element that is annotated.
  /// </summary>
  public IElement Element { private set; get; }
}
