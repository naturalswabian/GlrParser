// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Annotations;
using Glr.Grammar.Elements;
using Glr.Grammar.Elements.Sequence;
using Glr.Grammar.Elements.Symbols;
using System.Collections.Generic;
using System.Linq;

namespace Glr.Grammar.Normalizing;

/// <summary>
/// Provides normalizing of production rules such that they can be used by a parser.
/// </summary>
public class Normalizer
{
  /// <summary>
  /// Normalizes the given production by replacing the right hand side with simpler elements 
  /// that are suitable for the parser generator.
  /// </summary>
  public IList<Production<IAbstractSymbol>> Normalize(IProduction<IElement> production)
  {
    var context = new NormalizerContext(production.LeftSideSymbol.Name);
    var normalizedRightHandSides = NormalizeElement(production.RightHandSide, context);
    foreach (var rightHandSide in normalizedRightHandSides)
    {
      var normalizedProduction = new Production<IAbstractSymbol>(production, production.LeftSideSymbol, new ElementList<IAbstractSymbol>(rightHandSide));
      context.Productions.Add(normalizedProduction);
    }
    return context.Productions;
  }

  private void Normalize(Production<IElement> production, NormalizerContext context)
  {
    var normalizedRightHandSides = NormalizeElement(production.RightHandSide, context);
    foreach (var rightHandSide in normalizedRightHandSides)
    {
      var normalizedProduction = new Production<IAbstractSymbol>(production, production.LeftSideSymbol, new ElementList<IAbstractSymbol>(rightHandSide));
      context.Productions.Add(normalizedProduction);
    }
  }

  private List<IList<IAbstractSymbol>> NormalizeElement(IElement element, NormalizerContext context)
  {
    switch (element)
    {
      case Terminal terminal: return NormalizeSymbol(terminal);
      case NonTerminal nonTerminal: return NormalizeSymbol(nonTerminal);
      case AnnotatedElement annotated: return NormalizeAnnotatedElement(annotated, context);
      case OnceOrMoreElement onceOrMore: return NormalizeOnceOrMore(onceOrMore, context);
      case AlternativeList alternative: return NormalizeAlternation(alternative, context);
      case OptionalElement optional: return NormalizeOptional(optional, context);
      case UnorderedGroup unordered: return NormalizeUnorderedGroup(unordered, context);
      case ZeroOrMoreElement zeroOrMore: return NormalizeZeroOrMore(zeroOrMore, context);
      case ElementList<IElement> elementList: return NormalizeElementList(elementList, context);
    }
    throw new GrammarException($"unhandled grammar element '{element}'");
  }

  #region Normalizers
  private List<IList<IAbstractSymbol>> NormalizeOnceOrMore(OnceOrMoreElement onceOrMoreElement, NormalizerContext context)
  {
    var derrivedNonTerminal = new DerivedNonTerminal(context.NewDerivedNonTerminalName());
    ElementList<IElement> ruleElements = new ElementList<IElement>();
    IElement optional = new OptionalElement(derrivedNonTerminal);
    ruleElements.Add(optional);
    ruleElements.AddRange(onceOrMoreElement);
    Production<IElement> production = new(derrivedNonTerminal, ruleElements);
    Normalize(production, context);
    return NormalizeSymbol(derrivedNonTerminal);
  }

  private List<IList<IAbstractSymbol>> NormalizeAlternation(AlternativeList alternativeList, NormalizerContext context)
  {
    List<IList<IAbstractSymbol>> result = [];
    foreach (IElement element in alternativeList)
      result.AddRange(NormalizeElement(element, context));
    return result;
  }

  private List<IList<IAbstractSymbol>> NormalizeUnorderedGroup(UnorderedGroup unorderedGroup, NormalizerContext context)
  {
    // get all permutations
    List<IElement> elements = unorderedGroup.ToList();
    var permuations = GetPermutations(elements);

    // create temporary alternative list that contains all permutations of this group as alternatives
    List<IElement> alternatives = new List<IElement>();
    foreach (var permutation in permuations)
      alternatives.Add(new ElementList<IElement>(permutation));
    AlternativeList al = new AlternativeList(alternatives);

    // let the temporary alternative element normalize the permutations
    return NormalizeAlternation(al, context);
  }

  private List<IList<IAbstractSymbol>> NormalizeOptional(OptionalElement optionalElement, NormalizerContext context)
  {
    var result = NormalizeElementList(optionalElement, context);
    result.Add(new List<IAbstractSymbol>());
    return result;
  }

  private static List<IList<IAbstractSymbol>> NormalizeSymbol(IAbstractSymbol symbol)
  {
    List<IList<IAbstractSymbol>> result = [];
    List<IAbstractSymbol> options = new List<IAbstractSymbol>();
    result.Add(options);
    options.Add(symbol);
    return result;
  }

  private List<IList<IAbstractSymbol>> NormalizeElementList(IElementList<IElement> elementList, NormalizerContext context)
  {
    List<IList<IAbstractSymbol>> result = [];
    result.Add(new List<IAbstractSymbol>());
    foreach (IElement element in elementList)
    {
      var normalized = NormalizeElement(element, context);
      result = AppendList(result, normalized);
    }
    return result;
  }

  private List<IList<IAbstractSymbol>> NormalizeAnnotatedElement(AnnotatedElement annotatedElement, NormalizerContext context)
  {
    AnnotatedNonTerminal left = null;
    var existingSymbol = context.Symbols.GetSymbol(annotatedElement.Name);
    if (existingSymbol != null)
    {
      if (!(existingSymbol is AnnotatedNonTerminal annotatedNonTerminal))
        throw new GrammarException("???");
      if (!SameAnnotations(annotatedNonTerminal.Annotations, annotatedElement.Annotations))
        throw new GrammarException("???");
      left = annotatedNonTerminal;
    }

    if (left == null)
    {
      left = new AnnotatedNonTerminal(annotatedElement.Name, annotatedElement.Annotations);
      context.Symbols.Add(left);
    }

    var rightChoices = NormalizeElement(annotatedElement.Element, context);
    foreach (var rightChoice in rightChoices)
    {
      ElementList<IAbstractSymbol> right = new ElementList<IAbstractSymbol>(rightChoice);
      Production<IAbstractSymbol> production = new Production<IAbstractSymbol>(null, left, right);
      context.Productions.Add(production);
    }
    var result = new List<IList<IAbstractSymbol>>();
    List<IAbstractSymbol> list = new List<IAbstractSymbol>();
    list.Add(left);
    result.Add(list);
    return result;
  }

  private List<IList<IAbstractSymbol>> NormalizeZeroOrMore(ZeroOrMoreElement zeroOrMoreElement, NormalizerContext context)
  {
    var tmp = new DerivedNonTerminal(context.NewDerivedNonTerminalName());
    var ruleElements = new ElementList<IElement>();
    ruleElements.Add(new OptionalElement(tmp));
    ruleElements.AddRange(zeroOrMoreElement);
    var production = new Production<IElement>(tmp, ruleElements);
    Normalize(production, context);
    return NormalizeOptional(new OptionalElement(tmp), context);
  }
  #endregion

  #region Util
  private static List<IList<IAbstractSymbol>> AppendList(IList<IList<IAbstractSymbol>> list1, IList<IList<IAbstractSymbol>> list2)
  {
    List<IList<IAbstractSymbol>> result = [];
    foreach (var sub1List in list1)
    {
      foreach (var sub2List in list2)
      {
        List<IAbstractSymbol> list = [];
        list.AddRange(sub1List);
        list.AddRange(sub2List);
        result.Add(list);
      }
    }
    return result;
  }

  private static bool SameAnnotations(ISet<IAnnotation> set1, ISet<IAnnotation> set2)
  {
    if (set1.Count != set2.Count)
      return false;

    foreach (var annotation in set1)
      if (!set2.Contains(annotation))
        return false;

    return true;
  }

  private static List<IList<E>> GetPermutations<E>(IList<E> elements)
  {
    List<IList<E>> result = [];

    if (elements.Count == 1)
    {
      result.Add(new List<E>(elements));
    }
    else
    {
      for (int i = 0; i < elements.Count; i++)
      {
        var subList = new List<E>();
        for (int j = 0; j < elements.Count; j++)
        {
          if (i == j) continue;
          subList.Add(elements[j]);
        }
        var permList = GetPermutations(subList);
        foreach (var l in permList)
        {
          l.Insert(0, elements[i]);
          result.Add(l);
        }

      }
    }
    return result;
  }

  #endregion
}
