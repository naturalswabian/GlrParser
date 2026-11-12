// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar;
using Glr.Grammar.Elements.Symbols;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glr.Parser.Table;

/// <summary>
/// A state is a list of all items that belong to that state. 
/// A transition from this state to the next state can be determined.
/// </summary>
public sealed class ItemState : ITableState
{
  /// <summary>
  /// Creates a new state based on the given items.
  /// </summary>
  /// <param name="items">The item base of which the hull is created.</param>
  /// <param name="grammar">The grammar on which the state is based on.</param>
  public ItemState(ISet<Item> items, IGrammar grammar)
  {
    _closure = new HashSet<Item>(items);
    _grammar = grammar;
    var lookAheads = new Dictionary<IAbstractSymbol, HashSet<Terminal>>();

    bool closureChanged = true;
    while (closureChanged)
    {
      closureChanged = ComputeClosure(lookAheads);
      closureChanged |= ProcessProductions(lookAheads);
    }

    // calculate hash code
    _hashCode = _closure.GetHashCode();
  }

  /// <summary>
  /// Calculates the following state for the given symbol.
  /// </summary>
  /// <param name="symbol">The input for the transition to the next state.</param>
  public ItemState GoTo(IAbstractSymbol symbol)
  {
    return new ItemState(GetShiftableItems(symbol), _grammar);
  }

  /// <summary>
  /// Gets the set of items that can be reduced for the given look-ahead terminal.
  /// If there are no reduce/reduce conflicts the resulting set contains only one 
  /// single element or is empty.
  /// </summary>
  /// <param name="lookAhead">The look-ahead for which the reducible items should be determined.</param>
  /// <returns>The set of reducible items.</returns>
  public ISet<Item> GetReduceableItems(Terminal lookAhead)
  {
    var res = new HashSet<Item>();
    foreach (Item item in _closure)
    {
      if (item.IsReduceable)
      {
        HashSet<Terminal> iLookAhead = item.LookAhead;
        foreach (Terminal symbol in iLookAhead)
          if (symbol == lookAhead)
            res.Add(item);
      }
    }
    return res;
  }

  /// <summary>
  /// Gets the items of this state.
  /// </summary>
  /// <returns>The items of this state.</returns>
  public ISet<Item> GetItems()
  {
    return _closure;
  }

  /// <summary>
  /// Determine the set of symbols that can be used for shifting.
  /// </summary>
  public ISet<IAbstractSymbol> GetShiftSymbols()
  {
    var set = new HashSet<IAbstractSymbol>();
    foreach (Item item in _closure)
    {
      if (!item.IsReduceable)
        set.Add(item.GetDotSymbol(_grammar));
    }
    return set;
  }

  /// <summary>
  /// Determine the set of symbols which can be used for reducing this state.
  /// </summary>
  public ISet<Terminal> GetReduceSymbols()
  {
    var set = new HashSet<Terminal>();
    foreach (Item item in _closure)
    {
      if (item.IsReduceable)
        set.UnionWith(item.LookAhead);
    }
    return set;
  }

  /// <summary>
  /// Gets the set of items that can be shifted.
  /// </summary>
  /// <param name="symbol">The symbol that should be used for shifting.</param>
  /// <returns>The set of shifted items.</returns>
  public ISet<Item> GetShiftableItems(IAbstractSymbol symbol)
  {
    var shiftableItems = new HashSet<Item>();
    foreach (Item item in _closure)
    {
      IAbstractSymbol dotSymbol = item.GetDotSymbol(_grammar);
      if (symbol == dotSymbol)
      {
        Item shiftedItem = item.Shift();
        shiftableItems.Add(shiftedItem);
      }
    }
    return shiftableItems;
  }

  /// <inheritdoc/>
  public override string ToString()
  {
    var result = new StringBuilder();
    foreach (var item in _closure)
      result.AppendLine(item.ToString());
    return result.ToString();
  }

  /// <summary>
  /// Checks if two <see cref="ItemState"/>s are equal. They are equal if the hull
  /// of both items is equal.
  /// </summary>
  public override bool Equals(object obj)
  {
    if (obj == this) return true;
    if (obj is ItemState itemState)
    {
      return _closure.Equals(itemState._closure);
    }
    return false;
  }

  /// <inheritdoc/>
  public override int GetHashCode()
  {
    return _hashCode;
  }


  private bool ComputeClosure(Dictionary<IAbstractSymbol, HashSet<Terminal>> lookAheads)
  {
    bool closureChanged = false;
    foreach (Item item in _closure)
    {
      IAbstractSymbol symbol = item.GetDotSymbol(_grammar);
      if (symbol is NonTerminal)
      {
        HashSet<Terminal> lookAhead = null;
        if (!lookAheads.TryGetValue(symbol, out lookAhead))
        {
          lookAhead = new HashSet<Terminal>();
          lookAheads.Add(symbol, lookAhead);
        }

        int lookAheadSize = lookAhead.Count;
        HashSet<Terminal> newLookAhead = CreateClosureLookAhead(item.LookAhead, _grammar, item.GetDotRest());
        lookAhead.UnionWith(newLookAhead);
        closureChanged |= lookAheadSize !=
          lookAhead.Count;
      }
    }
    return closureChanged;
  }

  private bool ProcessProductions(Dictionary<IAbstractSymbol, HashSet<Terminal>> lookAheads)
  {
    bool closureChanged = false;
    foreach (Production<IAbstractSymbol> production in _grammar.Productions)
    {
      NonTerminal lhs = production.LeftSideSymbol;
      HashSet<Terminal> lookAhead = null;
      if (lookAheads.ContainsKey(lhs)) lookAhead = lookAheads[lhs];
      if (lookAhead != null)
      {
        Item newItem = _closure.FirstOrDefault(item => item.Production == production && item.Position == 0);
        if (newItem == null)
        {
          newItem = new Item(production, 0, lookAhead);
          closureChanged = true;
          _closure.Add(newItem);
        }
      }
    }
    return closureChanged;
  }

  private static HashSet<Terminal> CreateClosureLookAhead(HashSet<Terminal> oldLookAhead,
    IGrammar grammar, List<IAbstractSymbol> dotRest)
  {
    HashSet<Terminal> newLookAhead = new HashSet<Terminal>();
    foreach (Terminal element in oldLookAhead)
      CreateClosureLookAhead(element, grammar, dotRest, newLookAhead);
    return newLookAhead;
  }

  private static void CreateClosureLookAhead(Terminal oldLookAhead,
  IGrammar grammar, List<IAbstractSymbol> dotRest, HashSet<Terminal> newLookAhead)
  {
    List<IAbstractSymbol> list = new List<IAbstractSymbol>();
    list.AddRange(dotRest);
    list.Add(oldLookAhead);
    newLookAhead.UnionWith(grammar.First(list));
  }

  private readonly int _hashCode;
  private readonly HashSet<Item> _closure;
  private readonly IGrammar _grammar;
}
