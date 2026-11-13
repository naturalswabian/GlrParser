// Generalized LR Parser
// Copyright © 2020 Jan B. Freuer
// All rights reserved

using AwesomeAssertions;
using Glr.Grammar.Elements.Symbols;
using Glr.Meta;
using Glr.Meta.Attributes;
using Glr.Scanner;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GlrParser.Tests.Meta;

[TestClass]
public class ClientParserTests
{
  [TestMethod]
  public void TestBooleanExpression()
  {
    // arrange
    var expression = "a & a";
    var parser = CreateParser();
    var scannerSource = new ReaderScannerSource(expression, expression);

    // act
    var r = parser.Parse(scannerSource, null);

    // assert
    r.Model.Should().NotBeNull();
  }

  [TestMethod]
  public void ConstantTrue()
  {
    // arrange
    var expression = "true";
    var parser = CreateParser();
    var scannerSource = new ReaderScannerSource(expression, expression);

    // act
    var r = parser.Parse(scannerSource, null);

    // assert
    r.Model.Should().NotBeNull();
    var factor = r.Model.Operands[0].Operands[0].Operands[0].Factor;
    factor.Should().BeOfType<Constant>();
    var constant = (Constant)factor;
    constant.Value.Should().Be(BooleanConstant.True);
  }

  [TestMethod]
  public void ConstantFalse()
  {
    // arrange
    var expression = "false";
    var parser = CreateParser();
    var scannerSource = new ReaderScannerSource(expression, expression);

    // act
    var r = parser.Parse(scannerSource, null);

    // assert
    r.Model.Should().NotBeNull();
    var factor = r.Model.Operands[0].Operands[0].Operands[0].Factor;
    factor.Should().BeOfType<Constant>();
    var constant = (Constant)factor;
    constant.Value.Should().Be(BooleanConstant.False);
  }

  private static IParser<Expression> CreateParser()
  {
    var skipTerminals = new HashSet<Terminal>()
    {
      new Terminal("whitespace", "[ \t\r\n]+")
    };
    return ParserFactory.CreateParser<Expression>(false, skipTerminals);
  }

  #region grammar
  [Rule("Operands += Xor ('|' Operands += Xor)*")]
  class Expression : IFactor
  {
    public List<Xor> Operands = null;
  }

  [Rule("Operands += And ('^' Operands += And)*")]
  class Xor
  {
    public List<And> Operands = null;
  }

  [Rule("Operands += Term ('&' Operands += Term)*")]
  class And
  {
    public List<Term> Operands = null;
  }

  [Rule("IsNegated ?= '!'? ( Factor = Constant | Factor = Variable | ( '(' Factor = Expression ')' ))")]
  class Term
  {
    public bool IsNegated = false;
    public IFactor Factor = null;
  }

  interface IFactor
  {
  }

  [Rule("Name = <Identifier>")]
  class Variable : IFactor
  {
    public string Name = null;
  }

  [Pattern(@"[_a-zA-Z][_a-zA-Z0-9]*", -1)]
  class Identifier
  {
  }

  [Rule("Value = BooleanConstant")]
  class Constant : IFactor
  {
    public BooleanConstant Value = BooleanConstant.True;
  }

  [EnumRule]
  enum BooleanConstant
  {
    [Name("true")]
    True,

    [Name("false")]
    False
  }

  #endregion
}
