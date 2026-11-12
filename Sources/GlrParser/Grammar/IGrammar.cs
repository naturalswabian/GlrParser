// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements.Symbols;
using Glr.Meta;
using Glr.Scanner;
using System.Collections.Generic;

namespace Glr.Grammar;

/// <summary>
/// A (context-free) grammar describes all possible inputs for a machine that accepts these
/// entries. The machine is usually parsed by a parser realized in combination with a table.
/// Production rules are used to describe the language,
/// that describe how the derivation tree for a given input looks like.
/// The sequence of these production rules does not play any role in principle.
/// For some grammar having conflicts, the order play a role in 
/// conflict resolution. All grammars must be start separated. 
/// This means that there can only be one single rule, for
/// which is the start symbol on the left. <see cref="IParser{T}"/>.
/// </summary>
public interface IGrammar : ISymbolMap
{
  /// <summary>
  /// Returns the start symbol of this grammar.
  /// </summary>
  NonTerminal StartSymbol { get; }

  /// <summary>
  /// Gets the EOF (End Of File) symbol of this grammar.
  /// The EOF symbol is used to return the end of an input, which is provided by an 
  /// <see cref="IScanner"/>.
  /// </summary>
  Terminal EofSymbol { get; }

  /// <summary>
  /// The set of <see cref="Terminal"/>s that contained within this grammar.
  /// </summary>
  ISet<Terminal> Terminals { get; }

  /// <summary>
  /// The set of <see cref="NonTerminal"/>s that are contained within this grammar.
  /// </summary>
  ISet<NonTerminal> NonTerminals { get; }

  /// <summary>
  /// The production rules that form this grammar.
  /// </summary>
  IList<Production<IAbstractSymbol>> Productions { get; }

  /// <summary>
  /// The one and only production rule that is used as root.
  /// </summary>
  IProduction<IAbstractSymbol> StartProduction { get; }

  /// <summary>
  /// Determines the 'first' set of terminals for the given symbols. 
  /// </summary>
  /// <param name="symbols">The symbols of which the 'first' set should be determined.</param>
  /// <returns>The set of 'first' terminals.</returns>
  ISet<Terminal> First(IList<IAbstractSymbol> symbols);

  /// <summary>
  /// The set of terminals that are fully skipped (ignored) while parsing.
  /// </summary>
  ISet<Terminal> SkipTerminals { get; }

  /// <summary>
  /// The set of terminals that are partially skipped (ignored) while parsing.
  /// However, they are still kept within the syntax tree.
  /// </summary>
  ISet<Terminal> SpecialTerminals { get; }

  /// <summary>
  /// The symbol that is used for Epsilon rules (empty production rules).
  /// </summary>
  Terminal EpsilonSymbol { get; }

  /// <summary>
  /// When <see langword="true"/>, the associated parser accepts also empty inputs.
  /// </summary>
  bool AllowsEmptyInput { get; }
}
