// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar;
using Glr.Grammar.Elements.Symbols;
using System.Collections.Generic;
using System.Linq;

namespace Glr.Parser.Table;

/// <summary>
/// A state machine table used for parsing that creates the states on the fly. 
/// The states are cached such that they can be re-used.
/// </summary>
public sealed class DynamicCachingTable : DynamicTable
{
  /// <summary>
  /// Creates a new parser state machine table.
  /// </summary>
  /// <param name="grammar">The grammar on which this table should be based on.</param>
  public DynamicCachingTable(IGrammar grammar)
      : base(grammar)
  {
    _shiftFollowStates = new Dictionary<ItemState, Dictionary<IAbstractSymbol, ItemState>>();
    _reducedProductions = new Dictionary<ItemState, Dictionary<Terminal, IList<IProduction<IAbstractSymbol>>>>();
    _states = new List<ItemState>();
  }

  /// <inheritdoc/>
  public override IList<IProduction<IAbstractSymbol>> GetReducedProductions(ItemState stateId, Terminal lookAhead)
  {
    Dictionary<Terminal, IList<IProduction<IAbstractSymbol>>> map = null;
    if (!_reducedProductions.TryGetValue(stateId, out map))
    {
      map = new Dictionary<Terminal, IList<IProduction<IAbstractSymbol>>>();
      _reducedProductions.Add(stateId, map);
    }

    IList<IProduction<IAbstractSymbol>> result = null;
    if (!map.TryGetValue(lookAhead, out result))
    {
      result = base.GetReducedProductions(stateId, lookAhead);
      map.Add(lookAhead, result);
    }

    return result;
  }

  /// <inheritdoc/>
  public override ItemState GetShiftFollowState(ItemState stateId, IAbstractSymbol symbol)
  {
    Dictionary<IAbstractSymbol, ItemState> map = null;
    if (!_shiftFollowStates.TryGetValue(stateId, out map))
    {
      map = new Dictionary<IAbstractSymbol, ItemState>();
      _shiftFollowStates[stateId] = map;
    }

    ItemState result = null;
    if (symbol != null && !map.TryGetValue(symbol, out result))
    {
      result = UnifyState(base.GetShiftFollowState(stateId, symbol));
      map[symbol] = result;
    }

    return result;
  }

  /// <inheritdoc/>
  private ItemState UnifyState(ItemState state)
  {
    ItemState result = null;
    if (state != null)
    {
      result = _states.FirstOrDefault(existingState => existingState.Equals(state));

      if (result == null)
      {
        result = state;
        _states.Add(result);
      }
    }
    return result;
  }

  private readonly List<ItemState> _states;
  private readonly Dictionary<ItemState, Dictionary<IAbstractSymbol, ItemState>> _shiftFollowStates;
  private readonly Dictionary<ItemState, Dictionary<Terminal, IList<IProduction<IAbstractSymbol>>>> _reducedProductions;
}
