// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Scanner;
using System.Collections.Generic;

namespace Glr.Meta;

/// <summary>
/// A parser that parses an input and creates a corresponding tree out of it.
/// </summary>
/// <typeparam name="T">The type of the root node of the resulting object model.</typeparam>
public interface IParser<out T> where T : class
{
  /// <summary>
  /// Parses the given <paramref name="source"/> and creates the corresponding object model out of it.
  /// </summary>
  /// <param name="source">The (textual) source that should be parsed.</param>
  /// <param name="factoryContext">The context of the factory that is used to create instances of the resulting object model.</param>
  /// <returns>The result of the parsing.</returns>
  IParserResult<T> Parse(IScannerSource source, object factoryContext);

  /// <summary>
  /// Parses the given <paramref name="tokens"/> and creates the corresponding object model out of it.
  /// </summary>
  /// <param name="tokens">The token sequence used for parsing.</param>
  /// <param name="resourceUri">The name of the resource.</param>
  /// <param name="factoryContext">The context of the factory that is used to create instances of the resulting object model.</param>
  /// <returns>The result of the parsing.</returns>
  IParserResult<T> Parse(IEnumerable<Token> tokens, string resourceUri, object factoryContext);

  /// <summary>
  /// Creates a new scanner that is based on the grammar definition of this parser.
  /// </summary>
  IScanner CreateScanner();
}
