// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

namespace Glr.Meta;

/// <summary>
/// The intermediate object that is created for an enumeration based grammar rule.
/// </summary>
public class EnumNode
{
  /// <summary>
  /// The lexeme of this enumeration node.
  /// </summary>
  public string Lexeme { set; get; }
}
