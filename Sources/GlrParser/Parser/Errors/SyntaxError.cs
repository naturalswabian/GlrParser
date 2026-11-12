// Generalized LR Parser
// Copyright © 2020 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements.Symbols;
using Glr.Scanner;
using System.Collections.Generic;
using System.Text;

namespace Glr.Parser.Errors;

/// <summary>
/// Represents a syntax error that occurred while parsing.
/// </summary>
public class SyntaxError
{
  /// <summary>
  /// Creates a new syntax error.
  /// </summary>
  /// <param name="token">The token that was not consumed any more.</param>
  /// <param name="lookAhead">The set of possible terminals.</param>
  public SyntaxError(Token token, ISet<Terminal> lookAhead)
  {
    LookAhead = lookAhead;
    Token = token;
  }

  /// <summary>
  /// The look-ahead terminals which could have been parsed
  /// </summary>
  public ISet<Terminal> LookAhead { get; }

  /// <summary>
  /// The current token that could not be consumed any more.
  /// </summary>
  public Token Token { get; }

  /// <summary>
  /// The location and the resource which caused this error.
  /// </summary>
  public SourceContext SourceContext => Token.SourceContext;

  /// <inheritdoc/>
  public override string ToString()
  {
    var message = new StringBuilder();
    var terminal = Token.Terminal;

    message.Append(GetType().Name);
    message.Append(':');
    if (terminal != null) message.Append(" on terminal " + terminal);
    else
    {
      message.Append(" on input '" + Token.Lexeme);
      message.Append('\'');
    }
    if (SourceContext != null)
    {
      message.Append(" in \"");
      message.Append(SourceContext.Name);
      message.Append("\" line ");
      message.Append(SourceContext.LineNumber);
      message.Append(" column ");
      message.Append(SourceContext.ColumnNumber);
    }
    if (LookAhead != null)
    {
      message.Append(". Expected ");
      List<Terminal> terms = new List<Terminal>(LookAhead);
      for (int i = 0; i < terms.Count; i++)
      {
        if (i > 0) message.Append(", ");
        if (i == terms.Count - 1) message.Append("or ");
        message.Append('\'');
        message.Append(terms[i].Name);
        message.Append('\'');
      }
    }
    return message.ToString();
  }
}
