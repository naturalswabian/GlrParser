// Generalized LR Parser
// Copyright © 2020 Jan B. Freuer
// All rights reserved

using AwesomeAssertions;
using Glr.Grammar;
using Glr.Parser;
using Glr.Parser.Table;
using Glr.Scanner;
using Glr.Tree;
using Glr.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace GlrParser.Tests.Parser;

[TestClass]
public class ErrorRecoveryTests
{
  /// <summary>
  /// Tests if the correct input <c>abc</c>
  /// is parsed without any errors.
  /// </summary>
  [TestMethod]
  public void Normal()
  {
    // arrange
    var grammar = AbcGrammar.CreateGrammar();
    var parser = CreateParser(grammar);
    var scanner = new KeywordScanner(grammar.EofSymbol, grammar.Terminals);
    var input = "abc";

    // act
    var result = parser.Parse(scanner.GetTokenSequence(Source(input)));

    // assert
    result.Trees.Length.Should().Be(1);
    result.Trees[0].Should().Be(GetExpectedTreeABC(grammar));
    result.Errors.Length.Should().Be(0);
  }

  /// <summary>
  /// Tests if the wrong input <c>ac</c> (<c>b</c> is missing)
  /// is parsed with errors and <b>no</b> error recovery.
  /// </summary>
  [TestMethod]
  public void Fail()
  {
    // arrange
    var grammar = AbcGrammar.CreateGrammar();
    var parser = CreateParser(grammar);
    var scanner = new KeywordScanner(grammar.EofSymbol, grammar.Terminals);
    var input = "ac";

    // act
    Action parseAction = () => parser.Parse(scanner.GetTokenSequence(Source(input)));

    // assert
    parseAction.Should().Throw<SyntaxException>();
  }

  /// <summary>
  /// Tests if the wrong input <c>ac</c> (<c>b</c> is missing)
  /// is parsed with errors and error recovery.
  /// </summary>
  [TestMethod]
  public void InjectB()
  {
    // arrange
    var grammar = AbcGrammar.CreateGrammar();
    var parser = CreateParser(grammar);
    var scanner = new KeywordScanner(grammar.EofSymbol, grammar.Terminals);
    var input = "ac";
    parser.IsRecoveryEnabled = true;

    // act
    var result = parser.Parse(scanner.GetTokenSequence(Source(input)));

    // assert
    result.Trees.Length.Should().Be(1);
    result.Trees[0].Should().Be(GetExpectedTreeABC(grammar));
    result.Errors.Length.Should().Be(1);
    result.Errors[0].Token.Terminal.Should().Be(grammar.GetTerminal("c"));
  }

  /// <summary>
  /// Tests if the wrong input <c>axbc</c> (<c>x</c> is wrong)
  /// is parsed with errors and error recovery.
  /// </summary>
  [TestMethod]
  public void RemoveSingleX1()
  {
    // arrange
    var grammar = AbcGrammar.CreateGrammar();
    var parser = CreateParser(grammar);
    var scanner = new KeywordScanner(grammar.EofSymbol, grammar.Terminals);
    var input = "axbc";
    parser.IsRecoveryEnabled = true;

    // act
    var result = parser.Parse(scanner.GetTokenSequence(Source(input)));

    // assert
    result.Trees.Length.Should().Be(1);
    result.Trees[0].Should().Be(GetExpectedTreeABC(grammar));
    result.Errors.Length.Should().Be(1);
    result.Errors[0].Token.Lexeme.Should().Be("x");
  }

  /// <summary>
  /// Tests if the wrong input <c>axbxc</c> (both <c>x</c> are wrong)
  /// is parsed with errors and error recovery.
  /// </summary>
  [TestMethod]
  public void RemoveSingleXSingleX()
  {
    // arrange
    var grammar = AbcGrammar.CreateGrammar();
    var parser = CreateParser(grammar);
    var scanner = new KeywordScanner(grammar.EofSymbol, grammar.Terminals);
    var input = "axbxc";
    parser.IsRecoveryEnabled = true;

    // act
    var result = parser.Parse(scanner.GetTokenSequence(Source(input)));

    // assert
    result.Trees.Length.Should().Be(1);
    result.Trees[0].Should().Be(GetExpectedTreeABC(grammar));
    result.Errors.Length.Should().Be(2);
    result.Errors[0].Token.Lexeme.Should().Be("x");
    result.Errors[1].Token.Lexeme.Should().Be("x");
  }

  /// <summary>
  /// Tests if the wrong input <c>abcx</c> (<c>x</c> is wrong)
  /// is parsed with errors and error recovery.
  /// </summary>
  [TestMethod]
  public void RemoveSingleX2()
  {
    // arrange
    var grammar = AbcGrammar.CreateGrammar();
    var parser = CreateParser(grammar);
    var scanner = new KeywordScanner(grammar.EofSymbol, grammar.Terminals);
    var input = "abcx";
    parser.IsRecoveryEnabled = true;

    // act
    var result = parser.Parse(scanner.GetTokenSequence(Source(input)));

    // assert
    result.Trees.Length.Should().Be(1);
    result.Trees[0].Should().Be(GetExpectedTreeABC(grammar));
    result.Errors.Length.Should().Be(1);
    result.Errors[0].Token.Lexeme.Should().Be("x");
  }

  /// <summary>
  /// Tests if the wrong input <c>axxbc</c> (all <c>x</c> are wrong)
  /// is parsed with errors and error recovery.
  /// </summary>
  [TestMethod]
  [Ignore("error recovery is not fully functional")]
  public void RemoveMultiX()
  {
    // arrange
    var grammar = AbcGrammar.CreateGrammar();
    var parser = CreateParser(grammar);
    var scanner = new KeywordScanner(grammar.EofSymbol, grammar.Terminals);
    var input = "axxbc";
    parser.IsRecoveryEnabled = true;

    // act
    var result = parser.Parse(scanner.GetTokenSequence(Source(input)));

    // assert
    result.Trees.Length.Should().Be(1);
    result.Trees[0].Should().Be(GetExpectedTreeAC(grammar));
    result.Errors.Length.Should().Be(1);
    result.Errors[0].Token.Lexeme.Should().Be("x");
  }

  /// <summary>
  /// Tests if the wrong input <c>abac</c> (the second <c>a</c> is wrong)
  /// is parsed with errors and error recovery.
  /// </summary>
  [TestMethod]
  public void RemoveSingleA()
  {
    // arrange
    var grammar = AbcGrammar.CreateGrammar();
    var parser = CreateParser(grammar);
    var scanner = new KeywordScanner(grammar.EofSymbol, grammar.Terminals);
    var input = "abac";
    parser.IsRecoveryEnabled = true;

    // act
    var result = parser.Parse(scanner.GetTokenSequence(Source(input)));

    // assert
    result.Trees.Length.Should().Be(1);
    result.Trees[0].Should().Be(GetExpectedTreeABC(grammar));
    result.Errors.Length.Should().Be(1);
    result.Errors[0].Token.Lexeme.Should().Be("a");
  }

  /// <summary>
  /// Tests if the wrong input <c>abac</c> (the second <c>a</c> is wrong)
  /// is parsed with errors and error recovery.
  /// </summary>
  [TestMethod]
  public void RemoveMultipleA()
  {
    // arrange
    var grammar = AbcGrammar.CreateGrammar();
    var parser = CreateParser(grammar);
    var scanner = new KeywordScanner(grammar.EofSymbol, grammar.Terminals);
    var input = "abaaaac";
    parser.IsRecoveryEnabled = true;

    // act
    var result = parser.Parse(scanner.GetTokenSequence(Source(input)));

    // assert
    result.Trees.Length.Should().Be(1);
    result.Trees[0].Should().Be(GetExpectedTreeAC(grammar)); // todo: should be also abc?
    result.Errors.Length.Should().Be(1);
    result.Errors[0].Token.Lexeme.Should().Be("a");
  }

  private static DefaultTreeNode GetExpectedTreeABC(DefaultGrammar grammar)
  {
    return new TestTree(grammar,
      new TestTree.Node("content",
        new TestTree.Node("a"),
        new TestTree.Node("b"),
        new TestTree.Node("c")))
      .AsTree();
  }

  private static DefaultTreeNode GetExpectedTreeAC(DefaultGrammar grammar)
  {
    return new TestTree(grammar,
      new TestTree.Node("content",
        new TestTree.Node("a"),
        new TestTree.Node("c")))
      .AsTree();
  }

  /// <summary>
  /// Creates a scanner source for the given input.
  /// </summary>
  private static IScannerSource Source(string input)
  {
    return new ReaderScannerSource(input, input);
  }

  private static GlrParser<ItemState, DefaultTreeNode> CreateParser(DefaultGrammar grammar)
  {
    var table = new DynamicCachingTable(grammar);
    var nodeFactory = new DefaultTreeNodeFactory();
    var parser = new GlrParser<ItemState, DefaultTreeNode>(table, nodeFactory);
    return parser;
  }

  private void Dump(DefaultTreeNode node)
  {
    var stringWriter = new StringWriter();
    var nodeWriter = new DefaultTreeNodeWriter(stringWriter) { Recursive = true };
    nodeWriter.Write(node);
    System.Diagnostics.Debug.WriteLine(stringWriter);
  }
}
