// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Tree;
using System.Reflection;

namespace Glr.Meta.Actions;

/// <summary>
/// Checks whether a symbol has been processed while parsing. Stores '<see langword="true"/>' within the
/// associated field or property if it has been processed. Otherwise '<see langword="false"/>' is stored.
/// </summary>
public class ExistsParserAction : StorageParserAction
{
  /// <summary>
  /// Creates a new 'exists' parser action that stores the result to the given field.
  /// </summary>
  /// <param name="fieldInfo">The field that should receive the result.</param>
  public ExistsParserAction(FieldInfo fieldInfo) : base(fieldInfo)
  {
  }

  /// <summary>
  /// Creates a new 'exists' parser action that stores the result to the given property.
  /// </summary>
  /// <param name="propertyInfo">The property that should receive the result.</param>
  public ExistsParserAction(PropertyInfo propertyInfo) : base(propertyInfo)
  {
  }

  /// <inheritdoc/>
  public override void Execute(DefaultTreeNode currentNode, ParserActionContext context)
  {
    // nothing to do here
  }

  /// <inheritdoc/>
  public override void Finalize(DefaultTreeNode currentNode, ParserActionContext context)
  {
    // ensure that the container is available
    CreateContainer(context);

    _setAction(context.RuleResult, true);
  }

  /// <inheritdoc/>
  protected override string Operator => "?=";
}
