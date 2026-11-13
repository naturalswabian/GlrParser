// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using Glr.Meta.Factories;
using Glr.Tree;
using System.Collections.Generic;

namespace Glr.Meta.Actions;

/// <summary>
/// Defines contextual information for the transformation of a syntax tree into an object model.
/// <see cref="IParserAction"/> do read and manipulate the context during the transformation.
/// </summary>
public class ParserActionContext
{
  /// <summary>
  /// Creates a new context used while transforming the syntax tree into an object model.
  /// </summary>
  public ParserActionContext() { }

  /// <summary>
  /// Holds handler specific data.
  /// </summary>
  public object Data { set; get; }

  /// <summary>
  /// The default factory used for creating the result of a rule.
  /// </summary>
  public IRuleResultFactory RuleResultFactory { set; get; }

  /// <summary>
  /// The context used for the <see cref="RuleResultFactory"/>.
  /// </summary>
  public object RuleResultFactoryContext { set; get; }

  /// <summary>
  /// The model element that receives storage data and is also used 
  /// as the result of a production rule.
  /// </summary>
  public object RuleResult { set; get; }

  /// <summary>
  /// The URI of the resource that is transformed into an object model.
  /// </summary>
  public string ResourceUri { set; get; }

  /// <summary>
  /// The model element that has been most recently created within this context.
  /// </summary>
  public object CurrentItem { get; set; }

  /// <summary>
  /// Provides mappings from resulting model object to the
  /// original <see cref="DefaultTreeNode"/>.
  /// </summary>
  public IDictionary<object, IList<DefaultTreeNode>> Mapping { set; get; }

  /// <summary>
  /// Maps the given model value to the tree node.
  /// </summary>
  public void MapNode(object modelValue, DefaultTreeNode node)
  {
    if (Mapping == null || modelValue == null || node == null)
      return;

    if (!Mapping.TryGetValue(modelValue, out var nodes))
    {
      nodes = new List<DefaultTreeNode>();
      Mapping.Add(modelValue, nodes);
    }
    nodes.Add(node);
  }
}
