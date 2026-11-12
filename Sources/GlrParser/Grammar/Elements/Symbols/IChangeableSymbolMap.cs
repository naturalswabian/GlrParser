// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

namespace Glr.Grammar.Elements.Symbols;

/// <summary>
/// A <see cref="ISymbolMap"/> that can modified.
/// </summary>
public interface IChangeableSymbolMap : ISymbolMap
{
  /// <summary>
  /// Adds a symbol to this map.
  /// </summary>
  /// <param name="symbol">The symbol that should be added.</param>
  void Add(IAbstractSymbol symbol);
}
