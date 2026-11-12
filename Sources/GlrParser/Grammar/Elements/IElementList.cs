// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using System.Collections.Generic;

namespace Glr.Grammar.Elements;

/// <summary>
/// A list of grammar elements such as terminals and non terminals.
/// </summary>
public interface IElementList<out T> : IElement, IEnumerable<T> where T : IElement
{
  /// <summary>
  /// The number of elements contained within this list.
  /// </summary>
  int Count { get; }

  /// <summary>
  /// Index operator to access an element within the list by its index.
  /// </summary>
  T this[int i] { get; }
}
