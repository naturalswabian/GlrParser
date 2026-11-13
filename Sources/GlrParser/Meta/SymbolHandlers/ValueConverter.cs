// Generalized LR Parser
// Copyright © 2019 Jan B. Freuer
// All rights reserved

using Glr.Grammar.Elements.Symbols;

namespace Glr.Meta.SymbolHandlers;

/// <summary>
/// Converts the given value (in the context of the given symbol) 
/// to a non abstract syntax tree node value.
/// </summary>
public delegate object ValueConverter(IAbstractSymbol symbol, string value);
