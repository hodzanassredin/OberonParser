This is Oberon parsers library.
Based on compiler generator Coco/R(C# version).

Implemented:

	1. Component Pascal : parser, AST, Pretty Printer, Sym Table.
	2. Active Oberon : parser, AST, Pretty Printer, Sym Table.
	3. AO to CP code converter.

Limitations:

	1. CP [code] attributes not supported.
	2. AO grammar based on outdated lang report, not all modern features are supported.
	3. Main problem in both parsers designator cast vs func application resolution. Sometaimes can fail.
	4. Code converter has too many bugs. :-(
	5. Use it only for learning purposes.
	6. Code is not clean and have to be refactored.
