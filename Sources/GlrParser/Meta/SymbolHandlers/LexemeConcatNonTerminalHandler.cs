// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements.Symbols;
using Glr.Meta.Actions;
using Glr.Tree;
using System.Text;

namespace Glr.Meta.SymbolHandlers;

/// <summary>
/// A non terminal handler that concatenates the child nodes to a single string.
/// It is expected that all children evaluate to a string.
/// </summary>
public class LexemeConcatNonTerminalHandler : INonTerminalHandler
{
  /// <summary>
  /// Creates a new terminal handler that concatenates the lexemes of child nodes.
  /// </summary>
  /// <param name="nonTerminal">The non-terminal that identifies this handler.</param>
  /// <param name="valueConverter">The optional converter.</param>
  public LexemeConcatNonTerminalHandler(NonTerminal nonTerminal, ValueConverter valueConverter = null)
  {
    NonTerminal = nonTerminal;
    _valueConverter = valueConverter;
  }

  /// <inheritdoc/>
  public NonTerminal NonTerminal { get; }

  /// <inheritdoc/>
  public override string ToString() => "concatenates lexemes";

  /// <inheritdoc/>
  public ParserActionContext Init(DefaultTreeNode currentNode, ParserActionContext parentContext)
  {
    // create context that stores the concatenation in progress
    return new ParserActionContext
    {
      Data = new StringBuilder(),
      Mapping = parentContext.Mapping,
      ResourceUri = parentContext.ResourceUri,
      RuleResultFactoryContext = parentContext.RuleResultFactoryContext
    };
  }

  /// <inheritdoc/>
  public void ProcessChildValue(ParserActionContext context)
  {
    // append the current item to the concatenation
    ((StringBuilder)context.Data).Append(context.CurrentItem.ToString());
  }

  /// <inheritdoc/>
  public virtual object Finalize(DefaultTreeNode currentNode, ParserActionContext context)
  {
    object result;
    // create final concatenated lexeme
    var lexeme = ((StringBuilder)context.Data).ToString();
    if (_valueConverter != null) result = _valueConverter(NonTerminal, lexeme);
    else result = lexeme;
    context.MapNode(result, currentNode);
    return result;
  }

  private readonly ValueConverter _valueConverter;
}
