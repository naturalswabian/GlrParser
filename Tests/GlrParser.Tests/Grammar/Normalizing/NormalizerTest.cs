// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using AwesomeAssertions;
using Glr.Grammar;
using Glr.Grammar.Elements;
using Glr.Grammar.Elements.Sequence;
using Glr.Grammar.Elements.Symbols;
using Glr.Grammar.Normalizing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Glr.Tests.Grammar.Normal;

/// <summary>
/// Tests for normalizing production rules performed in <see cref="Normalizer"/>.
/// </summary>
[TestClass]
public class NormalizerTests
{
  /// <summary>
  /// Test normalization of P -> a.
  /// </summary>
  [TestMethod]
  public void TestSimple()
  {
    // arrange
    var a = new Terminal("a");
    var p = new NonTerminal("P");
    var production = new Production<IElement>(p, a);
    var normalizer = new Normalizer();

    // act
    var result = normalizer.Normalize(production);

    // assert
    result.Count.Should().Be(1);
    result[0].Should().Be(new Production<IAbstractSymbol>(p, a));
  }

  /// <summary>
  /// Test normalization of <c>P -&gt; a a a</c>.
  /// </summary>
  [TestMethod]
  public void TestSequence()
  {
    // arrange
    var a = new Terminal("a");
    var p = new NonTerminal("P");
    var production = new Production<IElement>(p, a, a, a);
    var normalizer = new Normalizer();

    // act
    var result = normalizer.Normalize(production);

    // assert
    result.Count.Should().Be(1);
    result[0].Should().Be(new Production<IAbstractSymbol>(p, a, a, a));
  }

  /// <summary>
  /// Test normalization of P -&gt; a?.
  /// </summary>
  [TestMethod]
  public void TestOptional1()
  {
    // arrange
    var a = new Terminal("a");
    var p = new NonTerminal("P");
    var production = new Production<IElement>(p, new OptionalElement(a));
    var normalizer = new Normalizer();

    // act
    var result = normalizer.Normalize(production);

    // assert
    result.Count.Should().Be(2);
    result[0].Should().Be(new Production<IAbstractSymbol>(p, a));
    result[1].Should().Be(new Production<IAbstractSymbol>(p));
  }

  /// <summary>
  /// Test normalization of P -&gt; a a?.
  /// </summary>
  [TestMethod]
  public void TestOptional2()
  {
    // arrange
    var a = new Terminal("a");
    var p = new NonTerminal("P");
    var production = new Production<IElement>(p, a, new OptionalElement(a));
    var normalizer = new Normalizer();

    // act
    var result = normalizer.Normalize(production);

    // assert
    result.Count.Should().Be(2);
    result[0].Should().Be(new Production<IAbstractSymbol>(p, a, a));
    result[1].Should().Be(new Production<IAbstractSymbol>(p, a));
  }

  /// <summary>
  /// Test normalization of P -&gt; a P?.
  /// </summary>
  [TestMethod]
  public void TestOptional3()
  {
    // arrange
    var a = new Terminal("a");
    var p = new NonTerminal("P");
    var production = new Production<IElement>(p, a, new OptionalElement(p));
    var normalizer = new Normalizer();

    // act
    var result = normalizer.Normalize(production);

    // assert
    result.Count.Should().Be(2);
    result[0].Should().Be(new Production<IAbstractSymbol>(p, a, p));
    result[1].Should().Be(new Production<IAbstractSymbol>(p, a));
  }

  /// <summary>
  /// Test normalization of P -&gt; P? a.
  /// </summary>
  [TestMethod]
  public void TestOptional4()
  {
    // arrange
    var a = new Terminal("a");
    var p = new NonTerminal("P");
    var production = new Production<IElement>(p, new OptionalElement(p), a);
    var normalizer = new Normalizer();

    // act
    var result = normalizer.Normalize(production);

    // assert
    result.Count.Should().Be(2);
    result[0].Should().Be(new Production<IAbstractSymbol>(p, p, a));
    result[1].Should().Be(new Production<IAbstractSymbol>(p, a));
  }

  /// <summary>
  /// Test normalization of P -&gt; a | b.
  /// </summary>
  [TestMethod]
  public void TestAlternation()
  {
    // arrange
    var a = new Terminal("a");
    var b = new Terminal("b");
    var p = new NonTerminal("P");
    var production = new Production<IElement>(p, new AlternativeList(a, b));
    var normalizer = new Normalizer();

    // act
    var result = normalizer.Normalize(production);

    // assert
    result.Count.Should().Be(2);
    result[0].Should().Be(new Production<IAbstractSymbol>(p, a));
    result[1].Should().Be(new Production<IAbstractSymbol>(p, b));
  }

  /// <summary>
  /// Test normalization of P -&gt; a+.
  /// </summary>
  [TestMethod]
  public void TestOnceOrMore()
  {
    // arrange
    var a = new Terminal("a");
    var p = new NonTerminal("P");
    var production = new Production<IElement>(p, new OnceOrMoreElement(a));
    var normalizer = new Normalizer();

    // act
    var result = normalizer.Normalize(production);

    // assert
    result.Count.Should().Be(3);
    var n = result[0].LeftSideSymbol;
    result[0].Should().Be(new Production<IAbstractSymbol>(n, n, a));
    result[1].Should().Be(new Production<IAbstractSymbol>(n, a));
    result[2].Should().Be(new Production<IAbstractSymbol>(p, n));
  }

  /// <summary>
  /// Test normalization of P -&gt; a*.
  /// </summary>
  [TestMethod]
  public void TestZeroOrMore()
  {
    // arrange
    var a = new Terminal("a");
    var p = new NonTerminal("P");
    var production = new Production<IElement>(p, new ZeroOrMoreElement(a));
    var normalizer = new Normalizer();

    // act
    var result = normalizer.Normalize(production);

    // assert
    result.Count.Should().Be(4);
    var n = result[0].LeftSideSymbol;
    result[0].Should().Be(new Production<IAbstractSymbol>(n, n, a));
    result[1].Should().Be(new Production<IAbstractSymbol>(n, a));
    result[2].Should().Be(new Production<IAbstractSymbol>(p, n));
    result[3].Should().Be(new Production<IAbstractSymbol>(p));
  }

  /// <summary>
  /// Test normalization of P -&gt; (a &amp; b &amp; c).
  /// </summary>
  [TestMethod]
  public void TestUnorderedGroup()
  {
    // arrange
    var a = new Terminal("a");
    var b = new Terminal("b");
    var c = new Terminal("c");
    var p = new NonTerminal("P");
    var production = new Production<IElement>(p, new UnorderedGroup(a, b, c));
    var normalizer = new Normalizer();

    // act
    var result = normalizer.Normalize(production);

    // assert
    result.Count.Should().Be(6);
    result[0].Should().Be(new Production<IAbstractSymbol>(p, a, b, c));
    result[1].Should().Be(new Production<IAbstractSymbol>(p, a, c, b));
    result[2].Should().Be(new Production<IAbstractSymbol>(p, b, a, c));
    result[3].Should().Be(new Production<IAbstractSymbol>(p, b, c, a));
    result[4].Should().Be(new Production<IAbstractSymbol>(p, c, a, b));
    result[5].Should().Be(new Production<IAbstractSymbol>(p, c, b, a));
  }
}
