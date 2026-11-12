// Generalized LR Parser
// Copyright © 2020 Jan B. Freuer
// All rights reserved

using Glr.Grammar;
using Glr.Grammar.Elements;
using Glr.Grammar.Elements.Sequence;
using Glr.Grammar.Elements.Symbols;
using System.Collections.Generic;

namespace GlrParser.Tests.Parser;

public static class AbcGrammar
{
  /// <summary>
  /// Creates the test grammar:
  /// <code>
  /// start -&gt; content;<br/>
  /// content -&gt; a+ b+ c+;<br/>
  /// </code>
  /// </summary>
  public static DefaultGrammar CreateGrammar()
  {
    var startSymbol = new NonTerminal("start");
    var content = new NonTerminal("content");
    var eofSymbol = new Terminal("eof");
    var aTerminal = new Terminal("a");
    var bTerminal = new Terminal("b");
    var cTerminal = new Terminal("c");
    var rules = new List<Production<IElement>>
    {
      new Production<IElement>(startSymbol, content),
      new Production<IElement>(content,
        new OnceOrMoreElement(aTerminal),
        new OnceOrMoreElement(bTerminal),
        new OnceOrMoreElement(cTerminal)),
    };
    var grammar = new DefaultGrammar(rules, startSymbol, eofSymbol, null, null);
    return grammar;
  }
}
