// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Scanner;
using Glr.Tree;
using Glr.Util;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Glr.Parser.Conflict;

/// <summary>
/// An exception that is thrown when a conflict occurred while parsing that could not be resolved.
/// </summary>
/// <typeparam name="T">The type of the syntax tree nodes.</typeparam>
public sealed class ConflictException<T> : ParserException
  where T : ITreeNode<T>
{
  /// <summary>
  /// Creates a new exception that is reported when a conflict occurred while parsing.
  /// </summary>
  /// <param name="message">The description of the conflict.</param>
  /// <param name="nodes">The nodes that lead to the conflict.</param>
  /// <param name="context">The source location where this conflict occurred.</param>
  public ConflictException(string message, IEnumerable<T> nodes, SourceContext context)
    : base(message, context)
  {
    Nodes = nodes;
  }

  /// <summary>
  /// The nodes that lead to the conflict.
  /// </summary>
  public IEnumerable<T> Nodes { get; }

  /// <inheritdoc/>
  public override string ToString()
  {
    if (Nodes == null) return base.ToString();
    var writer = new StringWriter();
    writer.WriteLine(base.ToString());
    foreach (var node in Nodes.OfType<DefaultTreeNode>())
    {
      var dump = new DefaultTreeNodeWriter(writer);
      dump.Write(node);
      writer.WriteLine("------------------");
    }

    return writer.ToString();
  }
}
