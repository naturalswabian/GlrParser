// Generalized LR Parser
// Copyright © 2020 Jan B. Freuer
// All rights reserved

using Glr.Tree;

namespace GlrParser.Tests.Parser;

public static class DefaultTreeNodeExtensions
{
  public static DefaultTreeNodeAssertions Should(this DefaultTreeNode node)
  {
    return new DefaultTreeNodeAssertions(node);
  }
}
