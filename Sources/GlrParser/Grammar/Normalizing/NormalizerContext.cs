// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements.Symbols;
using System.Collections.Generic;

namespace Glr.Grammar.Normalizing;

internal class NormalizerContext
{
  public NormalizerContext(string baseName)
  {
    _baseName = baseName;
  }

  public string NewDerivedNonTerminalName()
  {
    _count++;
    return $"{_baseName}_N{_count}";
  }

  public List<Production<IAbstractSymbol>> Productions { get; } = new List<Production<IAbstractSymbol>>();

  public IChangeableSymbolMap Symbols { get; } = new SymbolMap();

  private readonly string _baseName;
  private int _count;
}
