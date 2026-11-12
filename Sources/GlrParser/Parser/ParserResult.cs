// Generalized LR Parser
// Copyright © 2020 Jan B. Freuer
// All rights reserved

using Glr.Parser.Errors;
using System;

namespace Glr.Parser;

/// <summary>
/// The result of a parser.
/// </summary>
/// <typeparam name="T">The type of the syntax tree nodes.</typeparam>
public class ParserResult<T>
{
  /// <summary>
  /// Creates a new empty result.
  /// </summary>
  public ParserResult()
    : this(Array.Empty<T>(), Array.Empty<SyntaxError>())
  {
  }

  /// <summary>
  /// Creates a new result based on the given syntax trees and syntax errors.
  /// </summary>
  public ParserResult(T[] trees, SyntaxError[] errors)
  {
    Trees = trees;
    Errors = errors;
  }

  /// <summary>
  /// The syntax trees that result from parsing.
  /// </summary>
  public T[] Trees { get; }

  /// <summary>
  /// The syntax errors that occurred while parsing.
  /// </summary>
  public SyntaxError[] Errors { get; }
}
