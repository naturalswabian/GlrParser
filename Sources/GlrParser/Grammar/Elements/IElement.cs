// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

namespace Glr.Grammar.Elements;

/// <summary>
/// An element of a grammar.
/// </summary>
public interface IElement
{
  /// <summary>
  /// The name of this element.
  /// </summary>
  string Name { get; }
}
