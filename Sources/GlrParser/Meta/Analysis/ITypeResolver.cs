// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using System;

namespace Glr.Meta.Analysis;

/// <summary>
/// Resolves <see cref="System.Type"/>s from grammar symbols.
/// </summary>
public interface ITypeResolver
{
  /// <summary>
  /// Gets the corresponding type of the given grammar symbol.
  /// </summary>
  Type GetType(string symbolName);
}
