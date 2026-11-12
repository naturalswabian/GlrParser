// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements.Symbols;
using System.Collections.Generic;

namespace Glr.Scanner;

/// <summary>
/// The scanner provides <see cref="Token"/>s for a parser. Characters from the
/// input stream will be combined and return as single tokens.
/// </summary>
public interface IScanner
{
  /// <summary>
  /// Gets the sequence of tokens extracted from the given <paramref name="source"/>.
  /// </summary>
  /// <param name="source">The source of tokens.</param>
  /// <returns>The token sequence.</returns>
  IEnumerable<Token> GetTokenSequence(IScannerSource source);

  /// <summary>
  /// The end-of-stream terminal that is emitted when the end has been reached.
  /// </summary>
  Terminal EofTerminal { get; }
}
