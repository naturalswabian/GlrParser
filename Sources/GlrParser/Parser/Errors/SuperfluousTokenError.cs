// Generalized LR Parser
// Copyright © 2020 Jan B. Freuer
// All rights reserved

using Glr.Scanner;

namespace Glr.Parser.Errors;

/// <summary>
/// A syntax error that represents a token that has been removed
/// in order to continue parsing when error recovery is enabled.
/// </summary>
public class SuperfluousTokenError : SyntaxError
{
  /// <summary>
  /// Creates a new syntax error that represents
  /// the removal of a superfluous token.
  /// </summary>
  /// <param name="token"></param>
  public SuperfluousTokenError(Token token) : base(token, null)
  {
  }
}
