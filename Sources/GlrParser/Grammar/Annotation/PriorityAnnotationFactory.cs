// Generalized LR Parser
// Copyright © 2018 Jan B. Freuer
// All rights reserved

using System.Collections.Generic;

namespace Glr.Grammar.Annotations;

/// <summary>
/// A factory that creates priority based annotations.
/// </summary>
public class PriorityAnnotationFactory : IAnnotationFactory
{
  /// <inheritdoc/>
  public IAnnotation CreateAnnotation(string id, List<string> args)
  {
    IAnnotation annotation = null;
    int prio = 0;
    switch (id)
    {
      case "short":
        if (args.Count != 0) throw new AnnotationException(InvalidArgumentMessage);
        annotation = new PriorityAnnotation(PriorityAnnotationType.Short);
        break;

      case "long":
        if (args.Count != 0) throw new AnnotationException(InvalidArgumentMessage);
        annotation = new PriorityAnnotation(PriorityAnnotationType.Long);
        break;

      case "prio":
        if (args.Count != 1) throw new AnnotationException(InvalidArgumentMessage);
        if (!int.TryParse(args[0], out prio)) throw new AnnotationException($"invalid priority: [{args[0]}]");
        annotation = new PriorityAnnotation(PriorityAnnotationType.Priority, prio);
        break;

    }
    if (annotation == null)
      throw new AnnotationException("invalid annotation: " + id);
    return annotation;
  }

  private const string InvalidArgumentMessage = "invalid number of arguments";
}
