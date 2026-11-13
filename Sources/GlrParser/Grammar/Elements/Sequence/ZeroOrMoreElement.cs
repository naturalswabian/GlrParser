// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using System.Collections.Generic;

namespace Glr.Grammar.Elements.Sequence;

/// <summary>
/// An element that can be repeated multiple times. It also can be completely omitted.
/// </summary>
public class ZeroOrMoreElement : ElementList<IElement>
{
  /// <summary>
  /// Creates a new zero-or-more element.
  /// </summary>
  /// <param name="elements">The elements that are optional and can be repeated.</param>
  public ZeroOrMoreElement(List<IElement> elements) : base("()*", elements) { }

  /// <summary>
  /// Creates a new zero-or-more element.
  /// </summary>
  /// <param name="elements">The elements that are optional and can be repeated.</param>
  public ZeroOrMoreElement(params IElement[] elements) : base("()*", new List<IElement>())
  {
    foreach (IElement element in elements) Add(element);
  }

  /// <inheritdoc/>
  public override string ToString()
  {
    var result = base.ToString();
    if (Count > 1) return $"({result})*";
    else return $"{result}*";
  }
}
