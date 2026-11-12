// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

namespace Glr.Meta.SymbolHandlers;

/// <summary>
/// Converts the given value into another value.
/// </summary>
/// <param name="value">The value that should be converted.</param>
/// <returns>The converted value.</returns>
public delegate object NonTerminalConverter(object value);
