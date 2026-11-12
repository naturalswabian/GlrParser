// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Glr.Grammar.Elements;

/// <summary>
/// An ordered list of grammar elements.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ElementList<T> : IElementList<T> where T : class, IElement
{
  /// <summary>
  /// Creates a new list that is initialized with the given elements.
  /// </summary>
  /// <param name="elements">The elements that should initially be contained in the new list.</param>
  public ElementList(IList<T> elements) : this("()", elements) { }

  /// <summary>
  /// Creates a new list that is initialized with the given elements.
  /// </summary>
  /// <param name="elements">The elements that should initially be contained in the new list.</param>
  public ElementList(params T[] elements)
      : this("()", elements) { }

  /// <summary>
  /// Creates a new list that is initialized with the given elements.
  /// </summary>
  /// <param name="name">The name of this list.</param>
  /// <param name="elements">The elements that should initially be contained in the new list.</param>
  protected ElementList(string name, IEnumerable<T> elements)
  {
    _elements = new List<T>(elements);
    Name = name;
    foreach (T element in elements)
      if (element == null) throw new GrammarException("invalid 'null' child in element list");
  }

  /// <inheritdoc/>
  public string Name { get; }

  /// <inheritdoc/>
  public override string ToString() => ToString(" ", null, null);

  /// <summary>
  /// Creates a string representation of this list.
  /// </summary>
  /// <param name="separator">The separator that is placed between the child elements of this list.</param>
  /// <param name="prefix">An addition string that precedes the resulting string.</param>
  /// <param name="postfix">An additional string that is appended to the resulting string.</param>
  /// <returns>The string representation.</returns>
  protected string ToString(string separator, string prefix, string postfix)
  {
    StringBuilder res = new StringBuilder();
    if (prefix != null) res.Append(prefix);
    for (int i = 0; i < _elements.Count; i++)
    {
      if (i != 0) res.Append(separator);
      res.Append(_elements[i]);
    }
    if (postfix != null) res.Append(postfix);
    return res.ToString();
  }

  /// <inheritdoc/>
  public int Count => _elements.Count;

  /// <inheritdoc/>
  T IElementList<T>.this[int i] => _elements[i];

  /// <inheritdoc/>
  public T this[int i] => _elements[i];

  /// <inheritdoc/>
  IEnumerator<T> IEnumerable<T>.GetEnumerator()
  {
    return _elements.GetEnumerator();
  }

  /// <inheritdoc/>
  IEnumerator IEnumerable.GetEnumerator()
  {
    return _elements.GetEnumerator();
  }

  /// <summary>
  /// Adds the given grammar element to this list.
  /// </summary>
  /// <param name="element">The element that should be added.</param>
  public void Add(T element)
  {
    _elements.Add(element);
  }

  /// <summary>
  /// Adds the given grammar elements to this list.
  /// </summary>
  /// <param name="elements">The elements that should be added.</param>
  public void AddRange(IEnumerable<T> elements)
  {
    _elements.AddRange(elements);
  }

  private readonly List<T> _elements;
}
