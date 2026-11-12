// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar;
using Glr.Meta.Transform;
using Glr.Parser;
using Glr.Parser.Conflict;
using Glr.Parser.Errors;
using Glr.Parser.Table;
using Glr.Scanner;
using Glr.Tree;
using System.Collections.Generic;

namespace Glr.Meta;

/// <summary>
/// A parser that is defined on a grammar.
/// </summary>
/// <typeparam name="T">The root type of the resulting object model.</typeparam>
public sealed class ClientParser<T> : IParser<T> where T : class
{
  /// <summary>
  /// Creates a new parser.
  /// </summary>
  /// <param name="grammar">The grammar on which the parser is based on.</param>
  /// <param name="table">The state machine of the parser.</param>
  /// <param name="isRecoveryEnabled">When <see langword="true"/>, the parser operates with error recovery.</param>
  /// <param name="conflictHandler">Conflict resolving handler.</param>
  public ClientParser(IGrammar grammar, ITable<ItemState> table, bool isRecoveryEnabled, IConflictHandler<ItemState, DefaultTreeNode> conflictHandler = null)
  {
    Grammar = grammar;
    _table = table;
    _conflictHandler = conflictHandler;
    IsRecoveryEnabled = isRecoveryEnabled;
  }

  /// <summary>
  /// The <see cref="ITreeTransformer"/> that is used to create the object model from the syntax tree.
  /// </summary>
  public ITreeTransformer TreeTransformer { set; get; }

  /// <inheritdoc/>
  public IScanner CreateScanner() => new KeywordScanner(Grammar.EofSymbol, Grammar.Terminals, Grammar.SpecialTerminals, Grammar.SkipTerminals);

  /// <summary>
  /// The grammar that was used for this parser.
  /// </summary>
  public IGrammar Grammar { get; }

  /// <summary>
  /// When <see langword="true"/>, this parser operates with error recovery.
  /// </summary>
  public bool IsRecoveryEnabled { get; }

  /// <summary>
  /// When <see langword="true"/>, node mappings between model nodes and syntax tree nodes are created.
  /// </summary>
  public bool CreateNodeMapping { get; } = true;

  /// <inheritdoc/>
  public IParserResult<T> Parse(IScannerSource source, object factoryContext)
  {
    var scanner = CreateScanner();
    return Parse(scanner.GetTokenSequence(source), source.Name, factoryContext);
  }

  /// <inheritdoc/>
  public IParserResult<T> Parse(IEnumerable<Token> tokens, string resourceUri, object factoryContext)
  {
    if (TreeTransformer == null)
      throw new ParserException("tree transformer required");

    var treeNodeFactory = new DefaultTreeNodeFactory();
    var parser = new GlrParser<ItemState, DefaultTreeNode>(_table, treeNodeFactory, _conflictHandler);
    parser.IsRecoveryEnabled = IsRecoveryEnabled;
    parser.NormalizeAnnotations = false;
    parser.NormalizeDerivations = false;
    var parserResult = parser.Parse(tokens);
    var clientParserResult = new ClientParserResult();
    if (parserResult.Trees.Length == 1)
    {
      if (CreateNodeMapping)
        clientParserResult.ModelMap = new Dictionary<object, IList<DefaultTreeNode>>();
      clientParserResult.Model = (T)TreeTransformer.Transform(resourceUri, parserResult.Trees[0], clientParserResult.ModelMap, factoryContext);
      clientParserResult.Tree = parserResult.Trees[0];
      clientParserResult.Errors = parserResult.Errors;
    }
    return clientParserResult;
  }

  private readonly IConflictHandler<ItemState, DefaultTreeNode> _conflictHandler;
  private readonly ITable<ItemState> _table;

  private sealed class ClientParserResult : IParserResult<T>
  {
    /// <inheritdoc/>
    public T Model { set; get; }

    /// <inheritdoc/>
    public DefaultTreeNode Tree { set; get; }

    /// <inheritdoc/>
    public IDictionary<object, IList<DefaultTreeNode>> ModelMap { set; get; }

    /// <inheritdoc/>
    public IList<SyntaxError> Errors { set; get; }
  }
}
