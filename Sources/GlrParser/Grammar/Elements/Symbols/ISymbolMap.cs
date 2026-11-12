// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

namespace Glr.Grammar.Elements.Symbols;

/// <summary>
/// A lookup map of grammar symbols. It allows retrieving <see cref="Terminal"/>s, 
/// <see cref="NonTerminal"/>s, and <see cref="IAbstractSymbol"/>s in general.
/// </summary>
public interface ISymbolMap
{
  /// <summary>
  /// Gets the <see cref="NonTerminal"/> with the given name.
  /// </summary>
  /// <param name="name">The name of the desired non-terminal.</param>
  /// <returns><see langword="null"/>, if there is no corresponding non-terminal.</returns>
  NonTerminal GetNonTerminal(string name);

  /// <summary>
  /// Gets the <see cref="Terminal"/> with the given name.
  /// </summary>
  /// <param name="name">The name of the desired terminal.</param>
  /// <returns><see langword="null"/>, if there is no corresponding terminal.</returns>
  Terminal GetTerminal(string name);

  /// <summary>
  /// Gets the <see cref="NonTerminal"/> or <see cref="Terminal"/> with the given name.
  /// </summary>
  /// <param name="name">The name of the desired terminal or non-terminal.</param>
  /// <returns><see langword="null"/>, if there is neither a corresponding terminal nor a non-terminal.</returns>
  IAbstractSymbol GetSymbol(string name);
}
