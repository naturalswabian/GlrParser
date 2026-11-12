// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements;
using Glr.Grammar.Elements.Symbols;
using System.Linq;
using System.Text;

namespace Glr.Grammar;

/// <summary>
/// The default implementation of a production rule.
/// </summary>
/// <typeparam name="T">The type of the grammar elements that form this rule.</typeparam>
public class Production<T> : IProduction<T> where T : class, IElement
{
  /// <summary>
  /// Creates a new production rule.
  /// </summary>
  /// <param name="origin">The original definition of the production rule. Is <see langword="null"/> if there is no original definition.</param>
  /// <param name="left">The left-hand side symbol of the production rule.</param>
  /// <param name="right">The grammar elements on the right-hand side. When empty, an epsilon rule is defined.</param>
  public Production(IProduction<IElement> origin, NonTerminal left, IElementList<T> right)
  {
    LeftSideSymbol = left;
    RightHandSide = right;
    Origin = origin;
    ReduceSteps = RightHandSide
      .OfType<IAbstractSymbol>()
      .Count();
  }

  /// <summary>
  /// Creates a new production rule.
  /// </summary>
  /// <param name="left">The left-hand side symbol of the production rule.</param>
  /// <param name="right">The grammar elements on the right-hand side. When empty, an epsilon rule is defined.</param>
  public Production(NonTerminal left, params T[] right) : this(null, left, new ElementList<T>(right)) { }

  /// <summary>
  /// Creates a new production rule.
  /// </summary>
  /// <param name="left">The left-hand side symbol of the production rule.</param>
  /// <param name="right">The grammar elements on the right-hand side. When empty, an epsilon rule is defined.</param>
  public Production(NonTerminal left, ElementList<T> right) : this(null, left, right) { }

  /// <inheritdoc/>
  public int ReduceSteps { get; }

  /// <inheritdoc/>
  public override string ToString()
  {
    StringBuilder res = new StringBuilder();
    res.Append(LeftSideSymbol.Name);
    res.Append(" -> ");
    res.Append(RightHandSide);
    return res.ToString();
  }

  /// <inheritdoc/>
  public NonTerminal LeftSideSymbol { get; }

  /// <inheritdoc/>
  public override bool Equals(object obj)
  {
    if (obj == this) return true;
    if (!(obj is Production<T> p)) return false;
    if (p.RightHandSide.Count != RightHandSide.Count) return false;

    if (!p.LeftSideSymbol.Equals(LeftSideSymbol)) return false;
    for (int i = 0; i < RightHandSide.Count; i++)
      if (!RightHandSide[i].Equals(p.RightHandSide[i])) return false;
    return true;
  }

  /// <inheritdoc/>
  public override int GetHashCode()
  {
    return LeftSideSymbol.GetHashCode() * 17 + RightHandSide.Count;
  }

  /// <inheritdoc/>
  public IProduction<IElement> Origin { get; }

  /// <inheritdoc/>
  public IElementList<T> RightHandSide { get; }
}
