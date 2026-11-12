// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using Glr.Meta.SymbolHandlers;
using System;
using System.Reflection;
using System.Text;

namespace Glr.Meta.Attributes;

/// <summary>
/// Defines an enumeration as grammar enumeration rule. The literals of that
/// enumeration are used as terminals unless annotated with the 
/// <see cref="NameAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Enum)]
public sealed class EnumRuleAttribute : RuleBaseAttribute
{
  /// <inheritdoc/>
  public override Type GetInstanceTypeForType(Type type)
  {
    return typeof(EnumNode);
  }

  /// <inheritdoc/>
  public override string GetDescriptionForType(Type type)
  {
    if (!type.IsEnum) throw new ArgumentException($"{type} is not an enumeration type");
    StringBuilder builder = new StringBuilder();
    bool isFirst = true;
    foreach (object enumValue in Enum.GetValues(type))
    {
      string enumName = enumValue.ToString();
      if (isFirst) isFirst = false;
      else builder.Append('|');
      FieldInfo info = type.GetField(enumName);
      NameAttribute symbolicName = info.GetCustomAttribute<NameAttribute>();
      string value = enumName;
      if (symbolicName != null)
        value = symbolicName.Value;
      builder.Append("Lexeme=");
      builder.Append('\'');
      builder.Append(value);
      builder.Append('\'');
    }
    return builder.ToString();
  }

  /// <inheritdoc/>
  public override NonTerminalConverter ConverterForType(Type type)
  {
    return (object lexeme) => Convert(type, (lexeme as EnumNode)?.Lexeme);
  }

  private static object Convert(Type enumType, string lexeme)
  {
    object result = null;
    foreach (object enumValue in Enum.GetValues(enumType))
    {
      string enumName = enumValue.ToString();
      MemberInfo[] info = enumType.GetMember(enumName);
      NameAttribute symbolicName = info[0].GetCustomAttribute<NameAttribute>();
      string value = enumName;
      if (symbolicName != null)
        value = symbolicName.Value;
      if (value == lexeme)
      {
        result = enumValue;
        break;
      }
    }
    return result;
  }
}
