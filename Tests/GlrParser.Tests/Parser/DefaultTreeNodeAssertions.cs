// Generalized LR Parser
// Copyright © 2020 Jan B. Freuer
// All rights reserved

using AwesomeAssertions;
using Glr.Tree;

namespace GlrParser.Tests.Parser;

public class DefaultTreeNodeAssertions
{
  public DefaultTreeNodeAssertions(DefaultTreeNode defaultTreeNode)
  {
    _defaultTreeNode = defaultTreeNode;
  }

  public void Be(DefaultTreeNode node)
  {
    Match(_defaultTreeNode, node).Should().Be(true);
  }

  private static bool Match(DefaultTreeNode parsedNode, DefaultTreeNode testNode)
  {
    if (parsedNode.Symbol != testNode.Symbol) return false;
    if (!parsedNode.IsTerminalNode)
    {
      if (parsedNode.Children.Count != testNode.Children.Count) return false;
      for (int i = 0; i < parsedNode.Children.Count; i++)
        if (!Match(parsedNode.Children[i], testNode.Children[i]))
          return false;
    }
    return true;
  }

  private readonly DefaultTreeNode _defaultTreeNode;
}
