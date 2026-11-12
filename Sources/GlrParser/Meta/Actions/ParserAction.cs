// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Tree;
using System;
using System.Reflection;

namespace Glr.Meta.Actions;

/// <summary>
/// A parser action that is based on a field or property.
/// </summary>
public abstract class ParserAction : IParserAction
{
  /// <summary>
  /// Creates a new parser action that is based on the given field.
  /// </summary>
  /// <param name="fieldInfo">The field which this action targets.</param>
  protected ParserAction(FieldInfo fieldInfo)
  {
    _setAction = fieldInfo.SetValue;
    _getAction = fieldInfo.GetValue;
    _memberType = fieldInfo.FieldType;
    _memberName = fieldInfo.Name;
    _ownerType = fieldInfo.DeclaringType;
    IsField = true;
  }

  /// <summary>
  /// Creates a new parser action that is based on the given property.
  /// </summary>
  /// <param name="propertyInfo">The property which this action targets.</param>
  protected ParserAction(PropertyInfo propertyInfo)
  {
    _setAction = propertyInfo.SetValue;
    _getAction = propertyInfo.GetValue;
    _memberType = propertyInfo.PropertyType;
    _memberName = propertyInfo.Name;
    _ownerType = propertyInfo.DeclaringType;
  }

  /// <inheritdoc/>
  public override bool Equals(object obj)
  {
    if (this == obj) return true;
    if (!(obj is ParserAction operation)) return false;
    if (_ownerType != operation._ownerType) return false;
    if (_memberName != operation._memberName) return false;
    if (_memberType != operation._memberType) return false;
    return true;
  }

  /// <inheritdoc/>
  public override int GetHashCode()
  {
    return _ownerType.GetHashCode() * 23 + _memberType.GetHashCode() * 17 + _memberName.GetHashCode();
  }

  /// <inheritdoc/>
  public override string ToString()
  {
    return $"[{_ownerType.Name} . {_memberName} {Operator}]";
  }

  /// <inheritdoc/>
  public abstract void Execute(DefaultTreeNode currentNode, ParserActionContext context);

  /// <inheritdoc/>
  public abstract void Finalize(DefaultTreeNode currentNode, ParserActionContext context);

  /// <summary>
  /// The textual representation of the operator that this action stands for.
  /// </summary>
  protected abstract string Operator { get; }

  /// <summary>
  /// When <see langword="true"/>, this action is defined for a field.
  /// </summary>
  protected bool IsField { get; }

  /// <summary>
  /// The type of the object that owns the underlying field or property.
  /// </summary>
  protected readonly Type _ownerType;

  /// <summary>
  /// The value type of the underlying field or property.
  /// </summary>
  protected readonly Type _memberType;

  /// <summary>
  /// The action that is used to store a value from the underlying field or property.
  /// </summary>
  protected readonly Action<object, object> _setAction;

  /// <summary>
  /// The function that is used to retrieve a value from the underlying field or property.
  /// </summary>
  protected readonly Func<object, object> _getAction;

  /// <summary>
  /// The name of the underlying field or property.
  /// </summary>
  protected readonly string _memberName;
}
