// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements.Symbols;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Glr.Scanner;

/// <summary>
/// A scanner that gives keywords the highest priority.
/// </summary>
public sealed class KeywordScanner : IScanner
{
  /// <summary>
  /// Creates a new scanner with no special or skipped terminals.
  /// </summary>
  /// <param name="eof">The end-of-file symbol used when the end has been reached.</param>
  /// <param name="terminals">The set of normal terminals.</param>
  public KeywordScanner(Terminal eof, ISet<Terminal> terminals) : this(eof, terminals, null, null) { }

  /// <summary>
  /// Creates a new scanner with skipped terminals and no special terminals.
  /// </summary>
  /// <param name="eof">The end-of-file symbol used when the end has been reached.</param>
  /// <param name="terminals">The set of normal terminals.</param>
  /// <param name="skipTerminals">The set of terminals that are skipped while parsing but kept in the syntax tree.</param>
  public KeywordScanner(Terminal eof, ISet<Terminal> terminals, ISet<Terminal> skipTerminals) : this(eof, terminals, null, skipTerminals) { }

  /// <summary>
  /// Creates a new scanner with skipped and special terminals.
  /// </summary>
  /// <param name="eof">The end-of-file symbol used when the end has been reached.</param>
  /// <param name="terminals">The set of normal terminals.</param>
  /// <param name="specialTerminals">The set of terminals that are skipped while parsing.</param>
  /// <param name="skipTerminals">The set of terminals that are skipped while parsing but kept in the syntax tree.</param>
  public KeywordScanner(Terminal eof, ISet<Terminal> terminals, ISet<Terminal> specialTerminals, ISet<Terminal> skipTerminals)
  {
    _context = new KeywordContext(eof);

    var allTerminals = new HashSet<Terminal>();
    if (terminals != null) allTerminals.UnionWith(terminals);
    if (specialTerminals != null)
    {
      _context.SpecialTerminals.UnionWith(specialTerminals);
      allTerminals.UnionWith(specialTerminals);
    }
    if (skipTerminals != null)
    {
      _context.SkipTerminals.UnionWith(skipTerminals);
      allTerminals.UnionWith(skipTerminals);
    }

    foreach (Terminal terminal in allTerminals)
    {
      if (terminal.IsKeyword) _context.Keywords.Add(terminal);
      else
      {
        string pattern = "\\G(?:" + terminal.Pattern + ")";
        Matcher matcher = new Matcher(terminal, new Regex(pattern, RegexOptions.Singleline));
        _context.Flex.Add(matcher);
      }
    }

    _context.Keywords.Sort((t1, t2) => -t1.Name.CompareTo(t2.Name));
    _context.Flex.Sort((m1, m2) => m2.Terminal.Priority - m1.Terminal.Priority);
  }

  /// <inheritdoc/>
  public Terminal EofTerminal => _context.EofTerminal;

  /// <inheritdoc/>
  public IEnumerable<Token> GetTokenSequence(IScannerSource source)
  {
    return new TokenSequence(source, _context);
  }

  private sealed class TokenSequence : IEnumerable<Token>
  {
    public TokenSequence(IScannerSource source, KeywordContext context)
    {
      _source = source;
      _context = context;
    }

    public IEnumerator<Token> GetEnumerator()
    {
      return new KeywordTokenEnumerator(_source, _context);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    private readonly IScannerSource _source;
    private readonly KeywordContext _context;
  }

  private readonly KeywordContext _context;
}
