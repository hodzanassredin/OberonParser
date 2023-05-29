using Ast;



using System;



public class Parser {
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _integer = 2;
	public const int _real = 3;
	public const int _character = 4;
	public const int _string = 5;
	public const int maxT = 74;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

Module module;  // module



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void Oberon() {
		Module();
	}

	void Module() {
		Expect(6);
		Expect(1);
		module = new Module(t.val); 
		Expect(7);
		if (la.kind == 13) {
			ImportList();
		}
		DeclSeq();
		if (la.kind == 8) {
			Get();
			StatementSeq();
		}
		if (la.kind == 9) {
			Get();
			StatementSeq();
		}
		Expect(10);
		Expect(1);
		Expect(11);
	}

	void number() {
		if (la.kind == 2) {
			Get();
		} else if (la.kind == 3) {
			Get();
		} else SynErr(75);
	}

	void ImportList() {
		Expect(13);
		ImportedModule();
		while (la.kind == 14) {
			Get();
			ImportedModule();
		}
		Expect(7);
	}

	void DeclSeq() {
		while (la.kind == 15 || la.kind == 16 || la.kind == 17) {
			if (la.kind == 15) {
				Get();
				while (la.kind == 1) {
					ConstDecl();
					Expect(7);
				}
			} else if (la.kind == 16) {
				Get();
				while (la.kind == 1) {
					TypeDecl();
					Expect(7);
				}
			} else {
				Get();
				while (la.kind == 1) {
					VarDecl();
					Expect(7);
				}
			}
		}
		while (la.kind == 20) {
			if (la.kind == 20) {
				ProcDecl();
				Expect(7);
			} else {
				ForwardDecl();
				Expect(7);
			}
		}
	}

	void StatementSeq() {
		Statement();
		while (la.kind == 7) {
			Get();
			Statement();
		}
	}

	void ImportedModule() {
		String name = null; String originalName; 
		if (la.kind == 1) {
			Get();
			name = t.val; 
			Expect(12);
		}
		Expect(1);
		originalName = t.val; module.Import.Add(new Import(name, originalName)); 
	}

	void ConstDecl() {
		IdentDef();
		Expect(18);
		ConstExpr();
	}

	void TypeDecl() {
		IdentDef();
		Expect(18);
		Type();
	}

	void VarDecl() {
		IdentList();
		Expect(19);
		Type();
	}

	void ProcDecl() {
		Expect(20);
		if (la.kind == 26) {
			Receiver();
		}
		IdentDef();
		if (la.kind == 26) {
			FormalPars();
		}
		MethAttributes();
		if (la.kind == 7) {
			Get();
			DeclSeq();
			if (la.kind == 8) {
				Get();
				StatementSeq();
			}
			Expect(10);
			Expect(1);
		}
	}

	void ForwardDecl() {
		Expect(20);
		Expect(25);
		if (la.kind == 26) {
			Receiver();
		}
		IdentDef();
		if (la.kind == 26) {
			FormalPars();
		}
		MethAttributes();
	}

	void IdentDef() {
		Expect(1);
		if (la.kind == 54 || la.kind == 66) {
			if (la.kind == 66) {
				Get();
			} else {
				Get();
			}
		}
	}

	void ConstExpr() {
		Expr();
	}

	void Type() {
		if (la.kind == 1) {
			Qualident();
		} else if (la.kind == 30) {
			Get();
			if (StartOf(1)) {
				ConstExpr();
				while (la.kind == 14) {
					Get();
					ConstExpr();
				}
			}
			Expect(31);
			Type();
		} else if (StartOf(2)) {
			if (la.kind == 22 || la.kind == 24 || la.kind == 32) {
				if (la.kind == 22) {
					Get();
				} else if (la.kind == 24) {
					Get();
				} else {
					Get();
				}
			}
			Expect(33);
			if (la.kind == 26) {
				Get();
				Qualident();
				Expect(27);
			}
			FieldList();
			while (la.kind == 7) {
				Get();
				FieldList();
			}
			Expect(10);
		} else if (la.kind == 34) {
			Get();
			Expect(35);
			Type();
		} else if (la.kind == 20) {
			Get();
			if (la.kind == 26) {
				FormalPars();
			}
		} else SynErr(76);
	}

	void IdentList() {
		IdentDef();
		while (la.kind == 14) {
			Get();
			IdentDef();
		}
	}

	void Receiver() {
		Expect(26);
		if (la.kind == 17 || la.kind == 28) {
			if (la.kind == 17) {
				Get();
			} else {
				Get();
			}
		}
		Expect(1);
		Expect(19);
		Expect(1);
		Expect(27);
	}

	void FormalPars() {
		Expect(26);
		if (StartOf(3)) {
			FPSection();
			while (la.kind == 7) {
				Get();
				FPSection();
			}
		}
		Expect(27);
		if (la.kind == 19) {
			Get();
			Type();
		}
	}

	void MethAttributes() {
		if (la.kind == 14) {
			Get();
			Expect(21);
		}
		if (la.kind == 14) {
			Get();
			if (la.kind == 22) {
				Get();
			} else if (la.kind == 23) {
				Get();
			} else if (la.kind == 24) {
				Get();
			} else SynErr(77);
		}
	}

	void FPSection() {
		if (la.kind == 17 || la.kind == 28 || la.kind == 29) {
			if (la.kind == 17) {
				Get();
			} else if (la.kind == 28) {
				Get();
			} else {
				Get();
			}
		}
		Expect(1);
		while (la.kind == 14) {
			Get();
			Expect(1);
		}
		Expect(19);
		Type();
	}

	void Qualident() {
		if (la.kind == 1) {
			Get();
			Expect(11);
		}
		Expect(1);
	}

	void FieldList() {
		if (la.kind == 1) {
			IdentList();
			Expect(19);
			Type();
		}
	}

	void Statement() {
		if (StartOf(4)) {
			if (la.kind == 1) {
				Designator();
				Expect(12);
				Expr();
			} else if (la.kind == 1) {
				Designator();
				if (la.kind == 26) {
					Get();
					if (StartOf(1)) {
						ExprList();
					}
					Expect(27);
				}
			} else if (la.kind == 36) {
				Get();
				Expr();
				Expect(37);
				StatementSeq();
				while (la.kind == 38) {
					Get();
					Expr();
					Expect(37);
					StatementSeq();
				}
				if (la.kind == 39) {
					Get();
					StatementSeq();
				}
				Expect(10);
			} else if (la.kind == 40) {
				Get();
				Expr();
				Expect(31);
				Case();
				while (la.kind == 41) {
					Get();
					Case();
				}
				if (la.kind == 39) {
					Get();
					StatementSeq();
				}
				Expect(10);
			} else if (la.kind == 42) {
				Get();
				Expr();
				Expect(43);
				StatementSeq();
				Expect(10);
			} else if (la.kind == 44) {
				Get();
				StatementSeq();
				Expect(45);
				Expr();
			} else if (la.kind == 46) {
				Get();
				Expect(1);
				Expect(12);
				Expr();
				Expect(35);
				Expr();
				if (la.kind == 47) {
					Get();
					ConstExpr();
				}
				Expect(43);
				StatementSeq();
				Expect(10);
			} else if (la.kind == 48) {
				Get();
				StatementSeq();
				Expect(10);
			} else if (la.kind == 49) {
				Get();
				if (la.kind == 1) {
					Guard();
					Expect(43);
					StatementSeq();
				}
				while (la.kind == 41) {
					Get();
					if (la.kind == 1) {
						Guard();
						Expect(43);
						StatementSeq();
					}
				}
				if (la.kind == 39) {
					Get();
					StatementSeq();
				}
				Expect(10);
			} else if (la.kind == 50) {
				Get();
			} else {
				Get();
				if (StartOf(1)) {
					Expr();
				}
			}
		}
	}

	void Designator() {
		Qualident();
		while (StartOf(5)) {
			if (la.kind == 11) {
				Get();
				Expect(1);
			} else if (la.kind == 71) {
				Get();
				ExprList();
				Expect(72);
			} else if (la.kind == 25) {
				Get();
			} else if (la.kind == 26) {
				Get();
				Qualident();
				Expect(27);
			} else {
				Get();
				if (StartOf(1)) {
					ExprList();
				}
				Expect(27);
			}
		}
		if (la.kind == 73) {
			Get();
		}
	}

	void Expr() {
		SimpleExpr();
		if (StartOf(6)) {
			Relation();
			SimpleExpr();
		}
	}

	void ExprList() {
		Expr();
		while (la.kind == 14) {
			Get();
			Expr();
		}
	}

	void Case() {
		if (StartOf(1)) {
			CaseLabels();
			while (la.kind == 14) {
				Get();
				CaseLabels();
			}
			Expect(19);
			StatementSeq();
		}
	}

	void Guard() {
		Qualident();
		Expect(19);
		Qualident();
	}

	void CaseLabels() {
		ConstExpr();
		if (la.kind == 52) {
			Get();
			ConstExpr();
		}
	}

	void SimpleExpr() {
		if (la.kind == 53 || la.kind == 54) {
			if (la.kind == 53) {
				Get();
			} else {
				Get();
			}
		}
		Term();
		while (la.kind == 53 || la.kind == 54 || la.kind == 65) {
			AddOp();
			Term();
		}
	}

	void Relation() {
		switch (la.kind) {
		case 18: {
			Get();
			break;
		}
		case 59: {
			Get();
			break;
		}
		case 60: {
			Get();
			break;
		}
		case 61: {
			Get();
			break;
		}
		case 62: {
			Get();
			break;
		}
		case 63: {
			Get();
			break;
		}
		case 28: {
			Get();
			break;
		}
		case 64: {
			Get();
			break;
		}
		default: SynErr(78); break;
		}
	}

	void Term() {
		Factor();
		while (StartOf(7)) {
			MulOp();
			Factor();
		}
	}

	void AddOp() {
		if (la.kind == 53) {
			Get();
		} else if (la.kind == 54) {
			Get();
		} else if (la.kind == 65) {
			Get();
		} else SynErr(79);
	}

	void Factor() {
		switch (la.kind) {
		case 1: {
			Designator();
			break;
		}
		case 2: case 3: {
			number();
			break;
		}
		case 4: {
			Get();
			break;
		}
		case 5: {
			Get();
			break;
		}
		case 55: {
			Get();
			break;
		}
		case 57: {
			Set();
			break;
		}
		case 26: {
			Get();
			Expr();
			Expect(27);
			break;
		}
		case 56: {
			Get();
			Factor();
			break;
		}
		default: SynErr(80); break;
		}
	}

	void MulOp() {
		if (la.kind == 66) {
			Get();
		} else if (la.kind == 67) {
			Get();
		} else if (la.kind == 68) {
			Get();
		} else if (la.kind == 69) {
			Get();
		} else if (la.kind == 70) {
			Get();
		} else SynErr(81);
	}

	void Set() {
		Expect(57);
		if (StartOf(1)) {
			Element();
			while (la.kind == 14) {
				Get();
				Element();
			}
		}
		Expect(58);
	}

	void Element() {
		Expr();
		if (la.kind == 52) {
			Get();
			Expr();
		}
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Oberon();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _T,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _T,_x,_T,_x, _T,_x,_T,_x, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_x, _x,_x,_x,_x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "ident expected"; break;
			case 2: s = "integer expected"; break;
			case 3: s = "real expected"; break;
			case 4: s = "character expected"; break;
			case 5: s = "string expected"; break;
			case 6: s = "\"MODULE\" expected"; break;
			case 7: s = "\";\" expected"; break;
			case 8: s = "\"BEGIN\" expected"; break;
			case 9: s = "\"CLOSE\" expected"; break;
			case 10: s = "\"END\" expected"; break;
			case 11: s = "\".\" expected"; break;
			case 12: s = "\":=\" expected"; break;
			case 13: s = "\"IMPORT\" expected"; break;
			case 14: s = "\",\" expected"; break;
			case 15: s = "\"CONST\" expected"; break;
			case 16: s = "\"TYPE\" expected"; break;
			case 17: s = "\"VAR\" expected"; break;
			case 18: s = "\"=\" expected"; break;
			case 19: s = "\":\" expected"; break;
			case 20: s = "\"PROCEDURE\" expected"; break;
			case 21: s = "\"NEW\" expected"; break;
			case 22: s = "\"ABSTRACT\" expected"; break;
			case 23: s = "\"EMPTY\" expected"; break;
			case 24: s = "\"EXTENSIBLE\" expected"; break;
			case 25: s = "\"^\" expected"; break;
			case 26: s = "\"(\" expected"; break;
			case 27: s = "\")\" expected"; break;
			case 28: s = "\"IN\" expected"; break;
			case 29: s = "\"OUT\" expected"; break;
			case 30: s = "\"ARRAY\" expected"; break;
			case 31: s = "\"OF\" expected"; break;
			case 32: s = "\"LIMITED\" expected"; break;
			case 33: s = "\"RECORD\" expected"; break;
			case 34: s = "\"POINTER\" expected"; break;
			case 35: s = "\"TO\" expected"; break;
			case 36: s = "\"IF\" expected"; break;
			case 37: s = "\"THEN\" expected"; break;
			case 38: s = "\"ELSIF\" expected"; break;
			case 39: s = "\"ELSE\" expected"; break;
			case 40: s = "\"CASE\" expected"; break;
			case 41: s = "\"|\" expected"; break;
			case 42: s = "\"WHILE\" expected"; break;
			case 43: s = "\"DO\" expected"; break;
			case 44: s = "\"REPEAT\" expected"; break;
			case 45: s = "\"UNTIL\" expected"; break;
			case 46: s = "\"FOR\" expected"; break;
			case 47: s = "\"BY\" expected"; break;
			case 48: s = "\"LOOP\" expected"; break;
			case 49: s = "\"WITH\" expected"; break;
			case 50: s = "\"EXIT\" expected"; break;
			case 51: s = "\"RETURN\" expected"; break;
			case 52: s = "\"..\" expected"; break;
			case 53: s = "\"+\" expected"; break;
			case 54: s = "\"-\" expected"; break;
			case 55: s = "\"NIL\" expected"; break;
			case 56: s = "\"~\" expected"; break;
			case 57: s = "\"{\" expected"; break;
			case 58: s = "\"}\" expected"; break;
			case 59: s = "\"#\" expected"; break;
			case 60: s = "\"<\" expected"; break;
			case 61: s = "\"<=\" expected"; break;
			case 62: s = "\">\" expected"; break;
			case 63: s = "\">=\" expected"; break;
			case 64: s = "\"IS\" expected"; break;
			case 65: s = "\"OR\" expected"; break;
			case 66: s = "\"*\" expected"; break;
			case 67: s = "\"/\" expected"; break;
			case 68: s = "\"DIV\" expected"; break;
			case 69: s = "\"MOD\" expected"; break;
			case 70: s = "\"&\" expected"; break;
			case 71: s = "\"[\" expected"; break;
			case 72: s = "\"]\" expected"; break;
			case 73: s = "\"$\" expected"; break;
			case 74: s = "??? expected"; break;
			case 75: s = "invalid number"; break;
			case 76: s = "invalid Type"; break;
			case 77: s = "invalid MethAttributes"; break;
			case 78: s = "invalid Relation"; break;
			case 79: s = "invalid AddOp"; break;
			case 80: s = "invalid Factor"; break;
			case 81: s = "invalid MulOp"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
