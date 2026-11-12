# Generalized LR Parser

A GLR parser (Generalized Left-to-Right Parser) is an extension of standard LR parsing designed to handle
non-deterministic and ambiguous context-free grammars. This implementation allows the dynamic definition of
grammars without the need of generating code. Grammars can be fully defined by an object model leaving the
full control to the application. For more comfort, grammars can also be defined by annotating classes
representing AST (Abstract Syntax Tree) nodes.
