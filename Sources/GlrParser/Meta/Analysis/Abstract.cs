// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using Glr.Grammar;
using Glr.Grammar.Elements.Symbols;
using Glr.Meta.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Glr.Meta.Analysis;

/// <summary>
/// Helper for detecting abstract rules.
/// </summary>
public sealed class Abstract
{
  /// <summary>
  /// Creates a new abstract analyzer.
  /// </summary>
  /// <param name="typeResolver">The type resolver used for analyzing.</param>
  public Abstract(ITypeResolver typeResolver)
  {
    _typeResolver = typeResolver;
  }

  /// <summary>
  /// Gets the set of non terminals that are used for production rules considered as abstract.
  /// An abstract production is able to simply forward the content to the outer context.
  /// </summary>
  public ISet<NonTerminal> GetAbstract(IEnumerable<Production<IAbstractSymbol>> productions)
  {
    // group productions by their left hand side
    var groupedProductions = GroupProductions(productions);

    // assume first that all non terminals are abstract
    var result = new HashSet<NonTerminal>(groupedProductions.Keys);

    foreach (var groupEntry in groupedProductions)
    {
      var nonTerminal = groupEntry.Key;
      var group = groupEntry.Value;
      var productionType = _typeResolver.GetType(nonTerminal.Name);
      if (productionType == null) continue;

      var isAbstract = IsAbstract(group, productionType);
      if (!isAbstract) result.Remove(nonTerminal);
    }
    return result;
  }

  private bool IsAbstract(ISet<Production<IAbstractSymbol>> group, Type productionType)
  {
    var isAbstract = true;
    foreach (var production in group)
    {
      // extract the non terminals from the production's right hand side
      var nonTerminals = production.RightHandSide.OfType<NonTerminal>().ToArray();

      // the production is not abstract if it does not have exactly one non terminal in its right hand side
      if (nonTerminals.Length != 1) { isAbstract = false; break; }
      var nonTerminal = nonTerminals[0];

      // check if the symbol is "consumed"
      if (nonTerminal is AnnotatedNonTerminal ant && ant.Annotations.OfType<StorageParserAction>().Any())
        continue;

      var symbolType = _typeResolver.GetType(nonTerminal.Name);
      if (symbolType == null || !productionType.IsAssignableFrom(symbolType))
      {
        isAbstract = false;
        break;
      }
    }

    return isAbstract;
  }

  /// <summary>
  /// Groups productions by their left hand side.
  /// </summary>
  private static Dictionary<NonTerminal, ISet<Production<IAbstractSymbol>>> GroupProductions(IEnumerable<Production<IAbstractSymbol>> productions)
  {
    Dictionary<NonTerminal, ISet<Production<IAbstractSymbol>>> groupedProductions = [];
    foreach (var production in productions)
    {
      if (!groupedProductions.TryGetValue(production.LeftSideSymbol, out ISet<Production<IAbstractSymbol>> group))
      {
        group = new HashSet<Production<IAbstractSymbol>>();
        groupedProductions[production.LeftSideSymbol] = group;
      }
      group.Add(production);
    }
    return groupedProductions;
  }

  private readonly ITypeResolver _typeResolver;
}
