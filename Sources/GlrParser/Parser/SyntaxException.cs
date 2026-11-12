// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Parser.Errors;

namespace Glr.Parser;

/// <summary>
/// An exception that is thrown when an input is not conform with the grammar definition.
/// </summary>
public sealed class SyntaxException : ParserException
{
  /// <summary>
  /// Creates a new syntax exception.
  /// </summary>
  public SyntaxException(SyntaxError syntaxError)
    : base(syntaxError.Token.SourceContext)
  {
    SyntaxError = syntaxError;
  }

  /// <summary>
  /// The underlying syntax error.
  /// </summary>
  public SyntaxError SyntaxError { get; }

  /// <inheritdoc/>
  public override string Message => ToString();

  /// <inheritdoc/>
  public override string ToString() => SyntaxError.ToString();
}
