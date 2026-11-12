// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements.Symbols;
using System;
using System.Text;

namespace Glr.Scanner;

/// <summary>
/// The token is a unit created by a <see cref="IScanner"/> that combines one or more characters into
/// a group that is associated to a <see cref="Terminal"/>.
/// </summary>
public sealed class Token
{
  /// <summary>
  /// Creates a new token.
  /// </summary>
  /// <param name="terminal">The terminal that classifies this token.</param>
  /// <param name="lexeme">The full underlying text of this token.</param>
  /// <param name="context">The location within the resource.</param>
  /// <param name="specialToken">The special tokens that has seen directly before this token.</param>
  public Token(Terminal terminal, string lexeme, SourceContext context, Token specialToken)
  {
    ArgumentNullException.ThrowIfNull(context);

    Terminal = terminal;
    SourceContext = context;
    Lexeme = lexeme;
    SpecialToken = specialToken;
  }

  /// <summary>
  /// The terminal that defines the kind of this token.
  /// </summary>
  public Terminal Terminal { get; }

  /// <summary>
  /// The location within the resource that lead to this token.
  /// </summary>
  public SourceContext SourceContext { get; }

  /// <summary>
  /// The text as it was seen within resource that forms this token.
  /// </summary>
  public string Lexeme { get; }

  /// <summary>
  /// The chained list of tokens that have been recognized as special tokens.
  /// </summary>
  public Token SpecialToken { get; }

  /// <inheritdoc/>
  public override string ToString()
  {
    var result = new StringBuilder();
    if (Terminal != null)
    {
      result.Append(Terminal.ToString());
      result.Append(' ');
    }
    else
    {
      result.Append("??? ");
    }

    if (Lexeme != null)
    {
      result.Append('\'');
      for (int i = 0; i < Lexeme.Length; i++)
      {
        switch (Lexeme[i])
        {
          case '\n': result.Append("\\n"); break;
          case '\r': result.Append("\\r"); break;
          case '\t': result.Append("\\t"); break;
          default: result.Append(Lexeme[i]); break;
        }
      }
      result.Append('\'');
    }
    Token specialToken = this.SpecialToken;
    bool hasSpecialTokens = specialToken != null;
    if (hasSpecialTokens) result.Append(" [");
    while (specialToken != null)
    {
      result.Append(specialToken.Terminal.ToString());
      specialToken = specialToken.SpecialToken;
      if (specialToken != null) result.Append(", ");
    }
    if (hasSpecialTokens) result.Append(']');
    return result.ToString();
  }
}
