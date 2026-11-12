// Generalized LR Parser
// Copyright © 2020 Jan B. Freuer
// All rights reserved

using AwesomeAssertions;
using Glr.Grammar;
using Glr.Grammar.Elements;
using Glr.Grammar.Elements.Sequence;
using Glr.Grammar.Elements.Symbols;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Glr.Tests.Grammar;

[TestClass]
public class EmptyInputGrammarTest
{
  /// <summary>
  /// Tests if a grammar is created that supports empty inputs.
  /// <code>
  /// start -&gt; a*
  /// </code>
  /// </summary>
  [TestMethod]
  public void HasEmptyInput()
  {
    // arrange
    var startSymbol = new NonTerminal("start");
    var eofSymbol = new Terminal("eof");
    var aTerminal = new Terminal("a");
    var rules = new List<Production<IElement>>
    {
      new Production<IElement>(startSymbol, new ZeroOrMoreElement(aTerminal))
    };

    // act
    var grammar = new DefaultGrammar(rules, startSymbol, eofSymbol, null, null);

    // assert
    grammar.AllowsEmptyInput.Should().Be(true);
  }

  /// <summary>
  /// Tests if a grammar is created that does not supports empty inputs.
  /// <code>
  /// start -&gt; a*
  /// </code>
  /// </summary>
  [TestMethod]
  public void HasNonEmptyInput()
  {
    // arrange
    var startSymbol = new NonTerminal("start");
    var eofSymbol = new Terminal("eof");
    var aTerminal = new Terminal("a");
    var rules = new List<Production<IElement>>
    {
      new Production<IElement>(startSymbol, aTerminal)
    };

    // act
    var grammar = new DefaultGrammar(rules, startSymbol, eofSymbol, null, null);

    // assert
    grammar.AllowsEmptyInput.Should().Be(false);
  }
}
