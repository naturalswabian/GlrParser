// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar;
using Glr.Grammar.Elements;
using Glr.Grammar.Elements.Symbols;
using Glr.Scanner;
using System.Collections.Generic;

namespace Glr.Tree;

/// <summary>
/// A syntax tree node.
/// </summary>
/// <typeparam name="T">The type of this node itself.</typeparam>
public interface ITreeNode<T> where T : ITreeNode<T>
{
  /// <summary>
  /// The child nodes of this node.
  /// </summary>
  IList<T> Children { get; }

  /// <summary>
  /// The symbol (<see cref="Terminal"/> or <see cref="NonTerminal"/> which represents this node.
  /// </summary>
  IAbstractSymbol Symbol { get; }

  /// <summary>
  /// The production rule that lead to this node.
  /// </summary>
  IProduction<IElement> Production { get; }

  /// <summary>
  /// The token used for this node.
  /// </summary>
  Token Token { get; }

  /// <summary>
  /// The number of terminals contained within this sub tree including all child nodes.
  /// </summary>
  int TerminalLength { get; }

  /// <summary>
  /// The lexeme of this node. Is <see langword="null"/>, when this is not terminal node.
  /// </summary>
  string Lexeme { get; }
}
