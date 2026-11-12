// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements.Symbols;
using System.Collections.Generic;

namespace Glr.Scanner;

internal class KeywordContext
{
  public KeywordContext(Terminal eof)
  {
    EofTerminal = eof;
  }

  public List<Terminal> Keywords { get; } = new List<Terminal>();
  public List<Matcher> Flex { get; } = new List<Matcher>();
  public ISet<Terminal> SkipTerminals { get; } = new HashSet<Terminal>();
  public ISet<Terminal> SpecialTerminals { get; } = new HashSet<Terminal>();
  public Terminal EofTerminal { get; }
}
