// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar;
using Glr.Grammar.Elements;
using Glr.Scanner;
using System.Collections.Generic;

namespace Glr.Tree;

/// <summary>
/// A <see cref="ITreeNodeFactory{T}"/> that creates <see cref="DefaultTreeNode"/>s.
/// </summary>
public class DefaultTreeNodeFactory : ITreeNodeFactory<DefaultTreeNode>
{
  /// <inheritdoc/>
  public DefaultTreeNode CreateNonTerminalNode(IProduction<IElement> production, IList<DefaultTreeNode> children)
  {
    return new DefaultTreeNode(production, children);
  }

  /// <inheritdoc/>
  public DefaultTreeNode CreateTerminalNode(Token token)
  {
    return new DefaultTreeNode(token);
  }
}
