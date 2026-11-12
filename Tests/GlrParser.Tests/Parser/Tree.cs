// Generalized LR Parser
// Copyright © 2020 Jan B. Freuer
// All rights reserved

using Glr.Grammar;
using Glr.Grammar.Elements;
using Glr.Scanner;
using Glr.Tree;
using System.Collections.Generic;

namespace GlrParser.Tests.Parser;

public class TestTree
{
  public TestTree(DefaultGrammar defaultGrammar, params Node[] children)
  {
    var root = new Node(defaultGrammar.StartSymbol.Name, children);
    _defaultGrammar = defaultGrammar;
    _children = new TestTree[] { root };
    root.Parent = this;
  }

  protected TestTree(string terminal)
  {
    _symbolName = terminal;
  }

  protected TestTree(string nonTerminal, params TestTree[] children)
  {
    _symbolName = nonTerminal;
    _children = children;
    foreach (var child in children)
      child.Parent = this;
  }

  public DefaultTreeNode AsTree()
  {
    if (_symbolName == null)
      return _children[0].AsTree();

    if (_children == null)
    {
      var terminal = Grammar.GetTerminal(_symbolName);
      var context = new SourceContext(0, "test", 0, 0);
      var token = new Token(terminal, null, context, null);
      return new DefaultTreeNode(token);
    }
    else
    {
      var nonTerminal = Grammar.GetNonTerminal(_symbolName);
      var production = new Production<IElement>(nonTerminal);
      var treeChildren = new List<DefaultTreeNode>();
      foreach (var child in _children)
        treeChildren.Add(child.AsTree());
      return new DefaultTreeNode(production, treeChildren);
    }
  }
  public class Node : TestTree
  {
    public Node(string terminalName) : base(terminalName) { }
    public Node(string terminalName, params Node[] children) : base(terminalName, children) { }
  }

  private DefaultGrammar Grammar
  {
    get
    {
      var current = this;
      while (current.Parent != null)
        current = current.Parent;
      return current._defaultGrammar;
    }
  }

  public TestTree Parent { set; get; }
  private readonly string _symbolName;
  private readonly DefaultGrammar _defaultGrammar;
  private readonly TestTree[] _children;
}
