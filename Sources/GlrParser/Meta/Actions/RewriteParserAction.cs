// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Meta.Factories;
using Glr.Tree;
using System;
using System.Reflection;

namespace Glr.Meta.Actions;

/// <summary>
/// A rewrite parser action.
/// </summary>
public class RewriteParserAction : StorageParserAction
{
  /// <summary>
  /// Creates a new parser rewrite action that stores its data within the given field.
  /// </summary>
  /// <param name="fieldInfo">The field within which the data should be stored.</param>
  /// <param name="factory">The factory which overrides the factory from the <see cref="ParserActionContext"/>.</param>
  public RewriteParserAction(FieldInfo fieldInfo, IRuleResultFactory factory) : base(fieldInfo)
  {
    _factory = factory;
    Type[] generics = _memberType.GetGenericArguments();
    if (generics.Length != 0) throw new ArgumentException("field with generic arguments is not supported");
  }

  /// <summary>
  /// Creates a new parser rewrite action that stores its data within the given property.
  /// </summary>
  /// <param name="propertyInfo">The property within which the data should be stored.</param>
  /// <param name="factory">The factory which overrides the factory from the <see cref="ParserActionContext"/>.</param>
  public RewriteParserAction(PropertyInfo propertyInfo, IRuleResultFactory factory) : base(propertyInfo)
  {
    _factory = factory;
    Type[] generics = _memberType.GetGenericArguments();
    if (generics.Length != 0) throw new ArgumentException("field with generic arguments is not supported");
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
    var instance = _factory.CreateInstance(context.RuleResultFactoryContext);

    // set value
    _setAction.Invoke(instance, context.CurrentItem);
    context.CurrentItem = instance;
  }

  /// <inheritdoc/>
  protected override string Operator => "~";

  private readonly IRuleResultFactory _factory;
}
