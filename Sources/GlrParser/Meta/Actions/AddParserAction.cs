// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Tree;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Glr.Meta.Actions;

/// <summary>
/// A storage action that adds the current value to a collection that is 
/// provided by a field or by a property.
/// </summary>
public sealed class AddParserAction : StorageParserAction
{
  /// <summary>
  /// Creates a new add parser action based on a field.
  /// </summary>
  /// <param name="fieldInfo">The field to which the current value should be written to.</param>
  public AddParserAction(FieldInfo fieldInfo) : base(fieldInfo)
  {
    AssertGenericParameters();
  }

  /// <summary>
  /// Creates a new add parser action based on a property.
  /// </summary>
  /// <param name="propertyInfo">The property to which the current value should be written to.</param>
  public AddParserAction(PropertyInfo propertyInfo) : base(propertyInfo)
  {
    AssertGenericParameters();
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

    // get list item type
    Type itemType = _memberType.GetGenericArguments()[0];

    // get/create list field
    object fieldValue = _getAction(context.RuleResult);
    if (fieldValue == null)
    {
      Type listType = typeof(List<>);
      Type type = listType.MakeGenericType(itemType);
      fieldValue = Activator.CreateInstance(type);
      _setAction(context.RuleResult, fieldValue);
    }

    MethodInfo addMethod = fieldValue.GetType().GetMethod("Add", [itemType]);
    addMethod.Invoke(fieldValue, [context.CurrentItem]);
  }

  /// <inheritdoc/>
  public override bool Equals(object obj)
  {
    if (!(obj is AddParserAction)) return false;
    return base.Equals(obj);
  }

  /// <inheritdoc/>
  public override int GetHashCode()
  {
    return base.GetHashCode();
  }

  /// <inheritdoc/>
  protected override string Operator => "+=";

  private void AssertGenericParameters()
  {
    Type[] generics = _memberType.GetGenericArguments();
    if (generics.Length != 1)
    {
      var fieldOrProperty = IsField ? "field" : "property";
      throw new ArgumentException($"{fieldOrProperty} '{_memberName}' in {_ownerType} requires exactly one single generic arguments but got {generics.Length}");
    }
  }
}
