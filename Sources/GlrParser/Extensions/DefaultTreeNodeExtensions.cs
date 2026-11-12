// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements.Symbols;
using Glr.Tree;
using System;

namespace Glr.Extensions;

/// <summary>
/// Extension methods for the usage of <see cref="DefaultTreeNode"/>.
/// </summary>
public static class DefaultTreeNodeExtensions
{
  /// <summary>
  /// Gets the first terminal node in the given tree node or one of its children.
  /// </summary>
  public static DefaultTreeNode GetFirstTerminalNode(this DefaultTreeNode treeNode)
  {
    ArgumentNullException.ThrowIfNull(treeNode);

    DefaultTreeNode terminalNode = treeNode;
    while (terminalNode.Symbol is NonTerminal)
      terminalNode = terminalNode.Children[0];

    return terminalNode;
  }

  /// <summary>
  /// Gets the last terminal node in the given tree node or one of its children.
  /// When applied to the root node, the returned node is the node before the
  /// end-of-file terminal.
  /// </summary>
  public static DefaultTreeNode GetLastTerminalNode(this DefaultTreeNode treeNode)
  {
    ArgumentNullException.ThrowIfNull(treeNode);

    DefaultTreeNode terminalNode = treeNode;
    while (terminalNode.Symbol is NonTerminal)
      terminalNode = terminalNode.Children[terminalNode.Children.Count - 1];

    return terminalNode;
  }
}
