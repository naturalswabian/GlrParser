// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements.Symbols;
using System.Text.RegularExpressions;

namespace Glr.Scanner;

internal class Matcher
{
  public Matcher(Terminal terminal, Regex regex)
  {
    Terminal = terminal;
    Regex = regex;
  }

  public Terminal Terminal { get; }
  public Regex Regex { get; }

  public override string ToString()
  {
    return Terminal.Name;
  }
}
