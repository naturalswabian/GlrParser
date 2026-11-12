// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar;
using Glr.Grammar.Elements.Symbols;
using Glr.Parser.Conflict;
using Glr.Parser.Errors;
using Glr.Parser.Table;
using Glr.Scanner;
using Glr.Tree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Glr.Parser;

/// <summary>
/// A generalized LR parser.
/// </summary>
/// <typeparam name="S">The type of the state of the underlying state machine.</typeparam>
/// <typeparam name="T">The type of syntax tree nodes.</typeparam>
public sealed class GlrParser<S, T>
    where S : class, ITableState
    where T : class, ITreeNode<T>
{
  /// <summary>
  /// Creates a new GLR parser.
  /// </summary>
  /// <param name="table">The states of the underlying state machine.</param>
  /// <param name="treeNodeFactory">The factory that is used for creating syntax tree nodes.</param>
  public GlrParser(ITable<S> table, ITreeNodeFactory<T> treeNodeFactory) : this(table, treeNodeFactory, null) { }

  /// <summary>
  /// Creates a new GLR parser.
  /// </summary>
  /// <param name="table">The states of the underlying state machine.</param>
  /// <param name="treeNodeFactory">The factory that is used for creating syntax tree nodes.</param>
  /// <param name="conflictHandler">The handler that is used when a parsing conflict occurs to resolve the conflict.</param>
  public GlrParser(ITable<S> table, ITreeNodeFactory<T> treeNodeFactory, IConflictHandler<S, T> conflictHandler)
  {
    _conflictHandler = conflictHandler;
    _table = table;
    _grammar = table.Grammar;
    _treeNodeFactory = treeNodeFactory;
    if (_conflictHandler == null)
      _conflictHandler = new DefaultConflictHandler<S, T>();
  }

  /// <summary>
  /// Parses the given token sequence. Can produce multiple derivation trees.
  /// </summary>
  /// <param name="tokenSequence">The tokens that are used as input.
  /// It is not required that the token sequence ends with the EOF symbol.</param>
  /// <returns>The syntax trees that result from parsing the given token sequence.</returns>
  public ParserResult<T> Parse(IEnumerable<Token> tokenSequence)
  {
    var allowEmptyInput = _table.Grammar.AllowsEmptyInput;
    var tokenStream = new TokenStream(tokenSequence, _grammar.EofSymbol);
    if (tokenStream.IsEmpty && allowEmptyInput)
      return new ParserResult<T>();

    var pstate = new ParserState<S, T>();
    pstate.TokenStream = tokenStream;
    pstate.StackSet = new StackSet<S, T>();
    pstate.StackSet.AddStack(new Stack<S, T>(_table.GetStartStateId()));

    while (true)
    {
      Process(pstate, tokenStream, true);
      var input = pstate.TokenStream.GetToken(TokenStream.Current);
      if (input.Terminal == _grammar.EofSymbol)
      {
        var result = GetFinalParseResult(pstate);
        if (result.Trees.Length == 0)
          ProcessSyntaxError(pstate, TokenStream.LookAhead);
        return result;
      }
    }
  }

  /// <summary>
  /// When <see langword="true"/>, the parser tries to produce parse trees
  /// even if there parse errors in the input.
  /// </summary>
  public bool IsRecoveryEnabled { set; get; }

  /// <summary>
  /// When <see langword="true"/>, annotations are normalized in the resulting syntax tree.
  /// </summary>
  public bool NormalizeAnnotations { get; set; } = true;

  /// <summary>
  /// When <see langword="true"/>, derived non terminals are eliminated from the resulting tree.
  /// </summary>
  public bool NormalizeDerivations { get; set; } = true;

  /// <summary>
  /// Gets the final parse result in case that EOF symbols has been seen.
  /// </summary>
  private ParserResult<T> GetFinalParseResult(ParserState<S, T> pstate)
  {
    var parseTrees = new List<T>();
    var lookAhead = pstate.TokenStream.GetToken(TokenStream.LookAhead);
    foreach (Stack<S, T> stack in pstate.StackSet)
    {
      var state = stack.State;
      foreach (IProduction<IAbstractSymbol> production in _table.GetReducedProductions(state, lookAhead.Terminal))
      {
        if (production == _grammar.StartProduction)
        {
          var reducedEntries = ReduceToProduction(stack, production);
          foreach (T tree in reducedEntries.Select(s => s.Node))
          {
            T resultTree = tree;
            if (NormalizeAnnotations) resultTree = NormalizeAnnotatedElements(tree);
            parseTrees.Add(resultTree);
          }
        }
      }
    }
    return new ParserResult<T>(parseTrees.ToArray(), pstate.Errors.ToArray());
  }

  /// <summary>
  /// Processes the next token.
  /// </summary>
  private void Process(ParserState<S, T> parserState, TokenStream tokenStream, bool handleErrors)
  {
    var newSet = ShiftReduce(parserState, out var affectedToken);
    if (newSet.Count == 0 && handleErrors)
      ProcessSyntaxError(parserState, affectedToken);
    else
    {
      SState(parserState, newSet);
      tokenStream.Next();
    }
  }

  private static void SState(ParserState<S, T> parserState, StackSet<S, T> newSet)
  {
    parserState.LastStackSet = parserState.StackSet;
    parserState.StackSet = newSet;
  }

  private void ProcessSyntaxError(ParserState<S, T> parserState, int affectedTokenOffset)
  {
    // get the token that is affected by the error
    Token input = parserState.TokenStream.GetToken(affectedTokenOffset);

    // report syntax error
    var error = new SyntaxError(input, GetExpectedInput(parserState.StackSet));
    parserState.Errors.Add(error);

    // try to recover from the error
    if (IsRecoveryEnabled && TryRecoverFromError(parserState, affectedTokenOffset))
      return;

    // fail, the error recovery was not successful (or not desired)
    throw new SyntaxException(error);
  }

  private bool TryRecoverFromError(ParserState<S, T> parserState, int affectedTokenOffset)
  {
    // try to remove a single superfluous token
    if (TryRemoveSingleSuperfluousToken(parserState, affectedTokenOffset))
      return true;

    // try to inject a token
    bool isInjected = GetExpectedInput(parserState.StackSet)
      .Any(expectedTerminal => TryInsertToken(expectedTerminal, parserState, affectedTokenOffset));
    if (isInjected)
      return true;

    if (TryRemoveMultipleSuperfluousTokens(parserState, affectedTokenOffset))
      return true;

    return false;
  }

  /// <summary>
  /// Tries to remove a superfluous token from the input to recover from an error.
  /// </summary>
  private bool TryRemoveSingleSuperfluousToken(ParserState<S, T> parserState, int affectedTokenOffset)
  {
    // sanity checks
    if (affectedTokenOffset != TokenStream.Current && affectedTokenOffset != TokenStream.LookAhead)
      throw new ArgumentException("Invalid token offset", nameof(affectedTokenOffset));

    var superfluousToken = parserState.TokenStream.GetToken(affectedTokenOffset);
    if (superfluousToken.Terminal == _grammar.EofSymbol) return false;

    // create a restore point if superfluous token cannot be removed
    var restorePoint = Snapshot(parserState);
    restorePoint.StreamSnapShot.Remove(affectedTokenOffset);

    if (affectedTokenOffset == TokenStream.Current)
    {
      // go back one step (in state and in token stream)
      parserState.StackSet = parserState.LastStackSet;
      parserState.TokenStream.Reset(TokenStream.Last);
    }

    Process(parserState, parserState.TokenStream, false);
    var stackSet = parserState.StackSet;

    if (stackSet.Count == 0)
    {
      // cannot remove superfluous token therefore restore the original state
      Restore(parserState, restorePoint);
      return false;
    }
    else
    {
      var expected = GetExpectedInput(stackSet);
      var input = parserState.TokenStream.GetToken(TokenStream.Current);
      if (!expected.Contains(input.Terminal))
      {
        Restore(parserState, restorePoint);
        return false;
      }
    }

    return true;
  }

  private bool TryRemoveMultipleSuperfluousTokens(ParserState<S, T> parserState, int affectedTokenOffset)
  {
    parserState.StackSet = GetRecoveryPoint(parserState.StackSet);
    var superfluousToken = parserState.TokenStream.GetToken(affectedTokenOffset);
    if (superfluousToken.Terminal == _grammar.EofSymbol) return false;
    var snapshot = parserState.TokenStream.CreateSnapshot();

    StackSet<S, T> stackSet = null;
    while (parserState.TokenStream.GetToken(affectedTokenOffset).Terminal != _grammar.EofSymbol)
    {
      snapshot.Remove(affectedTokenOffset);
      stackSet = ShiftReduce(parserState, out var _);
      if (stackSet.Count != 0)
        break;
    }

    return stackSet != null;
  }

  private StackSet<S, T> GetRecoveryPoint(StackSet<S, T> stackSet)
  {
    var currentStackSet = stackSet;
    while (currentStackSet.Count > 0)
    {
      foreach (var stack in currentStackSet)
      {
        // get the closest derived terminal (it marks the re-entry point)
        var derivedNonTerminal = stack.State.GetShiftSymbols()
          .OfType<DerivedNonTerminal>()
          .FirstOrDefault();
        if (derivedNonTerminal == null)
          continue;

        // create the error tree node
        var result = Shift(derivedNonTerminal, currentStackSet, null);

        if (result.Count == 0)
          continue;

        return result;
      }

      // no recovery stack found so far: try the parents
      var parentStackSet = new StackSet<S, T>();
      foreach (var s in currentStackSet)
        parentStackSet.Add(s.Parents);
      currentStackSet = parentStackSet;
    }

    // no recovery stack found: return an empty result
    return new StackSet<S, T>();
  }

  private sealed class RestorePoint
  {
    public ParserState<S, T> State { set; get; }
    public ITokenStreamSnapshot StreamSnapShot { set; get; }
  }

  private static RestorePoint Snapshot(ParserState<S, T> source)
  {
    var result = new RestorePoint();
    result.StreamSnapShot = source.TokenStream.CreateSnapshot();
    result.State = new ParserState<S, T>();
    result.State.StackSet = source.StackSet;
    result.State.LastStackSet = source.LastStackSet;
    return result;
  }

  private static void Restore(ParserState<S, T> source, RestorePoint restorePoint)
  {
    source.StackSet = restorePoint.State.StackSet;
    source.LastStackSet = restorePoint.State.LastStackSet;
    source.TokenStream.Restore(restorePoint.StreamSnapShot);
  }

  /// <summary>
  /// Tries inserting an injected token in order to recover from an error.
  /// </summary>
  private bool TryInsertToken(Terminal injectedTerminal, ParserState<S, T> parserState, int affectedTokenOffset)
  {
    var input = parserState.TokenStream.GetToken(affectedTokenOffset);
    var prev = parserState.TokenStream.GetToken(TokenStream.Last);
    if (prev != null && prev.Lexeme == null)
      return false;

    // an 'eof' can only be inserted if the look-ahead is also an eof.
    // DEBT: this makes no sense at all. remove
    if (injectedTerminal == _grammar.EofSymbol)
    {
      var lookAhead = parserState.TokenStream.GetToken(TokenStream.LookAhead);
      if (lookAhead.Terminal != _grammar.EofSymbol)
        return false;
    }

    var restorePoint = Snapshot(parserState);

    // inject token based on the given terminal
    var injectedToken = new Token(injectedTerminal, null, SourceContext.InjectedSourceContext, null);
    restorePoint.StreamSnapShot.Insert(affectedTokenOffset, injectedToken);
    if (affectedTokenOffset == TokenStream.Current)
    {
      parserState.TokenStream.Reset(TokenStream.Last);
      parserState.StackSet = parserState.LastStackSet;
    }

    // re-consume the last input but now with the injected terminal as look-ahead instead
    Process(parserState, parserState.TokenStream, false);
    if (parserState.StackSet.Count == 0)
    {
      Restore(parserState, restorePoint);
      return false;
    }

    // consume the injected token
    Process(parserState, parserState.TokenStream, false);
    if (parserState.StackSet.Count == 0)
    {
      Restore(parserState, restorePoint);
      return false;
    }

    // check if the failed input token can now be consumed
    var expectedInput = GetExpectedInput(parserState.StackSet);
    if (!expectedInput.Contains(input.Terminal))
    {
      Restore(parserState, restorePoint);
      return false;
    }

    return true;
  }

  /// <summary>
  /// Gets the set of terminals that could be consumed using the given stack set.
  /// </summary>
  private HashSet<Terminal> GetExpectedInput(StackSet<S, T> set)
  {
    HashSet<Terminal> expectedInput = [];
    foreach (Stack<S, T> stack in set)
    {
      var lookAhead = _table.GetLookAhead(stack.State);
      expectedInput.UnionWith(lookAhead);
    }
    return expectedInput;
  }

  private List<ParserNodeState<S, T>> ReduceToProduction(Stack<S, T> stack, IProduction<IAbstractSymbol> production)
  {
    int reduceSteps = production.ReduceSteps;
    var reducedTree = stack.Pop(reduceSteps);
    List<ParserNodeState<S, T>> reducedEntries = [];

    foreach (var reducedStackTree in reducedTree)
      reducedEntries.Add(ReduceTree(reducedStackTree, production));

    if (reducedEntries.Count > 1)
      _conflictHandler.ResolveConflict(reducedEntries, this);

    return reducedEntries;
  }

  private ParserNodeState<S, T> ReduceTree(StackTree<S, T> tree, IProduction<IAbstractSymbol> production)
  {
    StackTree<S, T> lastTree;
    List<T> children = new List<T>();
    do
    {
      T node = tree.Stack.Node;
      if (node != null)
      {
        if (node.Symbol is DerivedNonTerminal && NormalizeDerivations)
        {
          var derrivedChildren = node.Children;
          for (int i = derrivedChildren.Count - 1; i >= 0; i--)
            children.Add(derrivedChildren[i]);
        }
        else
          children.Add(node);
      }
      lastTree = tree;
      tree = tree.Parent;
    } while (tree != null);
    children.Reverse();
    T newNode = _treeNodeFactory.CreateNonTerminalNode(production, children);
    ParserNodeState<S, T> entry = new ParserNodeState<S, T>(lastTree.Stack, newNode);
    return entry;
  }

  /// <summary>
  /// Normalizes the node. Nodes which are artificially injected are removed.
  /// </summary>
  /// <param name="node">The node that should be normalized.</param>
  /// <returns>The normalized node.</returns>
  public T NormalizeAnnotatedElements(T node)
  {
    List<T> children = new List<T>();
    NormalizeAnnotatedElements(node, children);
    T result = _treeNodeFactory.CreateNonTerminalNode(node.Production, children);
    return result;
  }

  private void NormalizeAnnotatedElements(T node, List<T> children)
  {
    foreach (T child in node.Children)
    {
      IAbstractSymbol symbol = child.Symbol;
      if (symbol is NonTerminal nonTerminal)
      {
        if (nonTerminal is AnnotatedNonTerminal)
        {
          NormalizeAnnotatedElements(child, children);
        }
        else
        {
          var subChildren = new List<T>();
          NormalizeAnnotatedElements(child, subChildren);
          var production = child.Production;
          T newTreeNode = _treeNodeFactory.CreateNonTerminalNode(production.Origin, subChildren);
          children.Add(newTreeNode);
        }
      }
      else
      {
        children.Add(child);
      }
    }
  }

  private StackSet<S, T> ShiftReduce(ParserState<S, T> parserState, out int affectedTokenOffset)
  {
    var input = parserState.TokenStream.GetToken(TokenStream.Current);
    var lookAhead = parserState.TokenStream.GetToken(TokenStream.LookAhead);

    T node = _treeNodeFactory.CreateTerminalNode(input);
    StackSet<S, T> shifted = Shift(input?.Terminal, parserState.StackSet, node);
    affectedTokenOffset = shifted.Count == 0 ? TokenStream.Current : TokenStream.LookAhead;
    return Reduce(lookAhead, shifted);
  }

  private StackSet<S, T> Reduce(Token lookAhead, StackSet<S, T> stackSet)
  {
    StackSet<S, T> result = new StackSet<S, T>();
    if (lookAhead.Terminal == null)
      return result;
    result.Add(stackSet);

    foreach (Stack<S, T> stack in stackSet)
    {
      S state = stack.State;
      foreach (IProduction<IAbstractSymbol> production in _table.GetReducedProductions(state, lookAhead.Terminal))
      {
        var reducedEntries = ReduceToProduction(stack, production);
        for (int k = 0; k < reducedEntries.Count; k++)
        {
          ParserNodeState<S, T> entry = reducedEntries[k];
          StackSet<S, T> shiftedReducedSet = Shift(entry.Stack, production.LeftSideSymbol, entry.Node);
          result.Add(shiftedReducedSet);
          result.Add(Reduce(lookAhead, shiftedReducedSet));
        }
      }
    }

    return result;
  }

  private StackSet<S, T> Shift(Stack<S, T> stack, IAbstractSymbol symbol, T node)
  {
    StackSet<S, T> result = new StackSet<S, T>();
    S state = stack.State;
    S newItemState = _table.GetShiftFollowState(state, symbol);
    if (newItemState != null)
    {
      Stack<S, T> newStack = stack.Push(newItemState, node);
      result.AddStack(newStack);
    }
    return result;
  }

  private StackSet<S, T> Shift(IAbstractSymbol symbol, StackSet<S, T> stackSet, T node)
  {
    StackSet<S, T> result = new StackSet<S, T>();
    if (symbol == null) return result;
    foreach (Stack<S, T> stack in stackSet)
      result.Add(Shift(stack, symbol, node));
    return result;
  }

  private readonly ITreeNodeFactory<T> _treeNodeFactory;
  private readonly IGrammar _grammar;
  private readonly ITable<S> _table;
  private readonly IConflictHandler<S, T> _conflictHandler;
}
