// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

namespace Glr.Grammar.Elements.Symbols;

/// <summary>
/// The non terminal is a symbol that is refined within production rules.
/// </summary>
public class NonTerminal : IAbstractSymbol
{
  /// <summary>
  /// Creates a new non terminal with the given name.
  /// </summary>
  /// <param name="name">The name of the new non terminal.</param>
  public NonTerminal(string name)
  {
    Name = name;
  }

  /// <inheritdoc/>
  public override string ToString() => Name;

  /// <inheritdoc/>
  public string Name { get; }
}
