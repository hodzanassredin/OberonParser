
using System;



public class Parser {
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _integer = 2;
	public const int _real = 3;
	public const int _CharConstant = 4;
	public const int _string = 5;
	public const int maxT = 70;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

(* A grammar for Active Oberon *)

IMPORT Strings;
VAR alternatives:ARRAY 1024 OF CHAR;



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

	
	void ActiveOberon() {
		Module();
	}

	void Module() {
		Expect(6);
		Expect(1);
		Expect(7);
		if (la.kind == 9) {
			ImportList();
		}
		while (la.kind == 12) {
			Definition();
		}
		while (StartOf(1)) {
			DeclSeq();
		}
		Body();
		Expect(1);
		Expect(8);
	}

	void ImportList() {
		Expect(9);
		Expect(1);
		if (la.kind == 10) {
			Get();
			Expect(1);
		}
		while (la.kind == 11) {
			Get();
			Expect(1);
			if (la.kind == 10) {
				Get();
				Expect(1);
			}
		}
		Expect(7);
	}

	void Definition() {
		Expect(12);
		Expect(1);
		if (la.kind == 13) {
			Get();
			Qualident();
		}
		while (la.kind == 14) {
			Get();
			Expect(1);
			if (la.kind == 25) {
				FormalPars();
			}
			Expect(7);
		}
		Expect(15);
		Expect(1);
	}

	void DeclSeq() {
		if (la.kind == 16) {
			Get();
			while (la.kind == 1) {
				ConstDecl();
				Expect(7);
			}
		} else if (la.kind == 17) {
			Get();
			while (la.kind == 1) {
				TypeDecl();
				Expect(7);
			}
		} else if (la.kind == 18) {
			Get();
			while (la.kind == 1) {
				VarDecl();
				Expect(7);
			}
		} else if (StartOf(1)) {
			while (la.kind == 14) {
				ProcDecl();
				Expect(7);
			}
		} else SynErr(71);
	}

	void Body() {
		if (la.kind == 34) {
			StatBlock();
		} else if (la.kind == 15) {
			Get();
		} else SynErr(72);
	}

	void Qualident() {
		if (la.kind == 1) {
			Get();
			Expect(8);
		}
		Expect(1);
	}

	void FormalPars() {
		Expect(25);
		if (la.kind == 1 || la.kind == 18) {
			FPSection();
			while (la.kind == 7) {
				Get();
				FPSection();
			}
		}
		Expect(26);
		if (la.kind == 20) {
			Get();
			Qualident();
		}
	}

	void ConstDecl() {
		IdentDef();
		Expect(19);
		ConstExpr();
	}

	void TypeDecl() {
		IdentDef();
		Expect(19);
		Type();
	}

	void VarDecl() {
		IdentList();
		Expect(20);
		Type();
	}

	void ProcDecl() {
		Expect(14);
		ProcHead();
		Expect(7);
		while (StartOf(1)) {
			DeclSeq();
		}
		Body();
		Expect(1);
	}

	void IdentDef() {
		Expect(1);
		if (la.kind == 21 || la.kind == 56) {
			if (la.kind == 21) {
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
		switch (la.kind) {
		case 1: {
			Qualident();
			break;
		}
		case 27: {
			Get();
			if (la.kind == 23) {
				SysFlag();
			}
			if (StartOf(2)) {
				ConstExpr();
				while (la.kind == 11) {
					Get();
					ConstExpr();
				}
			}
			Expect(28);
			Type();
			break;
		}
		case 29: {
			Get();
			if (la.kind == 23) {
				SysFlag();
			}
			if (la.kind == 25) {
				Get();
				Qualident();
				Expect(26);
			}
			if (la.kind == 1 || la.kind == 7 || la.kind == 15) {
				FieldList();
			}
			Expect(15);
			break;
		}
		case 30: {
			Get();
			if (la.kind == 23) {
				SysFlag();
			}
			Expect(31);
			Type();
			break;
		}
		case 32: {
			Get();
			if (StartOf(3)) {
				if (la.kind == 23) {
					SysFlag();
				}
				if (la.kind == 25) {
					Get();
					Qualident();
					Expect(26);
				}
				if (la.kind == 33) {
					Get();
					Qualident();
				}
				while (StartOf(1)) {
					DeclSeq();
				}
				Body();
			}
			break;
		}
		case 14: {
			Get();
			if (la.kind == 23) {
				SysFlag();
			}
			if (la.kind == 25) {
				FormalPars();
			}
			break;
		}
		default: SynErr(73); break;
		}
	}

	void IdentList() {
		IdentDef();
		while (la.kind == 11) {
			Get();
			IdentDef();
		}
	}

	void ProcHead() {
		if (la.kind == 23) {
			SysFlag();
		}
		if (la.kind == 21 || la.kind == 22) {
			if (la.kind == 21) {
				Get();
			} else {
				Get();
			}
		}
		IdentDef();
		if (la.kind == 25) {
			FormalPars();
		}
	}

	void SysFlag() {
		Expect(23);
		Expect(1);
		Expect(24);
	}

	void FPSection() {
		if (la.kind == 18) {
			Get();
		}
		Expect(1);
		while (la.kind == 11) {
			Get();
			Expect(1);
		}
		Expect(20);
		Type();
	}

	void FieldList() {
		FieldDecl();
		while (la.kind == 7) {
			Get();
			FieldDecl();
		}
	}

	void FieldDecl() {
		if (la.kind == 1) {
			IdentList();
			Expect(20);
			Type();
		}
	}

	void StatBlock() {
		Expect(34);
		if (la.kind == 35) {
			Get();
			IdentList();
			Expect(36);
		}
		if (StartOf(4)) {
			StatSeq();
		}
		Expect(15);
	}

	void StatSeq() {
		Statement();
		while (la.kind == 7) {
			Get();
			Statement();
		}
	}

	void Statement() {
		if (StartOf(5)) {
			if (la.kind == 1) {
				Designator();
				Expect(10);
				Expr();
			} else if (la.kind == 1) {
				Designator();
				if (la.kind == 25) {
					Get();
					ExprList();
					Expect(26);
				}
			} else if (la.kind == 37) {
				Get();
				Expr();
				Expect(38);
				StatSeq();
				while (la.kind == 39) {
					Get();
					Expr();
					Expect(38);
					StatSeq();
				}
				if (la.kind == 40) {
					Get();
					StatSeq();
				}
				Expect(15);
			} else if (la.kind == 41) {
				Get();
				Expr();
				Expect(38);
				Case();
				while (la.kind == 42) {
					Get();
					Case();
				}
				if (la.kind == 40) {
					Get();
					StatSeq();
				}
				Expect(15);
			} else if (la.kind == 43) {
				Get();
				Expr();
				Expect(44);
				StatSeq();
				Expect(15);
			} else if (la.kind == 45) {
				Get();
				StatSeq();
				Expect(46);
				Expr();
			} else if (la.kind == 47) {
				Get();
				Expect(1);
				Expect(10);
				Expr();
				Expect(31);
				Expr();
				if (la.kind == 48) {
					Get();
					ConstExpr();
				}
				Expect(44);
				StatSeq();
				Expect(15);
			} else if (la.kind == 49) {
				Get();
				StatSeq();
				Expect(15);
			} else if (la.kind == 50) {
				Get();
				Qualident();
				Expect(20);
				Qualident();
				Expect(44);
				StatSeq();
				Expect(15);
			} else if (la.kind == 51) {
				Get();
			} else if (la.kind == 52) {
				Get();
				if (StartOf(2)) {
					Expr();
				}
			} else if (la.kind == 53) {
				Get();
				Expect(25);
				Expr();
				Expect(26);
			} else {
				StatBlock();
			}
		}
	}

	void Designator() {
		Qualident();
		while (la.kind == 8 || la.kind == 23 || la.kind == 25) {
			if (la.kind == 8) {
				Get();
				Expect(1);
			} else if (la.kind == 23) {
				Get();
				ExprList();
				Expect(24);
			} else {
				Get();
				Qualident();
				Expect(26);
			}
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
		while (la.kind == 11) {
			Get();
			Expr();
		}
	}

	void Case() {
		if (StartOf(2)) {
			CaseLabels();
			while (la.kind == 11) {
				Get();
				CaseLabels();
			}
			Expect(20);
			StatSeq();
		}
	}

	void CaseLabels() {
		ConstExpr();
		if (la.kind == 54) {
			Get();
			ConstExpr();
		}
	}

	void SimpleExpr() {
		Term();
		while (StartOf(7)) {
			MulOp();
			Term();
		}
	}

	void Relation() {
		switch (la.kind) {
		case 19: {
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
		case 64: {
			Get();
			break;
		}
		case 65: {
			Get();
			break;
		}
		default: SynErr(74); break;
		}
	}

	void Term() {
		if (la.kind == 55 || la.kind == 56) {
			if (la.kind == 55) {
				Get();
			} else {
				Get();
			}
		}
		Factor();
		while (la.kind == 55 || la.kind == 56 || la.kind == 69) {
			AddOp();
			Factor();
		}
	}

	void MulOp() {
		if (la.kind == 21) {
			Get();
		} else if (la.kind == 66) {
			Get();
		} else if (la.kind == 67) {
			Get();
		} else if (la.kind == 68) {
			Get();
		} else if (la.kind == 22) {
			Get();
		} else SynErr(75);
	}

	void Factor() {
		switch (la.kind) {
		case 1: {
			Designator();
			if (la.kind == 25) {
				Get();
				ExprList();
				Expect(26);
			}
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
		case 57: {
			Get();
			break;
		}
		case 35: {
			Set();
			break;
		}
		case 25: {
			Get();
			Expr();
			Expect(26);
			break;
		}
		case 58: {
			Get();
			Factor();
			break;
		}
		default: SynErr(76); break;
		}
	}

	void AddOp() {
		if (la.kind == 55) {
			Get();
		} else if (la.kind == 56) {
			Get();
		} else if (la.kind == 69) {
			Get();
		} else SynErr(77);
	}

	void number() {
		if (la.kind == 3) {
			Get();
		} else if (la.kind == 2) {
			Get();
		} else SynErr(78);
	}

	void Set() {
		Expect(35);
		if (StartOf(2)) {
			Element();
			while (la.kind == 11) {
				Get();
				Element();
			}
		}
		Expect(36);
	}

	void Element() {
		Expr();
		if (la.kind == 54) {
			Get();
			Expr();
		}
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		ActiveOberon();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_T,_x, _x,_x,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_T,_x,_x, _x,_T,_x,_T, _x,_T,_x,_T, _x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_T,_x,_x, _x,_T,_x,_T, _x,_T,_x,_T, _x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_x,_x,_x}

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
			case 4: s = "CharConstant expected"; break;
			case 5: s = "string expected"; break;
			case 6: s = "\"MODULE\" expected"; break;
			case 7: s = "\";\" expected"; break;
			case 8: s = "\".\" expected"; break;
			case 9: s = "\"IMPORT\" expected"; break;
			case 10: s = "\":=\" expected"; break;
			case 11: s = "\",\" expected"; break;
			case 12: s = "\"DEFINITION\" expected"; break;
			case 13: s = "\"REFINES\" expected"; break;
			case 14: s = "\"PROCEDURE\" expected"; break;
			case 15: s = "\"END\" expected"; break;
			case 16: s = "\"CONST\" expected"; break;
			case 17: s = "\"TYPE\" expected"; break;
			case 18: s = "\"VAR\" expected"; break;
			case 19: s = "\"=\" expected"; break;
			case 20: s = "\":\" expected"; break;
			case 21: s = "\"*\" expected"; break;
			case 22: s = "\"&\" expected"; break;
			case 23: s = "\"[\" expected"; break;
			case 24: s = "\"]\" expected"; break;
			case 25: s = "\"(\" expected"; break;
			case 26: s = "\")\" expected"; break;
			case 27: s = "\"ARRAY\" expected"; break;
			case 28: s = "\"OF\" expected"; break;
			case 29: s = "\"RECORD\" expected"; break;
			case 30: s = "\"POINTER\" expected"; break;
			case 31: s = "\"TO\" expected"; break;
			case 32: s = "\"OBJECT\" expected"; break;
			case 33: s = "\"IMPLEMENTS\" expected"; break;
			case 34: s = "\"BEGIN\" expected"; break;
			case 35: s = "\"{\" expected"; break;
			case 36: s = "\"}\" expected"; break;
			case 37: s = "\"IF\" expected"; break;
			case 38: s = "\"THEN\" expected"; break;
			case 39: s = "\"ELSIF\" expected"; break;
			case 40: s = "\"ELSE\" expected"; break;
			case 41: s = "\"CASE\" expected"; break;
			case 42: s = "\"|\" expected"; break;
			case 43: s = "\"WHILE\" expected"; break;
			case 44: s = "\"DO\" expected"; break;
			case 45: s = "\"REPEAT\" expected"; break;
			case 46: s = "\"UNTIL\" expected"; break;
			case 47: s = "\"FOR\" expected"; break;
			case 48: s = "\"BY\" expected"; break;
			case 49: s = "\"LOOP\" expected"; break;
			case 50: s = "\"WITH\" expected"; break;
			case 51: s = "\"EXIT\" expected"; break;
			case 52: s = "\"RETURN\" expected"; break;
			case 53: s = "\"AWAIT\" expected"; break;
			case 54: s = "\"..\" expected"; break;
			case 55: s = "\"+\" expected"; break;
			case 56: s = "\"-\" expected"; break;
			case 57: s = "\"NIL\" expected"; break;
			case 58: s = "\"~\" expected"; break;
			case 59: s = "\"#\" expected"; break;
			case 60: s = "\"<\" expected"; break;
			case 61: s = "\"<=\" expected"; break;
			case 62: s = "\">\" expected"; break;
			case 63: s = "\">=\" expected"; break;
			case 64: s = "\"IN\" expected"; break;
			case 65: s = "\"IS\" expected"; break;
			case 66: s = "\"DIV\" expected"; break;
			case 67: s = "\"MOD\" expected"; break;
			case 68: s = "\"/\" expected"; break;
			case 69: s = "\"OR\" expected"; break;
			case 70: s = "??? expected"; break;
			case 71: s = "invalid DeclSeq"; break;
			case 72: s = "invalid Body"; break;
			case 73: s = "invalid Type"; break;
			case 74: s = "invalid Relation"; break;
			case 75: s = "invalid MulOp"; break;
			case 76: s = "invalid Factor"; break;
			case 77: s = "invalid AddOp"; break;
			case 78: s = "invalid number"; break;

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
