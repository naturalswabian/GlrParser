// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using Glr.Grammar;
using Glr.Grammar.Elements.Symbols;
using Glr.Meta.SymbolHandlers;
using System.Collections.Generic;

namespace Glr.Meta.Transform;

/// <summary>
/// Factory that creates <see cref="ITreeTransformer"/>s.
/// </summary>
public static class TreeTransformerFactory
{
  /// <summary>
  /// Creates a new <see cref="ITreeTransformer"/>.
  /// </summary>
  public static ITreeTransformer Create(IGrammar grammar, IDictionary<IAbstractSymbol, ValueConverter> valueConverters, IDictionary<NonTerminal, INonTerminalHandler> nonTerminalHandlers)
  {
    CreateMissingHandlers(nonTerminalHandlers, grammar.NonTerminals);
    return new TreeTransformer(valueConverters, nonTerminalHandlers);
  }

  private static void CreateMissingHandlers(IDictionary<NonTerminal, INonTerminalHandler> nonTerminalHandlers, IEnumerable<NonTerminal> nonTerminals)
  {
    foreach (var nonTerminal in nonTerminals)
    {
      if (!nonTerminalHandlers.TryGetValue(nonTerminal, out var _))
      {
        switch (nonTerminal)
        {
          case AnnotatedNonTerminal _:
          case DerivedNonTerminal _:
            nonTerminalHandlers[nonTerminal] = new InheritContextNonTerminalHandler(nonTerminal);
            break;
        }
      }
    }
  }
}
