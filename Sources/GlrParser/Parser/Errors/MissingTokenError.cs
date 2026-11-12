// Generalized LR Parser
// Copyright © 2020 Jan B. Freuer
// All rights reserved

using Glr.Scanner;

namespace Glr.Parser.Errors;

/// <summary>
/// Represents a syntax error where a single artificial token
/// has been inserted in order to continue parsing
/// when error recovery is enabled.
/// </summary>
public class MissingTokenError : SyntaxError
{
  /// <summary>
  /// Creates a new missing token error.
  /// </summary>
  public MissingTokenError(Token token) : base(token, null)
  {
  }
}
