// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using System.Collections.Generic;

namespace Glr.Grammar.Elements.Symbols;

/// <summary>
/// The default implementation of the <see cref="IChangeableSymbolMap"/>.
/// </summary>
public sealed class SymbolMap : IChangeableSymbolMap
{
  /// <inheritdoc/>
  public NonTerminal GetNonTerminal(string name)
  {
    if (NonTerminals.TryGetValue(name, out NonTerminal nonTerminal))
      return nonTerminal;
    return null;
  }

  /// <inheritdoc/>
  public Terminal GetTerminal(string name)
  {
    if (Terminals.TryGetValue(name, out Terminal terminal))
      return terminal;
    return null;
  }

  /// <inheritdoc/>
  public IAbstractSymbol GetSymbol(string name)
  {
    IAbstractSymbol result;
    result = GetNonTerminal(name);
    if (result != null) return result;
    result = GetTerminal(name);
    return result;
  }

  /// <inheritdoc/>
  public void Add(IAbstractSymbol symbol)
  {
    switch (symbol)
    {
      case NonTerminal nonTerminal:
        if (NonTerminals.ContainsKey(symbol.Name))
          throw new GrammarException($"duplicate non terminal '{symbol.Name}'");
        NonTerminals[symbol.Name] = nonTerminal;
        break;
      case Terminal terminal:
        if (Terminals.ContainsKey(symbol.Name))
          throw new GrammarException($"duplicate terminal '{symbol.Name}'");
        Terminals[symbol.Name] = terminal;
        break;
    }
  }

  private Dictionary<string, Terminal> Terminals { get; } = [];
  private Dictionary<string, NonTerminal> NonTerminals { get; } = [];
}
