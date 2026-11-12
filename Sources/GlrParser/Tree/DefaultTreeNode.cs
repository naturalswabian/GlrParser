// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar;
using Glr.Grammar.Elements;
using Glr.Grammar.Elements.Symbols;
using Glr.Scanner;
using Glr.Util;
using System.Collections.Generic;
using System.IO;

namespace Glr.Tree;

/// <summary>
/// The default implementation of a <see cref="ITreeNode{T}"/>.
/// </summary>
public class DefaultTreeNode : ITreeNode<DefaultTreeNode>
{
  /// <summary>
  /// Creates a new non-terminal syntax tree node.
  /// </summary>
  /// <param name="production">The production rule that lead to the new node.</param>
  /// <param name="children">The children of the new node.</param>
  public DefaultTreeNode(IProduction<IElement> production, IList<DefaultTreeNode> children)
  {
    Children = children;
    Production = production;
    foreach (DefaultTreeNode child in children)
      TerminalLength += child.TerminalLength;
  }

  /// <summary>
  /// Creates a new terminal syntax tree node.
  /// </summary>
  /// <param name="token">The (terminal) token that lead to the new node.</param>
  public DefaultTreeNode(Token token)
  {
    Token = token;
    TerminalLength = 1;
  }

  /// <inheritdoc/>
  public IList<DefaultTreeNode> Children { get; }

  /// <inheritdoc/>
  public IAbstractSymbol Symbol
  {
    get
    {
      if (IsTerminalNode) return Token?.Terminal;
      else return Production?.LeftSideSymbol;
    }
  }

  /// <inheritdoc/>
  public Token Token { get; }

  /// <inheritdoc/>
  public IProduction<IElement> Production { get; }

  /// <inheritdoc/>
  public string Lexeme => Token?.Lexeme;

  /// <inheritdoc/>
  public int TerminalLength { get; }

  /// <summary>
  /// Is <see langword="true"/> when this node is a terminal node.
  /// </summary>
  public bool IsTerminalNode => Token != null;

  /// <summary>
  /// The location within the original source.
  /// </summary>
  public SourceContext SourceContext => Token?.SourceContext;

  /// <summary>
  /// Gets the string representation of this node.
  /// </summary>
  public override string ToString()
  {
    var stringWriter = new StringWriter();
    var treeDump = new DefaultTreeNodeWriter(stringWriter) { Recursive = false, WriteRightHandSideAnnotation = true };
    treeDump.Write(this);
    return stringWriter.ToString();
  }
}
