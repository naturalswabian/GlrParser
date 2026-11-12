// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements.Symbols;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Glr.Scanner;

internal sealed class KeywordTokenEnumerator : IEnumerator<Token>
{
  public KeywordTokenEnumerator(IScannerSource source, KeywordContext context)
  {
    _context = context;
    _source = source;
    _content = source.Content;
    _contentIndex = 0;
  }

  public Token Current { get; private set; }

  public void Dispose()
  {
    // nothing to cleanup
  }

  object IEnumerator.Current
  {
    get { return Current; }
  }

  public bool MoveNext()
  {
    bool doSearch = true;

    Token token = null;
    while (doSearch)
    {
      token = NextPrimitiveToken();
      var terminal = token.Terminal;
      if (terminal == _context.EofTerminal)
        return false;

      if (terminal != null)
      {
        if (_context.SkipTerminals.Contains(terminal))
        {
          doSearch = true;
        }
        else if (_context.SpecialTerminals.Contains(terminal))
        {
          _lastSpecial = token;
          doSearch = true;
        }
        else
        {
          _lastSpecial = null;
          doSearch = false;
        }
      }
      else
        doSearch = false;
    }

    Current = token;
    return true;
  }

  public void Reset()
  {
    throw new NotSupportedException("resetting is not supported");
  }

  /// <summary>
  /// Gets the next token without proceeding to the next token.
  /// </summary>
  /// <returns><see langword="null"/>, if there is no matching token.</returns>
  private InternalToken NextInternalToken()
  {
    if (_contentIndex < _content.Length)
    {
      // try to match keyword
      var keywordToken = MatchKeyword();

      // try to match flex terminal
      var flexToken = MatchFlexTerminal();

      // choose
      return Select(keywordToken, flexToken);
    }

    return new InternalToken(_context.EofTerminal, null);
  }

  private Token NextPrimitiveToken()
  {
    // create final token
    int startIndex = _contentIndex;
    SourceContext scontext = new SourceContext(startIndex, _source.Name, _source.GetLine(_contentIndex), _source.GetColumn(_contentIndex));

    // if we reached we end we return the EOF token
    if (_contentIndex >= _content.Length)
      return new Token(_context.EofTerminal, "", scontext, _lastSpecial);

    // move next
    Terminal foundTerminal = null;
    while (_contentIndex < _content.Length)
    {
      var internalToken = NextInternalToken();
      var foundLexeme = internalToken?.Lexeme;
      if (foundLexeme == null)
      {
        _contentIndex++;
      }
      else
      {
        if (_contentIndex == startIndex)
        {
          _contentIndex += foundLexeme.Length;
          foundTerminal = internalToken.Terminal;
        }
        break;
      }
    }

    var length = _contentIndex - startIndex;
    var lexeme = _content.Substring(startIndex, length);

    return new Token(foundTerminal, lexeme, scontext, _lastSpecial);
  }

  private InternalToken MatchKeyword()
  {
    Terminal foundKeyword = null;
    foreach (Terminal keyword in _context.Keywords)
    {
      string lexeme = keyword.Pattern;
      if (_contentIndex + lexeme.Length <= _content.Length)
      {
        int i = 0;
        while (i < lexeme.Length)
        {
          if (_content[_contentIndex + i] != lexeme[i]) break;
          i++;
        }
        if (i == lexeme.Length)
        {
          foundKeyword = keyword;
          break;
        }
      }
    }
    if (foundKeyword == null) return null;
    return new InternalToken(foundKeyword, foundKeyword.Pattern);
  }

  private InternalToken MatchFlexTerminal()
  {
    Terminal foundFlex = null;
    string foundFlexLexeme = null;
    foreach (Matcher matcher in _context.Flex)
    {
      var match = matcher.Regex.Match(_content, _contentIndex);
      if (match.Success)
      {
        bool useNew = SelectFlex(foundFlex, matcher, match.Value, foundFlexLexeme);
        if (useNew)
        {
          foundFlex = matcher.Terminal;
          foundFlexLexeme = match.Value;
        }
      }
    }
    if (foundFlex == null) return null;
    return new InternalToken(foundFlex, foundFlexLexeme);
  }

  private static bool SelectFlex(Terminal foundFlex, Matcher matcher, string match, string foundFlexLexeme)
  {
    bool useNew = false;
    if (foundFlex != null)
    {
      if (match.Length > foundFlexLexeme.Length) useNew = true;
      else if (match.Length == foundFlexLexeme.Length)
        useNew = matcher.Terminal.Priority > foundFlex.Priority;
    }
    else useNew = true;
    return useNew;
  }

  private static InternalToken Select(InternalToken foundKeyword, InternalToken foundFlex)
  {
    if (foundKeyword != null && foundFlex != null)
    {
      if (foundKeyword.Lexeme.Length > foundFlex.Lexeme.Length)
        return foundKeyword;
      else if (foundKeyword.Lexeme.Length < foundFlex.Lexeme.Length)
        return foundFlex;
      else
      {
        if (foundKeyword.Terminal.Priority < foundFlex.Terminal.Priority)
          return foundFlex;
        else
          return foundKeyword;
      }
    }
    else if (foundKeyword != null)
      return foundKeyword;
    else
      return foundFlex;
  }

  private sealed class InternalToken
  {
    public InternalToken(Terminal terminal, string lexeme)
    {
      ArgumentNullException.ThrowIfNull(terminal);
      Terminal = terminal;
      Lexeme = lexeme;
    }

    public string Lexeme { get; }
    public Terminal Terminal { get; }
  }

  private readonly KeywordContext _context;
  private readonly IScannerSource _source;
  private readonly string _content;
  private int _contentIndex;
  private Token _lastSpecial;
}
