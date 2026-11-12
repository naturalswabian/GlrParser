// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar;
using Glr.Grammar.Elements.Symbols;
using System.Collections.Generic;

namespace Glr.Parser.Table;

/// <summary>
/// A state machine table used for parsing that creates the states on the fly.
/// </summary>
public class DynamicTable : ITable<ItemState>
{
  /// <summary>
  /// Creates a new dynamic state machine table.
  /// </summary>
  /// <param name="grammar">The grammar on which this table should be based on.</param>
  public DynamicTable(IGrammar grammar)
  {
    Grammar = grammar;
  }

  /// <inheritdoc/>
  public virtual IList<IProduction<IAbstractSymbol>> GetReducedProductions(ItemState stateId, Terminal lookAhead)
  {
    var result = new List<IProduction<IAbstractSymbol>>();
    var reductionItems = stateId.GetReduceableItems(lookAhead);
    foreach (Item reductionItem in reductionItems)
    {
      result.Add(reductionItem.Production);
    }
    return result;
  }

  /// <inheritdoc/>
  public static int GetReductionCount(ItemState state, Terminal lookAhead)
  {
    if (state == null) return 0;
    return state.GetReduceableItems(lookAhead).Count;
  }

  /// <inheritdoc/>
  public virtual ItemState GetShiftFollowState(ItemState stateId, IAbstractSymbol symbol)
  {
    if (stateId == null) return null;
    ItemState followState = stateId.GoTo(symbol);
    if (followState.GetItems().Count == 0) return null;
    return followState;
  }

  /// <inheritdoc/>
  public ItemState GetStartStateId()
  {
    var lookahead = new HashSet<Terminal>();
    var items = new HashSet<Item>();
    lookahead.Add(Grammar.EofSymbol);
    items.Add(new Item(Grammar.StartProduction, 0, lookahead));
    ItemState startState = new ItemState(items, Grammar);
    return startState;
  }

  /// <inheritdoc/>
  public ISet<Terminal> GetLookAhead(ItemState stateId)
  {
    var result = new HashSet<Terminal>();
    foreach (Item item in stateId.GetItems())
    {
      // when the item is already complete we can directly use the look ahead of the item
      if (item.IsReduceable)
        result.UnionWith(item.LookAhead);
      else
      {
        // when the item is not complete we use the current terminal as look ahead
        var currentSymbol = item.GetDotSymbol(Grammar);
        if (currentSymbol is Terminal terminal)
          result.Add(terminal);
      }
    }
    return result;
  }

  /// <inheritdoc/>
  public IGrammar Grammar { get; }
}
