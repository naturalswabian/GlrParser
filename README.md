# <img src="Resources/GlrParserBanner.svg" valign="middle" height="60">&nbsp;Generalized&nbsp;LR&nbsp;Parser

A GLR parser (Generalized Left-to-Right Parser) is an extension of standard LR parsing designed to handle
non-deterministic and ambiguous context-free grammars. This implementation allows the dynamic definition of
grammars without the need of generating code. Grammars can be fully defined by an object model leaving the
full control to the application. For more comfort, grammars can also be defined by annotating classes
representing AST (Abstract Syntax Tree) nodes. Examples can be found in the unit tests. First starting points
might be:

- [GrammarTests][GrammarTests] for annotated AST classes
- [AbcGrammar][AbcGrammar] for defining a grammar via object model

[GrammarTests]: ./Tests/GlrParser.Tests/Meta/Grammar/GrammarTests.cs
[AbcGrammar]: ./Tests/GlrParser.Tests/Parser/AbcGrammar.cs
