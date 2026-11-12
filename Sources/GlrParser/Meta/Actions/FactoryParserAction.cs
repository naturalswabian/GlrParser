// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Meta.Factories;
using Glr.Tree;
using System;

namespace Glr.Meta.Actions;

/// <summary>
/// Defines a factory used for creating rule results.
/// This overrides the default factory that is associated with 
/// the rule's non terminal.
/// </summary>
public class FactoryParserAction : IParserAction
{
  /// <summary>
  /// Creates a new factory parser action.
  /// </summary>
  /// <param name="factory">The factory that should be made active.</param>
  public FactoryParserAction(IRuleResultFactory factory)
  {
    _factory = factory ?? throw new ArgumentNullException(nameof(factory));
  }

  /// <inheritdoc/>
  public void Execute(DefaultTreeNode currentNode, ParserActionContext context)
  {
    context.RuleResultFactory = _factory;
  }

  /// <inheritdoc/>
  public void Finalize(DefaultTreeNode currentNode, ParserActionContext context)
  {
    // nothing to do here
  }

  /// <inheritdoc/>
  public override string ToString() => $"[new {_factory.TargetType}]";

  private readonly IRuleResultFactory _factory;
}
