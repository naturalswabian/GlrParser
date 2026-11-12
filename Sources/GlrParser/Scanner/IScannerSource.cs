// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

namespace Glr.Scanner;

/// <summary>
/// The source of a <see cref="IScanner"/>. It mainly defines textual content.
/// </summary>
public interface IScannerSource
{
  /// <summary>
  /// The name of the resource such as a file name of this source.
  /// </summary>
  string Name { get; }

  /// <summary>
  /// The content of this source.
  /// </summary>
  string Content { get; }

  /// <summary>
  /// The number of characters within this source.
  /// </summary>
  int Length { get; }

  /// <summary>
  /// Gets the column number of the character with the given index.
  /// </summary>
  /// <param name="index">The index of the character.</param>
  /// <returns>-1, if the index is out of range.</returns>
  int GetColumn(int index);

  /// <summary>
  /// Gets the line number of the character with the given index.
  /// </summary>
  /// <param name="index">The index of the character.</param>
  /// <returns>-1, if the index is out of range.</returns>
  int GetLine(int index);

  /// <summary>
  /// Gets the single character at the given index.
  /// </summary>
  /// <param name="index">The index of the character.</param>
  /// <returns>The character at the given position.</returns>
  char GetCharAt(int index);
}
