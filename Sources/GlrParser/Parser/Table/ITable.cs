// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar;
using Glr.Grammar.Elements.Symbols;
using System.Collections.Generic;

namespace Glr.Parser.Table;

/// <summary>
/// The table is a lookup table for a parser. It contains the necessary information needed for
/// a state machine. The states of the automaton differ by unique identifiers. The start state 
/// can be retrieved via <see cref="GetStartStateId"/>.
/// </summary>
/// <typeparam name="S">The type of the identifier that uniquely identifies a state.</typeparam>
public interface ITable<S> where S : ITableState
{
  /// <summary>
  /// Gets the start or root state identifier.
  /// </summary>
  S GetStartStateId();

  /// <summary>
  /// Gets the successor state if the current state's type is 'shift'.
  /// </summary>
  /// <param name="stateId">The current state identifier of the state machine.</param>
  /// <param name="symbol">The next, non consumed input symbol.</param>
  S GetShiftFollowState(S stateId, IAbstractSymbol symbol);

  /// <summary>
  /// Returns in case of a reduction the reduced productions.
  /// </summary>
  /// <param name="stateId">The current state of the state machine.</param>
  /// <param name="lookAhead">The next, non consumed input terminal.</param>
  IList<IProduction<IAbstractSymbol>> GetReducedProductions(S stateId, Terminal lookAhead);

  /// <summary>
  /// Gets the look-ahead terminals for the given state. These are the terminals
  /// that can be consumed with the next shift-reduce.
  /// </summary>
  /// <param name="stateId">The state of which the look-ahead should be determined.</param>
  /// <returns>The set of look-ahead terminals.</returns>
  ISet<Terminal> GetLookAhead(S stateId);

  /// <summary>
  /// The underlying grammar.
  /// </summary>
  IGrammar Grammar { get; }
}
