// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Scanner;
using System;

namespace Glr.Parser;

/// <summary>
/// An exception that is created by a parser.
/// </summary>
public class ParserException : Exception
{
  /// <summary>
  /// Creates a new parser exception.
  /// </summary>
  /// <param name="message">The description of the error.</param>
  public ParserException(string message) : this(message, null) { }

  /// <summary>
  /// Creates a new parser exception.
  /// </summary>
  /// <param name="sourceContext">The source location that caused this error.</param>
  public ParserException(SourceContext sourceContext) : this(null, sourceContext) { }

  /// <summary>
  /// Creates a new parser exception.
  /// </summary>
  /// <param name="message">The description of the error.</param>
  /// <param name="sourceContext">The source location that caused this error.</param>
  public ParserException(string message, SourceContext sourceContext)
    : base(message)
  {
    SourceContext = sourceContext;
  }

  /// <inheritdoc/>
  public override string ToString()
  {
    string message;
    if (SourceContext != null) message = GetType().Name + ": " + Message + " (in " + SourceContext.Name + " at line " + SourceContext.LineNumber + " column " + SourceContext.ColumnNumber + ")";
    else message = GetType().Name + ": " + Message;
    return message;
  }

  /// <summary>
  /// The location and the resource which caused this error.
  /// </summary>
  public SourceContext SourceContext { get; }
}
