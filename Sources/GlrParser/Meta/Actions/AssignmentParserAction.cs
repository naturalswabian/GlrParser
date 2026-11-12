// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Annotations;
using Glr.Tree;
using System.Reflection;

namespace Glr.Meta.Actions;

/// <summary>
/// Assigns a value to the field or property.
/// </summary>
public class AssignmentParserAction : StorageParserAction, IAnnotation
{
  /// <summary>
  /// Creates a new 'assignment' parser action that stores the result within the given field.
  /// </summary>
  /// <param name="fieldInfo">The field that should receive the value.</param>
  public AssignmentParserAction(FieldInfo fieldInfo) : base(fieldInfo)
  {
  }

  /// <summary>
  /// Creates a new 'assignment' parser action that stores the result within the given property.
  /// </summary>
  /// <param name="propertyInfo">The property that should receive the value.</param>
  public AssignmentParserAction(PropertyInfo propertyInfo) : base(propertyInfo)
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

    _setAction.Invoke(context.RuleResult, context.CurrentItem);
  }

  /// <inheritdoc/>
  protected override string Operator => "=";
}
