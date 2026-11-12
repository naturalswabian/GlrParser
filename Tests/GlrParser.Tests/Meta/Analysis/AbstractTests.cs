// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using AwesomeAssertions;
using Glr.Grammar;
using Glr.Grammar.Annotations;
using Glr.Grammar.Elements.Symbols;
using Glr.Meta.Actions;
using Glr.Meta.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;

namespace Glr.Tests.Meta.Analysis;

/// <summary>
/// Tests the <see cref="Abstract"/> support class.
/// </summary>
[TestClass]
public class AbstractTests
{
  /// <summary>
  /// P : AType -> A : AType | B : AType
  /// </summary>
  [TestMethod]
  public void TestSame1()
  {
    // arrange
    var typeResolver = Substitute.For<ITypeResolver>();
    var p = new NonTerminal("P");
    var a = new NonTerminal("A");
    var b = new NonTerminal("B");
    var abs = new Abstract(typeResolver);
    var productions = new Production<IAbstractSymbol>[]
    {
      new Production<IAbstractSymbol>(p, a),
      new Production<IAbstractSymbol>(p, b)
    };

    typeResolver.GetType("A").Returns(typeof(AType));
    typeResolver.GetType("B").Returns(typeof(AType));
    typeResolver.GetType("P").Returns(typeof(AType));

    // act
    var result = abs.GetAbstract(productions);

    // assert
    result.Count.Should().Be(1);
    result.First().Should().Be(p);
  }

  /// <summary>
  /// P : AType -> A : ASubType1 | B : ASubType2
  /// </summary>
  [TestMethod]
  public void TestSame2()
  {
    // arrange
    var typeResolver = Substitute.For<ITypeResolver>();
    var p = new NonTerminal("P");
    var a = new NonTerminal("A");
    var b = new NonTerminal("B");
    var abs = new Abstract(typeResolver);
    var productions = new Production<IAbstractSymbol>[]
    {
      new Production<IAbstractSymbol>(p, a),
      new Production<IAbstractSymbol>(p, b)
    };

    typeResolver.GetType("A").Returns(typeof(ASubType1));
    typeResolver.GetType("B").Returns(typeof(ASubType2));
    typeResolver.GetType("P").Returns(typeof(AType));

    // act
    var result = abs.GetAbstract(productions);

    // assert
    result.Count.Should().Be(1);
    result.First().Should().Be(p);
  }

  /// <summary>
  /// P : AType -> A : AType | B : BType
  /// </summary>
  [TestMethod]
  public void NotTestSame1()
  {
    // arrange
    var typeResolver = Substitute.For<ITypeResolver>();
    var p = new NonTerminal("P");
    var a = new NonTerminal("A");
    var b = new NonTerminal("B");
    var abs = new Abstract(typeResolver);
    var productions = new Production<IAbstractSymbol>[]
    {
      new Production<IAbstractSymbol>(p, a),
      new Production<IAbstractSymbol>(p, b)
    };

    typeResolver.GetType("A").Returns(typeof(ASubType1));
    typeResolver.GetType("B").Returns(typeof(BType));
    typeResolver.GetType("P").Returns(typeof(AType));

    // act
    var result = abs.GetAbstract(productions);

    // assert
    result.Count.Should().Be(0);
  }

  /// <summary>
  /// P : AType -> A : ASubType1 | (assign B) : BType
  /// </summary>
  [TestMethod]
  public void TestWithAssignmentOperation()
  {
    var property = typeof(ClassWithProperties).GetProperty(nameof(ClassWithProperties.IntValue));
    var assignmentOperation = new AssignmentParserAction(property);
    TestWithStorageOperation(assignmentOperation);
  }

  /// <summary>
  /// P : AType -> A : ASubType1 | (exists B) : BType
  /// </summary>
  [TestMethod]
  public void TestWithExistsOperation()
  {
    var property = typeof(ClassWithProperties).GetProperty(nameof(ClassWithProperties.BoolValue));
    var assignmentOperation = new ExistsParserAction(property);
    TestWithStorageOperation(assignmentOperation);
  }

  /// <summary>
  /// P : AType -> A : ASubType1 | (add B) : BType
  /// </summary>
  [TestMethod]
  public void TestWithAddOperation()
  {
    var property = typeof(ClassWithProperties).GetProperty(nameof(ClassWithProperties.IntValues));
    var assignmentOperation = new AddParserAction(property);
    TestWithStorageOperation(assignmentOperation);
  }

  /// <summary>
  /// P : AType -> a : ASubType1 | '(' b : ASubType2 ')'
  /// </summary>
  [TestMethod]
  public void TestSameWithTerminals()
  {
    // arrange
    var typeResolver = Substitute.For<ITypeResolver>();
    var p = new NonTerminal("P");
    var a = new NonTerminal("A");
    var b = new NonTerminal("B");
    var leftParen = new Terminal("(");
    var rightParen = new Terminal(")");
    var abs = new Abstract(typeResolver);
    var productions = new Production<IAbstractSymbol>[]
    {
      new Production<IAbstractSymbol>(p, a),
      new Production<IAbstractSymbol>(p, leftParen, b, rightParen)
    };

    typeResolver.GetType("A").Returns(typeof(ASubType1));
    typeResolver.GetType("B").Returns(typeof(ASubType2));
    typeResolver.GetType("P").Returns(typeof(AType));

    // act
    var result = abs.GetAbstract(productions);

    // assert
    result.Count.Should().Be(1);
    result.First().Should().Be(p);
  }

  private static void TestWithStorageOperation(IAnnotation storageAnnotation)
  {
    // arrange
    var annotations = new HashSet<IAnnotation> { storageAnnotation };
    var typeResolver = Substitute.For<ITypeResolver>();
    var p = new NonTerminal("P");
    var a = new NonTerminal("A");
    var b = new AnnotatedNonTerminal("B", annotations);
    var abs = new Abstract(typeResolver);
    var productions = new Production<IAbstractSymbol>[]
    {
      new Production<IAbstractSymbol>(p, a),
      new Production<IAbstractSymbol>(p, b)
    };

    typeResolver.GetType("A").Returns(typeof(ASubType1));
    typeResolver.GetType("B").Returns(typeof(BType));
    typeResolver.GetType("P").Returns(typeof(AType));

    // act
    var result = abs.GetAbstract(productions);

    // assert
    result.Count.Should().Be(1);
    result.First().Should().Be(p);
  }


  public class AType { }
  public class ASubType1 : AType { }
  public class ASubType2 : AType { }
  public class BType { }
  public class ClassWithProperties
  {
    public bool BoolValue { set; get; }
    public int IntValue { set; get; }
    public IList<int> IntValues { set; get; }
  }
}
