// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using System.Collections.Generic;

namespace Glr.Grammar.Elements.Sequence;

/// <summary>
/// A group of grammar elements that can appear in any order.
/// </summary>
public class UnorderedGroup : ElementList<IElement>
{
  /// <summary>
  /// Creates a new unordered element with the given elements.
  /// The order of the elements while parsing is not determined.
  /// </summary>
  /// <param name="elements">The unordered elements.</param>
  public UnorderedGroup(IList<IElement> elements) : base("(&)", elements) { }

  /// <summary>
  /// Creates a new unordered element with the given elements.
  /// The order of the elements while parsing is not determined.
  /// </summary>
  /// <param name="elements">The unordered elements.</param>
  public UnorderedGroup(params IElement[] elements)
      : base("(&)", new List<IElement>())
  {
    foreach (IElement element in elements) Add(element);
  }

  /// <inheritdoc/>
  public override string ToString() => ToString(" & ", "(", ")");
}
