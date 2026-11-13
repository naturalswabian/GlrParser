// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements.Symbols;
using System.Collections.Generic;

namespace Glr.Parser.Table;

/// <summary>
/// The state of a parsing state machine.
/// </summary>
public interface ITableState
{
  /// <summary>
  /// Determine the set of symbols that can be used for shifting.
  /// </summary>
  ISet<IAbstractSymbol> GetShiftSymbols();
}
