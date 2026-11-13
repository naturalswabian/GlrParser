// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements.Symbols;
using Glr.Meta;
using Glr.Tree;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Glr.Util;

/// <summary>
/// Helper for writing a textual representation of <see cref="DefaultTreeNode"/>
/// hierarchies to a <see cref="TextWriter"/>.
/// </summary>
public class DefaultTreeNodeWriter
{
  /// <summary>
  /// Creates a new writer that emits a textual representation to the given writer.
  /// </summary>
  /// <param name="writer">The writer to which the syntax tree should be dumped.</param>
  public DefaultTreeNodeWriter(TextWriter writer)
  {
    _textWriter = writer;
  }

  /// <summary>
  /// Writes the abstract syntax tree which is represented by this node to debug console.
  /// </summary>
  public static void Write(DefaultTreeNode node, IDictionary<NonTerminal, INonTerminalHandler> nonTerminalHandlers = null)
  {
    var writer = new StringWriter();
    var dump = new DefaultTreeNodeWriter(writer) { Recursive = true, WriteRightHandSideAnnotation = true };
    dump.Write(node, nonTerminalHandlers);
    System.Diagnostics.Debug.WriteLine(writer);
  }

  /// <summary>
  /// Writes the abstract syntax tree which is represented by this node to the given text writer.
  /// </summary>
  public void Write(DefaultTreeNode node, int indent = 0)
  {
    Write(node, null, indent);
  }

  /// <summary>
  /// Writes the abstract syntax tree which is represented by this node to the given text writer.
  /// </summary>
  public void Write(DefaultTreeNode node, IDictionary<NonTerminal, INonTerminalHandler> nonTerminalHandlers, int indent = 0)
  {
    // write indentation
    for (int i = 0; i < indent; i++)
      _textWriter.Write("  ");

    // write the current node
    INonTerminalHandler handler = null;
    if (nonTerminalHandlers != null && node.Symbol is NonTerminal nonTerminal)
      nonTerminalHandlers.TryGetValue(nonTerminal, out handler);
    _textWriter.WriteLine(ToString(node, handler));

    // write child nodes
    if (node.Children == null || !Recursive) return;
    foreach (var childNode in node.Children)
      if (childNode != null) Write(childNode, nonTerminalHandlers, indent + 1);
  }

  private string ToString(DefaultTreeNode node, INonTerminalHandler handler)
  {
    if (node.IsTerminalNode) return node.Token.ToString();

    var result = new StringBuilder();
    var leftHandSideSymbol = node.Production.LeftSideSymbol;
    if (WriteLeftHandSideAnnotation && leftHandSideSymbol is AnnotatedNonTerminal annotated)
    {
      WriteAnnotations(result, annotated);
      result.Append(' ');
    }
    result.Append(node.Production.LeftSideSymbol.Name);
    result.Append(" ->");
    foreach (var rightHandSideSymbol in node.Production.RightHandSide)
    {
      result.Append(' ');
      if (rightHandSideSymbol is Terminal) result.Append(rightHandSideSymbol);
      else
      {
        if (WriteRightHandSideAnnotation && rightHandSideSymbol is AnnotatedNonTerminal annotatedNonTerminal)
        {
          WriteAnnotations(result, annotatedNonTerminal);
          result.Append(' ');
        }
        result.Append(rightHandSideSymbol.Name);
      }
    }

    if (handler != null) result.Append($" {{{handler}}}");

    return result.ToString();
  }

  private static void WriteAnnotations(StringBuilder result, AnnotatedNonTerminal annotatedNonTerminal)
  {
    var isFirst = true;
    foreach (var annotation in annotatedNonTerminal.Annotations)
    {
      if (isFirst) isFirst = false;
      else result.Append(' ');
      result.Append(annotation);
    }
  }

  /// <summary>
  /// Writes annotations that are attached to an <see cref="AnnotatedNonTerminal"/> element
  /// as well on the left hand side of a production rule.
  /// </summary>
  public bool WriteLeftHandSideAnnotation { set; get; }

  /// <summary>
  /// Writes annotations that are attached to <see cref="AnnotatedNonTerminal"/> elements 
  /// as well on the right hand side of a production rule.
  /// </summary>
  public bool WriteRightHandSideAnnotation { set; get; }

  /// <summary>
  /// When <see langword="true"/>, trees are dumped recursively.
  /// Otherwise only one single node is written.
  /// </summary>
  public bool Recursive { set; get; }

  private readonly TextWriter _textWriter;
}
