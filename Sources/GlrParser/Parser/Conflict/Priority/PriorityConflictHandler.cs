// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Annotations;
using Glr.Grammar.Elements;
using Glr.Grammar.Elements.Symbols;
using Glr.Parser.Table;
using Glr.Scanner;
using Glr.Tree;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Glr.Parser.Conflict.Priority;

/// <summary>
/// A conflict handler that resolves by using priority annotation that are part of the grammar.
/// </summary>
/// <typeparam name="S">The type of the state machine table.</typeparam>
/// <typeparam name="T">The type of the syntax tree nodes.</typeparam>
public class PriorityConflictHandler<S, T> : IConflictHandler<S, T>
    where S : class, ITableState
    where T : class, ITreeNode<T>
{
  /// <summary>
  /// Creates a new priority based conflict handler.
  /// </summary>
  /// <param name="allowMultipleTrees">When <see langword="true"/>, multiple derivation trees are allowed.</param>
  public PriorityConflictHandler(bool allowMultipleTrees)
  {
    _allowMultipleTrees = allowMultipleTrees;
    _unknownCause = new Cause<T>(ConflictType.Unknown);
  }

  /// <inheritdoc/>
  public void ResolveConflict(IList<ParserNodeState<S, T>> entries, GlrParser<S, T> parser)
  {
    while (entries.Count > 1)
    {
      T n1 = entries[0].Node;
      T n2 = entries[1].Node;
      Cause<T> cause = DetermineCause(n1, n2);
      switch (cause.Type)
      {
        case ConflictType.Unknown:
          return;
        case ConflictType.Single:
          ResolveSingle(entries, parser, cause);
          break;
        case ConflictType.Multiple:
          ResolveMultiple(entries, parser, cause);
          break;
      }
    }
  }

  private void ResolveMultiple(IList<ParserNodeState<S, T>> entries, GlrParser<S, T> parser, Cause<T> cause)
  {
    IElementList<IElement> rhs1 = cause.SourceNode.Production.RightHandSide;
    IElementList<IElement> rhs2 = cause.SecondaryNode.Production.RightHandSide;
    int prio1 = PriorityAnnotation.DefaultPriority;
    int prio2 = PriorityAnnotation.DefaultPriority;
    if (rhs1.Count == 1 && rhs1[0] is AnnotatedNonTerminal annotatedNonTerminal1)
    {
      var annotation = annotatedNonTerminal1.Annotations.OfType<PriorityAnnotation>().FirstOrDefault();
      if (annotation != null)
        prio1 = annotation.Priority;
    }
    if (rhs2.Count == 1 && rhs2[0] is AnnotatedNonTerminal annotatedNonTerminal2)
    {
      var annotation = annotatedNonTerminal2.Annotations.OfType<PriorityAnnotation>().FirstOrDefault();
      if (annotation != null)
        prio2 = annotation.Priority;
    }
    if (prio1 == prio2)
    {
      if (!_allowMultipleTrees)
      {
        SourceContext context = GetLastToken(cause.SourceNode).SourceContext;
        throw new ConflictException<T>(GetErrorMessage(cause, parser), null, context);
      }
      else return;
    }
    if (prio1 > prio2) entries.RemoveAt(1);
    else entries.RemoveAt(0);
  }

  private void ResolveSingle(IList<ParserNodeState<S, T>> entries, GlrParser<S, T> parser, Cause<T> cause)
  {
    var removed = false;
    var children1 = cause.SourceNode.Children;
    var children2 = cause.SecondaryNode.Children;

    for (int i = 0; i < children1.Count; i++)
    {
      T child1 = children1[i];
      T child2 = children2[i];
      IAbstractSymbol childSymbol = child1.Symbol;
      if (!(childSymbol is AnnotatedNonTerminal annotatedNonTerminal))
        continue;

      foreach (var annotation in annotatedNonTerminal.Annotations.OfType<PriorityAnnotation>())
      {
        int node1Length = child1.TerminalLength;
        int node2Length = child2.TerminalLength;

        switch (annotation.Type)
        {
          case PriorityAnnotationType.Short:
            removed = TryResolveShort(entries, node1Length, node2Length);
            break;
          case PriorityAnnotationType.Long:
            removed = TryResolveLong(entries, node1Length, node2Length);
            break;
        }
        if (removed) break;
      }
      if (removed) break;
    }
    if (!removed && !_allowMultipleTrees)
    {
      SourceContext context = GetLastToken(cause.SourceNode).SourceContext;
      throw new ConflictException<T>(GetErrorMessage(cause, parser), null, context);
    }
  }

  private static bool TryResolveShort(IList<ParserNodeState<S, T>> entries, int node1Length, int node2Length)
  {
    var removed = false;
    if (node1Length < node2Length)
    {
      entries.RemoveAt(1);
      removed = true;
    }
    if (node1Length > node2Length)
    {
      entries.RemoveAt(0);
      removed = true;
    }
    return removed;
  }

  private static bool TryResolveLong(IList<ParserNodeState<S, T>> entries, int node1Length, int node2Length)
  {
    var removed = false;
    if (node1Length > node2Length)
    {
      entries.RemoveAt(1);
      removed = true;
    }
    if (node1Length < node2Length)
    {
      entries.RemoveAt(0);
      removed = true;
    }
    return removed;
  }

  private Cause<T> DetermineCause(T primary, T secondary)
  {
    if (primary.Production == secondary.Production)
      return DetermineCauseSameProduction(primary, secondary);
    else
      return new Cause<T>(primary, secondary, ConflictType.Multiple);
  }

  private Cause<T> DetermineCauseSameProduction(T primary, T secondary)
  {
    if (primary.TerminalLength != secondary.TerminalLength) return _unknownCause;
    var primaryChildren = primary.Children;
    var secondaryChildren = secondary.Children;
    for (int i = 0; i < primaryChildren.Count; i++)
    {
      T c1 = primaryChildren[i];
      T c2 = secondaryChildren[i];
      if (c1.TerminalLength != c2.TerminalLength)
      {
        return new Cause<T>(primary, secondary, ConflictType.Single);
      }
      else if (c1.Symbol is NonTerminal)
      {
        Cause<T> cause = DetermineCause(c1, c2);
        if (cause.Type != ConflictType.Unknown) return cause;
      }
    }
    return _unknownCause;
  }

  private static string GetErrorMessage(Cause<T> cause, GlrParser<S, T> parser)
  {
    string message = "unable to determine cause";
    T n1 = parser.NormalizeAnnotatedElements(cause.SourceNode);
    T n2 = parser.NormalizeAnnotatedElements(cause.SecondaryNode);
    switch (cause.Type)
    {
      case ConflictType.Single:
        message = "multiple trees in single rule:\n";
        message += NodeToString(n1, 4);
        message += "---------------\n";
        message += NodeToString(n2, 4);
        break;
      case ConflictType.Multiple:
        message = "multiple trees in different alternatives:\n";
        message += NodeToString(n1, 4);
        message += "---------------\n";
        message += NodeToString(n2, 4);
        break;
    }
    return message;
  }

  private static string NodeToString(T node, int indent)
  {
    var result = new StringBuilder();
    NodeToString(node, indent, result);
    return result.ToString();
  }

  private static void NodeToString(T node, int indent, StringBuilder result)
  {
    for (int i = 0; i < indent; i++) result.Append("  ");
    result.AppendLine(node.ToString());
    if (node.Symbol is NonTerminal)
      foreach (T child in node.Children)
        NodeToString(child, indent + 1, result);
  }

  private static Token GetLastToken(T node)
  {
    if (node.Symbol is NonTerminal)
    {
      var children = node.Children;
      return GetLastToken(children[children.Count - 1]);
    }
    else
      return node.Token;
  }

  private readonly Cause<T> _unknownCause;
  private readonly bool _allowMultipleTrees;
}
