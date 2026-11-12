// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements.Symbols;
using Glr.Meta.Actions;
using Glr.Meta.Factories;
using Glr.Tree;

namespace Glr.Meta.SymbolHandlers;

/// <summary>
/// A non-terminal handler that creates an instance of the object model tree.
/// </summary>
public class CreateInstanceNonTerminalHandler : INonTerminalHandler
{
  /// <summary>
  /// Creates a new non terminal handler that creates instances of the object model tree.
  /// </summary>
  /// <param name="nonTerminal">The non-terminal which identifies this handler.</param>
  /// <param name="factory">The factory that is used for creating object model instances.</param>
  /// <param name="converter">The optional converter that finalizes instance creation.</param>
  public CreateInstanceNonTerminalHandler(NonTerminal nonTerminal, IRuleResultFactory factory, NonTerminalConverter converter = null)
  {
    Converter = converter;
    NonTerminal = nonTerminal;
    _factory = factory;
  }

  /// <inheritdoc/>
  public NonTerminal NonTerminal { get; }

  /// <inheritdoc/>
  public ParserActionContext Init(DefaultTreeNode currentNode, ParserActionContext parentContext)
  {
    return new ParserActionContext
    {
      RuleResultFactory = _factory,
      Mapping = parentContext.Mapping,
      ResourceUri = parentContext.ResourceUri,
      RuleResultFactoryContext = parentContext.RuleResultFactoryContext
    };
  }

  /// <inheritdoc/>
  public virtual void ProcessChildValue(ParserActionContext context)
  {
    // does nothing
  }

  /// <inheritdoc/>
  public object Finalize(DefaultTreeNode currentNode, ParserActionContext context)
  {
    object result;
    if (Converter != null) result = Converter.Invoke(context.RuleResult);
    else result = context.RuleResult;
    context.MapNode(result, currentNode);
    return result;
  }

  /// <inheritdoc/>
  public override string ToString() => _factory.ToString();

  /// <summary>
  /// The converter that is used to convert the node into an instance.
  /// </summary>
  public NonTerminalConverter Converter { get; }

  private readonly IRuleResultFactory _factory;
}
