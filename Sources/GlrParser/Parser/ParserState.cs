// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Parser.Errors;
using Glr.Parser.Table;
using Glr.Tree;
using System.Collections.Generic;

namespace Glr.Parser;

/// <summary>
/// The state of a parser while parsing.
/// </summary>
/// <typeparam name="S">The type of the underlying state machine states.</typeparam>
/// <typeparam name="T">The type of syntax tree nodes.</typeparam>
internal class ParserState<S, T>
    where S : ITableState
    where T : class, ITreeNode<T>
{
  /// <summary>
  /// The current state of the state machine.
  /// </summary>
  public StackSet<S, T> StackSet { set; get; }

  public StackSet<S, T> LastStackSet { set; get; }

  /// <summary>
  /// The list of errors that have been discovered so far.
  /// </summary>
  public IList<SyntaxError> Errors { get; } = new List<SyntaxError>();

  /// <summary>
  /// The stream of tokens.
  /// </summary>
  public TokenStream TokenStream { set; get; }
}
