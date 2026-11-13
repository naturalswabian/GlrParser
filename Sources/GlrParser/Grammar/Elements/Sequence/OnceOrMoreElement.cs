// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using System.Collections.Generic;

namespace Glr.Grammar.Elements.Sequence;

/// <summary>
/// An element that can be repeated multiple times but occurs at least one time.
/// </summary>
public class OnceOrMoreElement : ElementList<IElement>
{
  /// <summary>
  /// Creates a new list initialized with the given elements.
  /// </summary>
  /// <param name="elements">The elements that should be initially contained within this list.</param>
  public OnceOrMoreElement(IList<IElement> elements) : base("()+", elements) { }

  /// <summary>
  /// Creates a new list initialized with the given elements.
  /// </summary>
  /// <param name="elements">The elements that should be initially contained within this list.</param>
  public OnceOrMoreElement(params IElement[] elements)
    : base("()+", new List<IElement>())
  {
    foreach (IElement element in elements) Add(element);
  }

  /// <inheritdoc/>
  public override string ToString()
  {
    var result = base.ToString();
    if (Count > 1) return $"({result})+";
    else return $"{result}+";
  }
}
