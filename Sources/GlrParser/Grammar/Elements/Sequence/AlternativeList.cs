// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using System.Collections.Generic;

namespace Glr.Grammar.Elements.Sequence;

/// <summary>
/// A list of elements whereby only one single element is chosen.
/// </summary>
public class AlternativeList : ElementList<IElement>
{
  /// <summary>
  /// Creates a new alternation list initialized with the given elements.
  /// </summary>
  /// <param name="elements">The elements that should be initially contained within this list.</param>
  public AlternativeList(List<IElement> elements) : base("(|)", elements) { }

  /// <summary>
  /// Creates a new alternation list initialized with the given elements.
  /// </summary>
  /// <param name="elements">The elements that should be initially contained within this list.</param>
  public AlternativeList(params IElement[] elements)
    : base("(|)", new List<IElement>())
  {
    foreach (IElement element in elements) Add(element);
  }

  /// <inheritdoc/>
  public override string ToString() => ToString(" | ", "(", ")");
}
