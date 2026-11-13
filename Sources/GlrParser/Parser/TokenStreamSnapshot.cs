// Generalized LR Parser
// Copyright © 2020 Jan B. Freuer
// All rights reserved

using Glr.Scanner;

namespace Glr.Parser;

internal interface ITokenStreamSnapshot
{
  /// <summary>
  /// Inserts an artificial token into the token stream at the given offset.
  /// </summary>
  void Insert(int offset, Token token);

  /// <summary>
  /// Removes a token from the token stream at the given offset.
  /// </summary>
  void Remove(int offset);
}
