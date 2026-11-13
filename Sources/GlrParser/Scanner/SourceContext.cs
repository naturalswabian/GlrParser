// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

namespace Glr.Scanner;

/// <summary>
/// The description of a location within a defined resource.
/// </summary>
public class SourceContext
{
  /// <summary>
  /// Creates a new source context description.
  /// </summary>
  /// <param name="sourceStartIndex">The absolute index within the resource.</param>
  /// <param name="sourceName">The name of the resource.</param>
  /// <param name="sourceLineNumber">The line number within the resource.</param>
  /// <param name="sourceColumnNumber">THe column number within the resource.</param>
  public SourceContext(int sourceStartIndex, string sourceName, int sourceLineNumber, int sourceColumnNumber)
  {
    StartIndex = sourceStartIndex;
    Name = sourceName;
    LineNumber = sourceLineNumber;
    ColumnNumber = sourceColumnNumber;
  }

  /// <summary>
  /// The column number within the resource.
  /// </summary>
  public int ColumnNumber { get; }

  /// <summary>
  /// The line number within the resource.
  /// </summary>
  public int LineNumber { get; }

  /// <summary>
  /// The name of the resource.
  /// </summary>
  public string Name { get; }

  /// <summary>
  /// The absolute index within the resource.
  /// </summary>
  public int StartIndex { get; }

  /// <inheritdoc/>
  public override string ToString()
  {
    return $"{Name}:{LineNumber},{ColumnNumber}";
  }

  /// <summary>
  /// The context that is used when content is artificially injected.
  /// </summary>
  public static SourceContext InjectedSourceContext { get; } = new SourceContext(-1, null, 0, 0);
}
