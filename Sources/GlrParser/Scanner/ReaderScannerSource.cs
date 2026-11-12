// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Glr.Scanner;

/// <summary>
/// A source of a scanner that is fetches is content from a <see cref="TextReader"/> or <see cref="Stream"/>.
/// </summary>
public sealed partial class ReaderScannerSource : IScannerSource
{
  /// <summary>
  /// Creates a new scanner source that uses the content of the given reader.
  /// </summary>
  /// <param name="name">The resource name of this source.</param>
  /// <param name="reader">The reader that provides the content of this source.</param>
  public ReaderScannerSource(string name, TextReader reader) : this(name, GetContent(reader)) { }

  /// <summary>
  /// Creates a new scanner source that uses the content of the given stream.
  /// </summary>
  /// <param name="name">The resource name of this source.</param>
  /// <param name="stream">The stream that provides the content of this source.</param>
  public ReaderScannerSource(string name, Stream stream) : this(name, GetContent(stream)) { }

  /// <summary>
  /// Creates a new scanner source based on an explicitly given string as content.
  /// </summary>
  /// <param name="name">The resource name of this source.</param>
  /// <param name="content">The explicit content of this source.</param>
  public ReaderScannerSource(string name, string content)
  {
    Name = name;
    Content = content;

    _endOfLines = new List<int>();
    _endOfLines.Add(0);

    Match matcher = EndOfLinePattern().Match(Content);

    while (matcher.Success)
    {
      Group group = matcher.Groups[0];
      int end = group.Index + group.Length;
      _endOfLines.Add(end);
      matcher = matcher.NextMatch();
    }
  }

  /// <inheritdoc/>
  public char GetCharAt(int index)
  {
    return Content[index];
  }

  /// <inheritdoc/>
  public string Name { get; }

  /// <inheritdoc/>
  public int Length => Content.Length;

  /// <inheritdoc/>
  public override string ToString()
  {
    return Content;
  }

  /// <inheritdoc/>
  public int GetColumn(int index)
  {
    int lineIndex = GetLine(index) - 1;
    return index - _endOfLines[lineIndex] + 1;
  }

  /// <inheritdoc/>
  public int GetLine(int index)
  {
    int first = 0;
    int last = _endOfLines.Count - 1;
    while (first < last - 1)
    {
      int line = (last - first) / 2 + first;
      int lineIndex = _endOfLines[line];
      if (index <= lineIndex) last = line;
      if (index >= lineIndex) first = line;
    }
    int result = _endOfLines[last] == index ? last : first;
    return result + 1;
  }

  /// <inheritdoc/>
  public string Content { get; }

  private static string GetContent(TextReader reader)
  {
    string buffer = reader.ReadToEnd();
    reader.Close();
    return buffer;
  }

  private static string GetContent(Stream stream)
  {
    using (var reader = new StreamReader(stream))
      return GetContent(reader);
  }

  [GeneratedRegex("(\\r\\n)|\\r|\\n")]
  private static partial Regex EndOfLinePattern();

  private readonly List<int> _endOfLines;
}
