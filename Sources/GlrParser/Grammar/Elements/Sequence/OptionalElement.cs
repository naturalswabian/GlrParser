// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using System.Collections.Generic;

namespace Glr.Grammar.Elements.Sequence;

/// <summary>
/// An element that is optional. It can be omitted while parsing.
/// </summary>
public class OptionalElement : ElementList<IElement>
{
  /// <summary>
  /// Creates a new optional element with the predefined child elements that are considered optional.
  /// </summary>
  /// <param name="elements">The optional elements.</param>
  public OptionalElement(IList<IElement> elements) : base("()?", elements) { }

  /// <summary>
  /// Creates a new optional element with the predefined child elements that are considered optional.
  /// </summary>
  /// <param name="elements">The optional elements.</param>
  public OptionalElement(params IElement[] elements)
    : base("()?", new List<IElement>())
  {
    AddRange(elements);
  }

  /// <inheritdoc/>
  public override string ToString()
  {
    var result = base.ToString();
    if (Count > 1) return $"({result})?";
    else return $"{result}?";
  }
}
