// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using Glr.Tree;
using System.Collections.Generic;

namespace Glr.Meta.Transform;

/// <summary>
/// The definition of a transformation which converts a syntax tree into a corresponding object model.
/// </summary>
public interface ITreeTransformer
{
  /// <summary>
  /// Transforms the given syntax tree, that is represented by the <paramref name="treeNode"/> into a corresponding object model.
  /// </summary>
  /// <param name="resourceUri">The resource Uri from which the syntax tree was created.</param>
  /// <param name="treeNode">The root syntax tree node.</param>
  /// <param name="mapping">Resulting mapping from the original syntax tree node to created object model instances.</param>
  /// <param name="factoryContext">The context of the factory which is used when creating nodes of the object model.</param>
  /// <returns></returns>
  object Transform(string resourceUri, DefaultTreeNode treeNode, IDictionary<object, IList<DefaultTreeNode>> mapping, object factoryContext);
}
