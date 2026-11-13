// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using AwesomeAssertions;
using Glr.Meta;
using Glr.Meta.Attributes;
using Glr.Scanner;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Glr.Tests.Meta.Grammar;

[TestClass]
public class GrammarTests
{
  /// <summary>
  /// Tests the input "<c>a</c>" for:
  /// <inheritdoc cref="S1"/>
  /// </summary>
  [TestMethod]
  public void Test_S1_AssignmentParserAction()
  {
    // arrange
    var parser = ParserFactory.CreateParser<S1>();
    var source = new ReaderScannerSource(nameof(GrammarTests), "a");

    // act
    var result = parser.Parse(source, null);

    // assert
    result.Model.Content.Content.Should().Be("a");
  }

  /// <summary>
  /// Tests the input "<c>a</c>" for:
  /// <inheritdoc cref="S2"/>
  /// </summary>
  [TestMethod]
  public void Test_S2_AddParserAction()
  {
    // arrange
    var parser = ParserFactory.CreateParser<S2>();
    var source = new ReaderScannerSource(nameof(GrammarTests), "a");

    // act
    var result = parser.Parse(source, null);

    // assert
    result.Model.ListOfA.Count.Should().Be(1);
    result.Model.ListOfA[0].Content.Should().Be("a");
  }

  /// <summary>
  /// Tests the input <c>a</c> for:
  /// <inheritdoc cref="S3"/>
  /// </summary>
  [TestMethod]
  public void Test_S3_AddParserAction1()
  {
    // arrange
    var parser = ParserFactory.CreateParser<S3>();
    var source = new ReaderScannerSource(nameof(GrammarTests), "a");

    // act
    var result = parser.Parse(source, null);

    // assert
    result.Model.ListOfA.Count.Should().Be(1);
    result.Model.ListOfA[0].Content.Should().Be("a");
  }

  /// <summary>
  /// Tests the input "<c>aaa</c>" for:
  /// <inheritdoc cref="S3"/>
  /// </summary>
  [TestMethod]
  public void Test_S3_AddParserAction2()
  {
    // arrange
    var parser = ParserFactory.CreateParser<S3>();
    var source = new ReaderScannerSource(nameof(GrammarTests), "aaa");

    // act
    var result = parser.Parse(source, null);

    // assert
    result.Model.ListOfA.Count.Should().Be(3);
    result.Model.ListOfA[0].Content.Should().Be("a");
    result.Model.ListOfA[1].Content.Should().Be("a");
    result.Model.ListOfA[2].Content.Should().Be("a");
  }

  /// <summary>
  /// Tests the input "<c>aaabbb</c>" for:
  /// <inheritdoc cref="S4"/>
  /// </summary>
  [TestMethod]
  public void Test_S4_AddParserAction()
  {
    // arrange
    var parser = ParserFactory.CreateParser<S4>();
    var source = new ReaderScannerSource(nameof(GrammarTests), "aaabbb");

    // act
    var result = parser.Parse(source, null);

    // assert
    result.Model.ListOfA.Count.Should().Be(3);
    result.Model.ListOfA[0].Content.Should().Be("a");
    result.Model.ListOfA[1].Content.Should().Be("a");
    result.Model.ListOfA[2].Content.Should().Be("a");
    result.Model.ListOfB.Count.Should().Be(3);
    result.Model.ListOfB[0].Content.Should().Be("b");
    result.Model.ListOfB[1].Content.Should().Be("b");
    result.Model.ListOfB[2].Content.Should().Be("b");
  }

  /// <summary>
  /// Tests the input "<c>aaabbb</c>" for:
  /// <inheritdoc cref="S5"/>
  /// </summary>
  [TestMethod]
  public void Test_S5_AddParserAction()
  {
    // arrange
    var parser = ParserFactory.CreateParser<S5>();
    var source = new ReaderScannerSource(nameof(GrammarTests), "aaabbb");

    // act
    var result = parser.Parse(source, null);

    // assert
    result.Model.ListOfA.Count.Should().Be(3);
    result.Model.ListOfA[0].Content.Should().Be("a");
    result.Model.ListOfA[1].Content.Should().Be("a");
    result.Model.ListOfA[2].Content.Should().Be("a");
    result.Model.ListOfB.Count.Should().Be(3);
    result.Model.ListOfB[0].Content.Should().Be("b");
    result.Model.ListOfB[1].Content.Should().Be("b");
    result.Model.ListOfB[2].Content.Should().Be("b");
  }

  /// <summary>
  /// Tests the empty input for:
  /// <inheritdoc cref="S5"/>
  /// </summary>
  [TestMethod]
  public void Test_S5_Empty()
  {
    // arrange
    var parser = ParserFactory.CreateParser<S5>();
    var source = new ReaderScannerSource(nameof(GrammarTests), "");

    // act
    var result = parser.Parse(source, null);

    // assert
    result.ModelMap.Should().BeNull();
    result.Model.Should().BeNull();
    result.Tree.Should().BeNull();
  }

  #region Start Rules

  /// <summary>
  /// <code>
  /// S1 -> Content = A
  /// A -> Content = 'a'
  /// </code>
  /// </summary>
  [Rule("Content = A")]
  public class S1
  {
    public A Content { set; get; }
  }

  /// <summary>
  /// <code>
  /// S2 -> ListOfA += A
  /// A -> Content = 'a'
  /// </code>
  /// </summary>
  [Rule("ListOfA += A")]
  public class S2
  {
    public List<A> ListOfA { get; } = new List<A>();
  }

  /// <summary>
  /// <code>
  /// S3 -> ListOfA += A+<br/>
  /// A -> Content = 'a'<br/>
  /// </code>
  /// </summary>
  [Rule("ListOfA += A+")]
  public class S3
  {
    public List<A> ListOfA { get; } = new List<A>();
  }

  /// <summary>
  /// <code>
  /// S4 -> ListOfA += A+  ListOfB += B+<br/>
  /// A -> Content = 'a'<br/>
  /// B -> Content = 'b'<br/>
  /// </code>
  /// </summary>
  [Rule("ListOfA += A+ ListOfB += B+")]
  public class S4
  {
    public List<A> ListOfA { get; } = new List<A>();
    public List<B> ListOfB { get; } = new List<B>();
  }

  /// <summary>
  /// <code>
  /// S4 -> ListOfA += A*  ListOfB += B*<br/>
  /// A -> Content = 'a'<br/>
  /// B -> Content = 'b'<br/>
  /// </code>
  /// </summary>
  [Rule("ListOfA += A* ListOfB += B*")]
  public class S5
  {
    public List<A> ListOfA { get; } = new List<A>();
    public List<B> ListOfB { get; } = new List<B>();
  }
  #endregion

  #region Common Rules
  [Rule("Content = 'a'")]
  public class A
  {
    public string Content { set; get; }
  }

  [Rule("Content = 'b'")]
  public class B
  {
    public string Content { set; get; }
  }
  #endregion
}
