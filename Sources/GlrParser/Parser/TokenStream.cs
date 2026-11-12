// Generalized LR Parser
// Copyright © 2020 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements.Symbols;
using Glr.Scanner;
using System;
using System.Collections.Generic;
using System.Text;

namespace Glr.Parser;

/// <summary>
/// A stream of <see cref="Token"/>s that allows to insert and
/// remove tokens during parsing.
/// </summary>
internal sealed class TokenStream
{
  /// <summary>
  /// Creates a new token stream that will contain the given tokens.
  /// </summary>
  public TokenStream(IEnumerable<Token> tokens, Terminal eof)
  {
    _eof = eof;
    foreach (var token in tokens)
    {
      if (token.Terminal == eof)
        break;
      _tokenList.Add(token);
    }
  }

  /// <summary>
  /// Is <see langword="true"/>, when this token stream is empty.
  /// </summary>
  public bool IsEmpty => _tokenList.Count == 0;

  /// <summary>
  /// Creates a new snapshot of the current state of this token stream.
  /// </summary>
  public ITokenStreamSnapshot CreateSnapshot()
  {
    return new TokenStreamSnapshot(this);
  }

  /// <summary>
  /// Restores the given state using the given snapshot.
  /// </summary>
  public void Restore(ITokenStreamSnapshot streamSnapshot)
  {
    if (!(streamSnapshot is TokenStreamSnapshot snapshot) || snapshot.Owner != this)
      throw new ArgumentException("Cannot restore snapshot", nameof(streamSnapshot));

    for (int i = snapshot.Changes.Count - 1; i >= 0; i--)
    {
      var change = snapshot.Changes[i];
      switch (change.Kind)
      {
        case ChangeKind.Insert:
          Remove(change.Index);
          break;
        case ChangeKind.Remove:
          Insert(change.Index, change.Token);
          break;
      }
    }

    Position = snapshot.Position;
  }

  /// <summary>
  /// Gets the current read position of this token stream.
  /// </summary>
  public int Position { get; private set; }

  /// <summary>
  /// Moves the read cursor to the given relative amount forward or backwards.
  /// </summary>
  public void Reset(int offset)
  {
    var resultPosition = Position + offset;
    if (resultPosition < 0 || resultPosition >= _tokenList.Count)
      throw new ArgumentOutOfRangeException(nameof(offset));
    Position += offset;
  }

  /// <summary>
  /// Gets the token relative to the given offset. When the offset is larger
  /// than the number of tokens contained within this stream the <c>eof</c>
  /// symbol is returned.
  /// </summary>
  public Token GetToken(int offset)
  {
    var index = Position + offset;
    if (index < 0) return null;
    if (index >= _tokenList.Count)
    {
      var lastToken = _tokenList[_tokenList.Count - 1];
      var tokenLength = lastToken.Lexeme.Length;
      var inputLength = lastToken.SourceContext.StartIndex + tokenLength;
      var resource = lastToken.SourceContext.Name;
      var line = lastToken.SourceContext.LineNumber;
      var column = lastToken.SourceContext.ColumnNumber + tokenLength;
      var context = new SourceContext(inputLength, resource, line, column);
      return new Token(_eof, null, context, null);
    }
    return _tokenList[index];
  }

  /// <summary>
  /// Consumes the current token.
  /// </summary>
  public void Next()
  {
    Position++;
  }

  /// <inheritdoc/>
  public override string ToString() => ToString(0); // Position

  /// <summary>
  /// Gets the contents (tokens) of this token stream as string
  /// while starting at the given position.
  /// </summary>
  public string ToString(int startIndex)
  {
    var result = new StringBuilder();

    int i = startIndex;
    var endPosition = Math.Min(_tokenList.Count, Position + 3);
    while (i < endPosition)
    {
      if (i == Position)
      {
        result.Append('#');
        result.Append(' ');
      }
      result.Append(_tokenList[i]);
      result.Append(' ');
      i++;
    }
    if (i == _tokenList.Count)
      result.Append(_eof);
    else if (i < _tokenList.Count)
      result.Append("...");

    return result.ToString();
  }

  private void Insert(int index, Token token)
  {
    if (index < 0) throw new InvalidOperationException();
    if (index > _tokenList.Count) throw new InvalidOperationException();
    _tokenList.Insert(index, token);
  }

  private void Remove(int index)
  {
    if (index < 0) throw new InvalidOperationException();
    if (index >= _tokenList.Count) throw new InvalidOperationException();
    _tokenList.RemoveAt(index);
  }

  private enum ChangeKind
  {
    Insert, Remove
  }


  private sealed class Change
  {
    public Change(ChangeKind kind, Token token, int index)
    {
      Kind = kind;
      Index = index;
      Token = token;
    }

    public Token Token { get; }
    public ChangeKind Kind { get; }
    public int Index { get; }
  }

  private sealed class TokenStreamSnapshot : ITokenStreamSnapshot
  {
    public TokenStreamSnapshot(TokenStream owner)
    {
      Owner = owner;
      Position = owner.Position;
    }

    public TokenStream Owner { get; }
    public int Position { get; }

    public List<Change> Changes { get; } = [];

    public void Insert(int offset, Token token)
    {
      var index = Owner.Position + offset;
      Owner.Insert(index, token);
      Changes.Add(new Change(ChangeKind.Insert, token, index));
    }

    public void Remove(int offset)
    {
      var index = Owner.Position + offset;
      var token = Owner._tokenList[index];
      Owner.Remove(index);
      Changes.Add(new Change(ChangeKind.Remove, token, index));
    }
  }

  public const int Last = -1;
  public const int Current = 0;
  public const int LookAhead = 1;
  public const int LookAhead2 = 2;

  private readonly Terminal _eof;
  private readonly List<Token> _tokenList = new List<Token>();
}
