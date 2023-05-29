
using System;



public class Parser {
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _integer = 2;
	public const int _real = 3;
	public const int _CharConstant = 4;
	public const int _string = 5;
	public const int maxT = 64;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;



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
		module();
	}

	void module() {
		Expect(63);
		Expect(1);
		Expect(16);
		if (la.kind == 62) {
			ImportList();
		}
		DeclarationSequence();
		if (la.kind == 58) {
			Get();
			StatementSequence();
		}
		Expect(15);
		Expect(1);
		Expect(7);
	}

	void number() {
		if (la.kind == 2) {
			Get();
		} else if (la.kind == 3) {
			Get();
		} else SynErr(65);
	}

	void identdef() {
		Expect(1);
		if (la.kind == 6) {
			Get();
		}
	}

	void qualident() {
		if (la.kind == 1) {
			Get();
			Expect(7);
		}
		Expect(1);
	}

	void ConstantDeclaration() {
		identdef();
		Expect(8);
		ConstExpression();
	}

	void ConstExpression() {
		expression();
	}

	void expression() {
		SimpleExpression();
		if (StartOf(1)) {
			relation();
			SimpleExpression();
		}
	}

	void TypeDeclaration() {
		identdef();
		Expect(8);
		type();
	}

	void type() {
		if (la.kind == 1) {
			qualident();
		} else if (la.kind == 9) {
			ArrayType();
		} else if (la.kind == 12) {
			RecordType();
		} else if (la.kind == 18) {
			PointerType();
		} else if (la.kind == 20) {
			ProcedureType();
		} else SynErr(66);
	}

	void ArrayType() {
		Expect(9);
		length();
		while (la.kind == 10) {
			Get();
			length();
		}
		Expect(11);
		type();
	}

	void RecordType() {
		Expect(12);
		if (la.kind == 13) {
			Get();
			BaseType();
			Expect(14);
		}
		FieldListSequence();
		Expect(15);
	}

	void PointerType() {
		Expect(18);
		Expect(19);
		type();
	}

	void ProcedureType() {
		Expect(20);
		if (la.kind == 13) {
			FormalParameters();
		}
	}

	void length() {
		ConstExpression();
	}

	void BaseType() {
		qualident();
	}

	void FieldListSequence() {
		FieldList();
		while (la.kind == 16) {
			Get();
			FieldList();
		}
	}

	void FieldList() {
		if (la.kind == 1) {
			IdentList();
			Expect(17);
			type();
		}
	}

	void IdentList() {
		identdef();
		while (la.kind == 10) {
			Get();
			identdef();
		}
	}

	void FormalParameters() {
		Expect(13);
		if (la.kind == 1 || la.kind == 61) {
			FPSection();
			while (la.kind == 16) {
				Get();
				FPSection();
			}
		}
		Expect(14);
		if (la.kind == 17) {
			Get();
			qualident();
		}
	}

	void VariableDeclaration() {
		IdentList();
		Expect(17);
		type();
	}

	void designator() {
		qualident();
		while (StartOf(2)) {
			if (la.kind == 7) {
				Get();
				Expect(1);
			} else if (la.kind == 21) {
				Get();
				ExpList();
				Expect(22);
			} else if (la.kind == 13) {
				Get();
				qualident();
				Expect(14);
			} else {
				Get();
			}
		}
	}

	void ExpList() {
		expression();
		while (la.kind == 10) {
			Get();
			expression();
		}
	}

	void SimpleExpression() {
		if (la.kind == 31 || la.kind == 32) {
			if (la.kind == 31) {
				Get();
			} else {
				Get();
			}
		}
		term();
		while (la.kind == 31 || la.kind == 32 || la.kind == 33) {
			AddOperator();
			term();
		}
	}

	void relation() {
		switch (la.kind) {
		case 8: {
			Get();
			break;
		}
		case 24: {
			Get();
			break;
		}
		case 25: {
			Get();
			break;
		}
		case 26: {
			Get();
			break;
		}
		case 27: {
			Get();
			break;
		}
		case 28: {
			Get();
			break;
		}
		case 29: {
			Get();
			break;
		}
		case 30: {
			Get();
			break;
		}
		default: SynErr(67); break;
		}
	}

	void term() {
		factor();
		while (StartOf(3)) {
			MulOperator();
			factor();
		}
	}

	void AddOperator() {
		if (la.kind == 31) {
			Get();
		} else if (la.kind == 32) {
			Get();
		} else if (la.kind == 33) {
			Get();
		} else SynErr(68);
	}

	void factor() {
		switch (la.kind) {
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
		case 38: {
			Get();
			break;
		}
		case 40: {
			Set();
			break;
		}
		case 1: {
			designator();
			if (la.kind == 13) {
				ActualParameters();
			}
			break;
		}
		case 13: {
			Get();
			expression();
			Expect(14);
			break;
		}
		case 39: {
			Get();
			factor();
			break;
		}
		default: SynErr(69); break;
		}
	}

	void MulOperator() {
		if (la.kind == 6) {
			Get();
		} else if (la.kind == 34) {
			Get();
		} else if (la.kind == 35) {
			Get();
		} else if (la.kind == 36) {
			Get();
		} else if (la.kind == 37) {
			Get();
		} else SynErr(70);
	}

	void Set() {
		Expect(40);
		if (StartOf(4)) {
			element();
			while (la.kind == 10) {
				Get();
				element();
			}
		}
		Expect(41);
	}

	void ActualParameters() {
		Expect(13);
		if (StartOf(4)) {
			ExpList();
		}
		Expect(14);
	}

	void element() {
		expression();
		if (la.kind == 42) {
			Get();
			expression();
		}
	}

	void statement() {
		if (StartOf(5)) {
			if (la.kind == 1) {
				assignment();
			} else if (la.kind == 1) {
				ProcedureCall();
			} else if (la.kind == 46) {
				IfStatement();
			} else if (la.kind == 50) {
				CaseStatement();
			} else if (la.kind == 52) {
				WhileStatement();
			} else if (la.kind == 54) {
				RepeatStatement();
			} else if (la.kind == 56) {
				LoopStatement();
			} else if (la.kind == 57) {
				WithStatement();
			} else if (la.kind == 43) {
				Get();
			} else {
				Get();
				if (StartOf(4)) {
					expression();
				}
			}
		}
	}

	void assignment() {
		designator();
		Expect(45);
		expression();
	}

	void ProcedureCall() {
		designator();
		if (la.kind == 13) {
			ActualParameters();
		}
	}

	void IfStatement() {
		Expect(46);
		expression();
		Expect(47);
		StatementSequence();
		while (la.kind == 48) {
			Get();
			expression();
			Expect(47);
			StatementSequence();
		}
		if (la.kind == 49) {
			Get();
			StatementSequence();
		}
		Expect(15);
	}

	void CaseStatement() {
		Expect(50);
		expression();
		Expect(11);
		Case();
		while (la.kind == 51) {
			Get();
			Case();
		}
		if (la.kind == 49) {
			Get();
			StatementSequence();
		}
		Expect(15);
	}

	void WhileStatement() {
		Expect(52);
		expression();
		Expect(53);
		StatementSequence();
		Expect(15);
	}

	void RepeatStatement() {
		Expect(54);
		StatementSequence();
		Expect(55);
		expression();
	}

	void LoopStatement() {
		Expect(56);
		StatementSequence();
		Expect(15);
	}

	void WithStatement() {
		Expect(57);
		qualident();
		Expect(17);
		qualident();
		Expect(53);
		StatementSequence();
		Expect(15);
	}

	void StatementSequence() {
		statement();
		while (la.kind == 16) {
			Get();
			statement();
		}
	}

	void Case() {
		if (StartOf(4)) {
			CaseLabelList();
			Expect(17);
			StatementSequence();
		}
	}

	void CaseLabelList() {
		CaseLabels();
		while (la.kind == 10) {
			Get();
			CaseLabels();
		}
	}

	void CaseLabels() {
		ConstExpression();
		if (la.kind == 42) {
			Get();
			ConstExpression();
		}
	}

	void ProcedureDeclaration() {
		ProcedureHeading();
		Expect(16);
		ProcedureBody();
		Expect(1);
	}

	void ProcedureHeading() {
		Expect(20);
		if (la.kind == 6) {
			Get();
		}
		identdef();
		if (la.kind == 13) {
			FormalParameters();
		}
	}

	void ProcedureBody() {
		DeclarationSequence();
		if (la.kind == 58) {
			Get();
			StatementSequence();
		}
		Expect(15);
	}

	void DeclarationSequence() {
		while (la.kind == 59 || la.kind == 60 || la.kind == 61) {
			if (la.kind == 59) {
				Get();
				while (la.kind == 1) {
					ConstantDeclaration();
					Expect(16);
				}
			} else if (la.kind == 60) {
				Get();
				while (la.kind == 1) {
					TypeDeclaration();
					Expect(16);
				}
			} else {
				Get();
				while (la.kind == 1) {
					VariableDeclaration();
					Expect(16);
				}
			}
		}
		while (la.kind == 20) {
			if (la.kind == 20) {
				ProcedureDeclaration();
				Expect(16);
			} else {
				ForwardDeclaration();
				Expect(16);
			}
		}
	}

	void ForwardDeclaration() {
		Expect(20);
		Expect(23);
		Expect(1);
		if (la.kind == 6) {
			Get();
		}
		if (la.kind == 13) {
			FormalParameters();
		}
	}

	void FPSection() {
		if (la.kind == 61) {
			Get();
		}
		Expect(1);
		while (la.kind == 10) {
			Get();
			Expect(1);
		}
		Expect(17);
		FormalType();
	}

	void FormalType() {
		while (la.kind == 9) {
			Get();
			Expect(11);
		}
		if (la.kind == 1) {
			qualident();
		} else if (la.kind == 20) {
			ProcedureType();
		} else SynErr(71);
	}

	void ImportList() {
		Expect(62);
		import();
		while (la.kind == 10) {
			Get();
			import();
		}
		Expect(16);
	}

	void import() {
		Expect(1);
		if (la.kind == 45) {
			Get();
			Expect(1);
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
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_T,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_x,_x,_x, _x,_x,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_x,_T,_x, _x,_x,_T,_x, _T,_x,_T,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x}

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
			case 6: s = "\"*\" expected"; break;
			case 7: s = "\".\" expected"; break;
			case 8: s = "\"=\" expected"; break;
			case 9: s = "\"ARRAY\" expected"; break;
			case 10: s = "\",\" expected"; break;
			case 11: s = "\"OF\" expected"; break;
			case 12: s = "\"RECORD\" expected"; break;
			case 13: s = "\"(\" expected"; break;
			case 14: s = "\")\" expected"; break;
			case 15: s = "\"END\" expected"; break;
			case 16: s = "\";\" expected"; break;
			case 17: s = "\":\" expected"; break;
			case 18: s = "\"POINTER\" expected"; break;
			case 19: s = "\"TO\" expected"; break;
			case 20: s = "\"PROCEDURE\" expected"; break;
			case 21: s = "\"[\" expected"; break;
			case 22: s = "\"]\" expected"; break;
			case 23: s = "\"^\" expected"; break;
			case 24: s = "\"#\" expected"; break;
			case 25: s = "\"<\" expected"; break;
			case 26: s = "\"<=\" expected"; break;
			case 27: s = "\">\" expected"; break;
			case 28: s = "\">=\" expected"; break;
			case 29: s = "\"IN\" expected"; break;
			case 30: s = "\"IS\" expected"; break;
			case 31: s = "\"+\" expected"; break;
			case 32: s = "\"-\" expected"; break;
			case 33: s = "\"OR\" expected"; break;
			case 34: s = "\"/\" expected"; break;
			case 35: s = "\"DIV\" expected"; break;
			case 36: s = "\"MOD\" expected"; break;
			case 37: s = "\"&\" expected"; break;
			case 38: s = "\"NIL\" expected"; break;
			case 39: s = "\"~\" expected"; break;
			case 40: s = "\"{\" expected"; break;
			case 41: s = "\"}\" expected"; break;
			case 42: s = "\"..\" expected"; break;
			case 43: s = "\"EXIT\" expected"; break;
			case 44: s = "\"RETURN\" expected"; break;
			case 45: s = "\":=\" expected"; break;
			case 46: s = "\"IF\" expected"; break;
			case 47: s = "\"THEN\" expected"; break;
			case 48: s = "\"ELSIF\" expected"; break;
			case 49: s = "\"ELSE\" expected"; break;
			case 50: s = "\"CASE\" expected"; break;
			case 51: s = "\"|\" expected"; break;
			case 52: s = "\"WHILE\" expected"; break;
			case 53: s = "\"DO\" expected"; break;
			case 54: s = "\"REPEAT\" expected"; break;
			case 55: s = "\"UNTIL\" expected"; break;
			case 56: s = "\"LOOP\" expected"; break;
			case 57: s = "\"WITH\" expected"; break;
			case 58: s = "\"BEGIN\" expected"; break;
			case 59: s = "\"CONST\" expected"; break;
			case 60: s = "\"TYPE\" expected"; break;
			case 61: s = "\"VAR\" expected"; break;
			case 62: s = "\"IMPORT\" expected"; break;
			case 63: s = "\"MODULE\" expected"; break;
			case 64: s = "??? expected"; break;
			case 65: s = "invalid number"; break;
			case 66: s = "invalid type"; break;
			case 67: s = "invalid relation"; break;
			case 68: s = "invalid AddOperator"; break;
			case 69: s = "invalid factor"; break;
			case 70: s = "invalid MulOperator"; break;
			case 71: s = "invalid FormalType"; break;

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
