// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar;
using Glr.Grammar.Elements;
using Glr.Grammar.Elements.Symbols;
using System;
using System.Collections.Generic;
using System.Text;

namespace Glr.Parser;

/// <summary>
/// An item consists of a production rule, a marker - that specifies how many symbols have been
/// already successfully shifted, and a look ahead set which defines when this item can be reduced.
/// Items are used as states of a parser.
/// </summary>
public class Item
{
  /// <summary>
  /// Creates a new  item based on the given production, dot position, and look ahead.
  /// If the given position matches the number of symbols of the production's right hand side,
  /// this item is fully shifted and can be reduced.
  /// </summary>
  public Item(IProduction<IAbstractSymbol> production, int position, HashSet<Terminal> lookAhead)
  {
    // sanity check
    if (position > production.RightHandSide.Count)
      throw new ArgumentException($"illegal position {position} in production {production}");

    // initialize properties
    Production = production;
    LookAhead = lookAhead;
    Position = position;

    // calculate the hash code
    const int hash = 47;
    _hashCode = LookAhead.GetHashCode();
    _hashCode = _hashCode * hash + Production.GetHashCode();
    _hashCode = _hashCode * hash + position;
  }

  /// <summary>
  /// Gets the production rule associated with this item.
  /// </summary>
  public IProduction<IAbstractSymbol> Production { get; }

  /// <summary>
  /// Gets the number of already shifted symbols within the right hand side.
  /// Returns 0, if no symbol has been shifted so far. If all symbols are shifted
  /// the returned value equals the number of symbols within the right hand side of 
  /// the associated production rule.
  /// </summary>
  public int Position { get; }

  /// <summary>
  /// Gets the list of symbols following the marked "dot" position.
  /// </summary>
  public List<IAbstractSymbol> GetDotRest()
  {
    List<IAbstractSymbol> rest = new List<IAbstractSymbol>();
    IElementList<IAbstractSymbol> rhs = Production.RightHandSide;
    for (int i = Position + 1; i < rhs.Count; i++)
    {
      IAbstractSymbol element = rhs[i];
      rest.Add(element);
    }
    return rest;
  }

  /// <summary>
  /// Return the symbol which is marked by the "dot" position. 
  /// If this item cannot be reduced, the epsilon symbol is returned instead.
  /// </summary>
  public IAbstractSymbol GetDotSymbol(IGrammar grammar)
  {
    if (!IsReduceable)
      return Production.RightHandSide[Position];
    return grammar.EpsilonSymbol;
  }

  /// <summary>
  /// Shifts this item.
  /// </summary>
  public Item Shift()
  {
    if (!IsReduceable)
    {
      int pos = Position + 1;
      IElementList<IAbstractSymbol> rhs = Production.RightHandSide;
      if (pos <= rhs.Count)
        return new Item(Production, pos, LookAhead);
    }
    throw new ParserException($"can't shift item '{ToString()}'");
  }

  /// <summary>
  /// Returns <see langword="true"/> when this item is reducible. An item is reducible if all symbols on the
  /// right hand side (of the contained production rule) can be successfully shifted.
  /// </summary>
  public bool IsReduceable => Position >= Production.RightHandSide.Count;

  /// <summary>
  /// Gets the look ahead set of this item. An item can only be reduced using
  /// a symbol contained within this look ahead set.
  /// </summary>
  public HashSet<Terminal> LookAhead { get; }

  /// <summary>
  /// Gets the string representation of this item.
  /// </summary>
  public override string ToString()
  {
    NonTerminal left = Production.LeftSideSymbol;
    var result = new StringBuilder();
    result.Append(left.Name);
    result.Append(" -> ");
    IElementList<IAbstractSymbol> rhs = Production.RightHandSide;
    for (int i = 0; i < rhs.Count; i++)
    {
      if (i == Position) result.Append("# ");
      result.Append(rhs[i]);
      result.Append(' ');
    }
    if (Position == rhs.Count) result.Append("# ");

    result.Append('{');
    var isFirst = true;
    foreach (var terminal in LookAhead)
    {
      if (isFirst) isFirst = false;
      else result.Append(' ');
      result.Append(terminal);
    }
    result.Append('}');
    return result.ToString();
  }

  /// <summary>
  /// Checks if the given object equals this item.
  /// </summary>
  public override bool Equals(object obj)
  {
    if (this == obj) return true;
    bool res = false;
    if (obj is Item item)
    {
      res = item.Position == Position && item.Production == Production
        && LookAhead.Equals(item.LookAhead);
    }
    return res;
  }

  /// <summary>
  /// Gets the hash code of this item.
  /// </summary>
  public override int GetHashCode()
  {
    return _hashCode;
  }

  private readonly int _hashCode;
}
