// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements;
using Glr.Grammar.Elements.Symbols;

namespace Glr.Grammar;

/// <summary>
/// The production rule defines how an input string is decomposed into smaller parts.
/// </summary>
/// <typeparam name="T">The type of grammar elements that form this production rule.</typeparam>
public interface IProduction<out T> where T : IElement
{
  /// <summary>
  /// The left-hand symbol of this production rule.
  /// </summary>
  NonTerminal LeftSideSymbol { get; }

  /// <summary>
  /// The right-hand side of this production rule.
  /// </summary>
  IElementList<T> RightHandSide { get; }

  /// <summary>
  /// The number of reduce steps of this production rule.
  /// </summary>
  int ReduceSteps { get; }

  /// <summary>
  /// The original definition of this production or <see langword="null"/> if there is no origin.
  /// </summary>
  IProduction<IElement> Origin { get; }
}
