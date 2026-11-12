// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using System.Text;
using System.Text.RegularExpressions;

namespace Glr.Grammar.Elements.Symbols;

/// <summary>
/// A terminal is symbol that cannot be further refined. It acts an end point when parsing.
/// </summary>
public class Terminal : IAbstractSymbol
{
  /// <summary>
  /// Creates a new terminal symbol.
  /// </summary>
  /// <param name="name">The name of the new terminal.</param>
  /// <param name="regexp">The regular expression used when parsing.</param>
  /// <param name="isKeyword">When <see langword="true"/>, this terminal is a keyword and has implicitly a higher priority.</param>
  /// <param name="priority">The priority of regular expression based terminals.</param>
  public Terminal(string name, string regexp, bool isKeyword, int priority)
  {
    if (regexp == null)
    {
      if (!isKeyword) regexp = Regex.Escape(name);
      else regexp = name;
    }
    Name = name;
    Priority = priority;
    Pattern = regexp;
    IsKeyword = isKeyword;
  }

  /// <summary>
  /// Creates a new terminal symbol.
  /// </summary>
  /// <param name="name">The name of the new terminal.</param>
  /// <param name="regexp">The regular expression used when parsing.</param>
  /// <param name="priority">The priority of regular expression based terminals.</param>
  public Terminal(string name, string regexp, int priority) : this(name, regexp, false, priority) { }

  /// <summary>
  /// Creates a new terminal symbol.
  /// </summary>
  /// <param name="name">The name of the new terminal.</param>
  /// <param name="regexp">The regular expression used when parsing.</param>
  public Terminal(string name, string regexp) : this(name, regexp, false, 0) { }

  /// <summary>
  /// Creates a new terminal symbol.
  /// </summary>
  /// <param name="name">The name of the new terminal.</param>
  /// <param name="isKeyword">When <see langword="true"/>, this terminal is a keyword and has implicitly a higher priority.</param>
  public Terminal(string name, bool isKeyword) : this(name, null, isKeyword, 0) { }


  /// <summary>
  /// Creates a new terminal symbol.
  /// </summary>
  /// <param name="name">The name of the new terminal.</param>
  public Terminal(string name) : this(name, null, false, 0) { }

  /// <summary>
  /// The priority of this terminal.
  /// </summary>
  public int Priority { get; }

  /// <inheritdoc/>
  public string Name { get; }

  /// <summary>
  /// The regular expression pattern used for identifying this terminal.
  /// </summary>
  public string Pattern { get; }

  /// <summary>
  /// When <see langword="true"/>, this terminal is keyword.
  /// </summary>
  public bool IsKeyword { get; }

  /// <inheritdoc/>
  public override string ToString()
  {
    var result = new StringBuilder();
    result.Append('<');
    result.Append(Name);
    result.Append('>');
    return result.ToString();
  }
}
