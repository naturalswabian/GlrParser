// Generalized LR Parser
// Copyright © 2020 Jan B. Freuer
// All rights reserved

using AwesomeAssertions;
using Glr.Grammar.Elements.Symbols;
using Glr.Scanner;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GlrParser.Tests.Parser;

[TestClass]
public class KeywordScannerTests
{
  [TestMethod]
  public void ValidTokens()
  {
    // arrange
    var eofSymbol = new Terminal("eof");
    var terminals = new HashSet<Terminal>
    {
      new Terminal("a"),
      new Terminal("b"),
      new Terminal("c")
    };

    var scanner = new KeywordScanner(eofSymbol, terminals);
    var input = "aabccc";

    // act
    var tokens = new List<Token>(scanner.GetTokenSequence(new ReaderScannerSource("test-resource", input)));

    // assert
    for (int i = 0; i < tokens.Count; i++)
    {
      var token = tokens[i];
      var context = token.SourceContext;
      context.Should().NotBeNull();
      context.Name.Should().Be("test-resource");
      context.StartIndex.Should().Be(i);
      if (token.Terminal != eofSymbol)
        token.Lexeme.Should().Be(input.Substring(i, 1));
      else
        token.Lexeme.Should().Be(string.Empty);
    }
  }

  [TestMethod]
  public void InvalidMiddleTokens()
  {
    // arrange
    var eofSymbol = new Terminal("eof");
    var terminalA = new Terminal("a");
    var terminalB = new Terminal("b");
    var terminals = new HashSet<Terminal> { terminalA, terminalB };
    var expected = new List<Terminal> { terminalA, null, terminalB };

    var scanner = new KeywordScanner(eofSymbol, terminals);
    var input = "axyzb";

    // act
    var tokens = new List<Token>(scanner.GetTokenSequence(new ReaderScannerSource("test-resource", input)));

    // assert
    AssertTerms(tokens, expected);
  }

  [TestMethod]
  public void InvalidLeadingTokens()
  {
    // arrange
    var eofSymbol = new Terminal("eof");
    var terminalA = new Terminal("a");
    var terminalB = new Terminal("b");
    var terminals = new HashSet<Terminal> { terminalA, terminalB };
    var expected = new List<Terminal> { null, terminalA, terminalB };

    var scanner = new KeywordScanner(eofSymbol, terminals);
    var input = "xyzab";

    // act
    var tokens = new List<Token>(scanner.GetTokenSequence(new ReaderScannerSource("test-resource", input)));

    // assert
    AssertTerms(tokens, expected);
  }

  [TestMethod]
  public void InvalidTrailingTokens()
  {
    // arrange
    var eofSymbol = new Terminal("eof");
    var terminalA = new Terminal("a");
    var terminalB = new Terminal("b");
    var terminals = new HashSet<Terminal> { terminalA, terminalB };
    var expected = new List<Terminal> { terminalA, terminalB, null };

    var scanner = new KeywordScanner(eofSymbol, terminals);
    var input = "abxyz";

    // act
    var tokens = new List<Token>(scanner.GetTokenSequence(new ReaderScannerSource("test-resource", input)));

    // assert
    AssertTerms(tokens, expected);
  }

  private static void AssertTerms(IList<Token> tokens, IList<Terminal> expected)
  {
    tokens.Count.Should().Be(expected.Count);
    for (int i = 0; i < tokens.Count; i++)
    {
      var terminal = tokens[i].Terminal;
      var expectedTerminal = expected[i];
      terminal.Should().Be(expectedTerminal);
    }
  }
}
