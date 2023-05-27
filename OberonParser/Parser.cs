
using Ast;
using System;



public class Parser {
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _number = 2;
	public const int maxT = 28;

	const bool _T = true;
	const bool _x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

	Proc curProc;  // current program unit (procedure or main program)



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

	
	void Taste() {
		String name; 
		Expect(3);
		Ident(out name);
		curProc = new Proc(name, null, this); 
		Expect(4);
		while (la.kind == 8 || la.kind == 9 || la.kind == 10) {
			if (la.kind == 8 || la.kind == 9) {
				VarDecl();
			} else {
				ProcDecl();
			}
		}
		Expect(5);
		foreach (Obj obj in curProc.locals) {
		if (obj is Proc) ((Proc)obj).Dump();
		} 
	}

	void Ident(out String name) {
		Expect(1);
		name = t.val; 
	}

	void VarDecl() {
		String name; Ast.Type type; 
		Typ(out type);
		Ident(out name);
		curProc.add(new Var(name, type)); 
		while (la.kind == 6) {
			Get();
			Ident(out name);
			curProc.add(new Var(name, type)); 
		}
		Expect(7);
	}

	void ProcDecl() {
		String name; 
		Expect(10);
		Ident(out name);
		Proc oldProc = curProc;
		curProc = new Proc(name, oldProc, this);
		oldProc.add(curProc); 
		Expect(11);
		Expect(12);
		Block(out curProc.block);
		curProc = oldProc; 
	}

	void Typ(out Ast.Type type) {
		type = Ast.Type.INT; 
		if (la.kind == 8) {
			Get();
		} else if (la.kind == 9) {
			Get();
			type = Ast.Type.BOOL; 
		} else SynErr(29);
	}

	void Block(out Block b) {
		Stat s; 
		Expect(4);
		b = new Block(); 
		while (StartOf(1)) {
			if (StartOf(2)) {
				Stat(out s);
				b.add(s); 
			} else {
				VarDecl();
			}
		}
		Expect(5);
	}

	void Stat(out Stat s) {
		String name; Expr e; Stat s2; Block b; 
		s = null; 
		switch (la.kind) {
		case 1: {
			Ident(out name);
			Obj obj = curProc.find(name); 
			if (la.kind == 13) {
				Get();
				Expr(out e);
				Expect(7);
				s = new Assignment(obj, e); 
			} else if (la.kind == 11) {
				Get();
				Expect(12);
				Expect(7);
				s = new Call(obj); 
			} else SynErr(30);
			break;
		}
		case 14: {
			Get();
			Expect(11);
			Expr(out e);
			Expect(12);
			Stat(out s);
			s = new If(e, s); 
			if (la.kind == 15) {
				Get();
				Stat(out s2);
				s = new IfElse(s, s2); 
			}
			break;
		}
		case 16: {
			Get();
			Expect(11);
			Expr(out e);
			Expect(12);
			Stat(out s);
			s = new While(e, s); 
			break;
		}
		case 17: {
			Get();
			Ident(out name);
			Expect(7);
			s = new Read(curProc.find(name)); 
			break;
		}
		case 18: {
			Get();
			Expr(out e);
			Expect(7);
			s = new Write(e); 
			break;
		}
		case 4: {
			Block(out b);
			s = b; 
			break;
		}
		default: SynErr(31); break;
		}
	}

	void Expr(out Expr e) {
		Operator op; Expr e2; 
		SimExpr(out e);
		if (la.kind == 25 || la.kind == 26 || la.kind == 27) {
			RelOp(out op);
			SimExpr(out e2);
			e = new BinExpr(e, op, e2); 
		}
	}

	void SimExpr(out Expr e) {
		Operator op; Expr e2; 
		Term(out e);
		while (la.kind == 19 || la.kind == 22) {
			AddOp(out op);
			Term(out e2);
			e = new BinExpr(e, op, e2); 
		}
	}

	void RelOp(out Operator op) {
		op = Operator.EQU; 
		if (la.kind == 25) {
			Get();
		} else if (la.kind == 26) {
			Get();
			op = Operator.LSS; 
		} else if (la.kind == 27) {
			Get();
			op = Operator.GTR; 
		} else SynErr(32);
	}

	void Term(out Expr e) {
		Operator op; Expr e2; 
		Factor(out e);
		while (la.kind == 23 || la.kind == 24) {
			MulOp(out op);
			Factor(out e2);
			e = new BinExpr(e, op, e2); 
		}
	}

	void AddOp(out Operator op) {
		op = Operator.ADD; 
		if (la.kind == 22) {
			Get();
		} else if (la.kind == 19) {
			Get();
			op = Operator.SUB; 
		} else SynErr(33);
	}

	void Factor(out Expr e) {
		String name; 
		e = null; 
		if (la.kind == 1) {
			Ident(out name);
			e = new Ident(curProc.find(name)); 
		} else if (la.kind == 2) {
			Get();
			e = new IntCon(Int32.Parse(t.val)); 
		} else if (la.kind == 19) {
			Get();
			Factor(out e);
			e = new UnaryExpr(Operator.SUB, e); 
		} else if (la.kind == 20) {
			Get();
			e = new BoolCon(true); 
		} else if (la.kind == 21) {
			Get();
			e = new BoolCon(false); 
		} else SynErr(34);
	}

	void MulOp(out Operator op) {
		op = Operator.MUL; 
		if (la.kind == 23) {
			Get();
		} else if (la.kind == 24) {
			Get();
			op = Operator.DIV; 
		} else SynErr(35);
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Taste();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_T,_x,_x, _T,_x,_x,_x, _T,_T,_x,_x, _x,_x,_T,_x, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x},
		{_x,_T,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x}

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
			case 2: s = "number expected"; break;
			case 3: s = "\"program\" expected"; break;
			case 4: s = "\"{\" expected"; break;
			case 5: s = "\"}\" expected"; break;
			case 6: s = "\",\" expected"; break;
			case 7: s = "\";\" expected"; break;
			case 8: s = "\"int\" expected"; break;
			case 9: s = "\"bool\" expected"; break;
			case 10: s = "\"void\" expected"; break;
			case 11: s = "\"(\" expected"; break;
			case 12: s = "\")\" expected"; break;
			case 13: s = "\"=\" expected"; break;
			case 14: s = "\"if\" expected"; break;
			case 15: s = "\"else\" expected"; break;
			case 16: s = "\"while\" expected"; break;
			case 17: s = "\"read\" expected"; break;
			case 18: s = "\"write\" expected"; break;
			case 19: s = "\"-\" expected"; break;
			case 20: s = "\"true\" expected"; break;
			case 21: s = "\"false\" expected"; break;
			case 22: s = "\"+\" expected"; break;
			case 23: s = "\"*\" expected"; break;
			case 24: s = "\"/\" expected"; break;
			case 25: s = "\"==\" expected"; break;
			case 26: s = "\"<\" expected"; break;
			case 27: s = "\">\" expected"; break;
			case 28: s = "??? expected"; break;
			case 29: s = "invalid Typ"; break;
			case 30: s = "invalid Stat"; break;
			case 31: s = "invalid Stat"; break;
			case 32: s = "invalid RelOp"; break;
			case 33: s = "invalid AddOp"; break;
			case 34: s = "invalid Factor"; break;
			case 35: s = "invalid MulOp"; break;

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
