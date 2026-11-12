// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

namespace Glr.Grammar.Elements.Symbols;

/// <summary>
/// The derived non terminal is non terminal that is artificially 
/// introduced while normalizing production rules.
/// </summary>
public class DerivedNonTerminal : InjectedNonTerminal
{
  /// <summary>
  /// Creates a new derived non terminal with the given name.
  /// </summary>
  /// <param name="name">The name of the derived non terminal.</param>
  public DerivedNonTerminal(string name) : base(name) { }
}
