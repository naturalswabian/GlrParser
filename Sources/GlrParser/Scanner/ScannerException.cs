// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using System;

namespace Glr.Scanner;

/// <summary>
/// An exception that is thrown by a <see cref="IScanner"/> 
/// while scanning a <see cref="IScannerSource"/>.
/// </summary>
public sealed class ScannerException : Exception
{
  /// <summary>
  /// Creates a new scanner exception that is associated to a source location.
  /// </summary>
  /// <param name="message">The message that describes the error.</param>
  /// <param name="context">The source location that is associated to this error.</param>
  public ScannerException(string message, SourceContext context) : base(message)
  {
    SourceContext = context;
  }

  /// <summary>
  /// Creates a new scanner exception that is not associated to a source location.
  /// </summary>
  /// <param name="message">The message that describes the error.</param>
  public ScannerException(string message) : this(message, null) { }

  /// <summary>
  /// The source context (file and location) of the error. Can be <see langword="null"/>.
  /// </summary>
  public SourceContext SourceContext { get; }

  /// <inheritdoc/>
  public override string ToString()
  {
    string detail = "";
    if (SourceContext != null)
      detail = " (in: \"" + SourceContext.Name + "\" line " + SourceContext.LineNumber + " column " + SourceContext.ColumnNumber + ")";
    return Message + detail;
  }
}
