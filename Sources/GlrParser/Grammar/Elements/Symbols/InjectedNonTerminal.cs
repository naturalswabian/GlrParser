// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

namespace Glr.Grammar.Elements.Symbols;

/// <summary>
/// A non terminal that is injected into the grammar.
/// These non terminals are usually not part of the original grammar.
/// </summary>
public abstract class InjectedNonTerminal : NonTerminal
{
  /// <summary>
  /// Creates a new injected non terminal with the given name.
  /// </summary>
  /// <param name="name">The name of the injected non terminal.</param>
  protected InjectedNonTerminal(string name)
    : base(name) { }
}
