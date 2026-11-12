// Generalized LR Parser
// Copyright © 2020 Jan B. Freuer
// All rights reserved

using AwesomeAssertions;
using Glr.Grammar;
using Glr.Parser;
using Glr.Parser.Table;
using Glr.Scanner;
using Glr.Tree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GlrParser.Tests.Parser;

[TestClass]
public class SimpleEnumeration
{
  [TestMethod]
  public void Enumeration()
  {
    // arrange
    var grammar = AbcGrammar.CreateGrammar();
    var parser = CreateParser(grammar);
    var context = new SourceContext(0, "test", 0, 0);
    var tokens = new Token[]
    {
      new Token(grammar.GetTerminal("a"), "a", context, null),
      new Token(grammar.GetTerminal("b"), "b", context, null),
      new Token(grammar.GetTerminal("c"), "c", context, null),
    };

    // act
    var result = parser.Parse(tokens);

    // assert
    result.Trees.Length.Should().Be(1);
    result.Trees[0].Should().Be(GetExpectedTreeABC(grammar));
  }

  private static GlrParser<ItemState, DefaultTreeNode> CreateParser(DefaultGrammar grammar)
  {
    var table = new DynamicCachingTable(grammar);
    var nodeFactory = new DefaultTreeNodeFactory();
    var parser = new GlrParser<ItemState, DefaultTreeNode>(table, nodeFactory);
    return parser;
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
}
