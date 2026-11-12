// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using Glr.Parser;
using System.Reflection;

namespace Glr.Meta.Actions;

/// <summary>
/// The base implementation for all storage related parser actions that
/// write to a field or property. This action creates an object where the values can be stored in.
/// It uses the factory defined within <see cref="ParserActionContext"/>.
/// </summary>
public abstract class StorageParserAction : ParserAction
{
  /// <summary>
  /// Creates a new storage action based on the given field.
  /// </summary>
  /// <param name="fieldInfo">The field within which the values are stored in.</param>
  protected StorageParserAction(FieldInfo fieldInfo) : base(fieldInfo) { }

  /// <summary>
  /// Creates a new storage action based on the given property.
  /// </summary>
  /// <param name="propertyInfo">The property within which the values are stored in.</param>
  protected StorageParserAction(PropertyInfo propertyInfo) : base(propertyInfo) { }

  /// <summary>
  /// Creates a new container object.
  /// </summary>
  /// <param name="context">The current context that should be considered when creating objects.</param>
  protected void CreateContainer(ParserActionContext context)
  {
    // if the container already exists do nothing
    if (context.RuleResult != null) return;

    if (context.RuleResultFactory == null)
      throw new ParserException($"no factory defined in current parser context for creating instances of type '{_ownerType}'");

    context.RuleResult = context.RuleResultFactory.CreateInstance(context.RuleResultFactoryContext);
  }
}
