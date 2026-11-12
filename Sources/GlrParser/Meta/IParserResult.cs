// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using Glr.Parser.Errors;
using Glr.Tree;
using System.Collections.Generic;

namespace Glr.Meta;

/// <summary>
/// The result of the <see cref="IParser{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the root node of the resulting object model.</typeparam>
public interface IParserResult<out T> where T : class
{
  /// <summary>
  /// The root node of the object model.
  /// </summary>
  T Model { get; }

  /// <summary>
  /// The root node of the syntax tree.
  /// </summary>
  DefaultTreeNode Tree { get; }

  /// <summary>
  /// The mapping from object model nodes to syntax tree nodes. Since the 
  /// generate model nodes might not be unique there can be multiple of them.
  /// </summary>
  IDictionary<object, IList<DefaultTreeNode>> ModelMap { get; }

  /// <summary>
  /// The parsing errors. Is empty when no errors occurred.
  /// </summary>
  IList<SyntaxError> Errors { get; }
}
