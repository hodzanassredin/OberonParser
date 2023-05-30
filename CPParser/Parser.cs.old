using CPParser;



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

public AstBuilder builder = new AstBuilder(); 



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

	
	void CP() {
		Module();
	}

	void Module() {
		Expect(6);
		Ident(out builder.Module.Ident);
		Expect(7);
		if (la.kind == 13) {
			ImportList(out builder.Module.ImportList);
		}
		DeclSeq(out builder.Module.DeclSeq);
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

	void Ident(out CPParser.Ast.Ident o) {
		Expect(1);
		o = new CPParser.Ast.Ident{Name = t.val}; 
	}

	void number() {
		if (la.kind == 2) {
			Get();
		} else if (la.kind == 3) {
			Get();
		} else SynErr(75);
	}

	void ImportList(out CPParser.Ast.AstList o) {
		o = new CPParser.Ast.AstList(); 
		Expect(13);
		ImportedModule(o);
		while (la.kind == 14) {
			Get();
			ImportedModule(o);
		}
		Expect(7);
	}

	void DeclSeq(out CPParser.Ast.DeclSeq o) {
		o = new CPParser.Ast.DeclSeq(); 
		while (la.kind == 15 || la.kind == 16 || la.kind == 17) {
			if (la.kind == 15) {
				Get();
				var lst = new CPParser.Ast.IConstTypeVarListDecl.ConstDeclList(); 
				while (la.kind == 1) {
					ConstDecl(lst);
					Expect(7);
				}
				o.ConstTypeVarDecls.Add(lst); 
			} else if (la.kind == 16) {
				Get();
				var lst = new CPParser.Ast.IConstTypeVarListDecl.TypeDeclList(); 
				while (la.kind == 1) {
					TypeDecl(lst);
					Expect(7);
				}
				o.ConstTypeVarDecls.Add(lst); 
			} else {
				Get();
				var lst = new CPParser.Ast.IConstTypeVarListDecl.VarDeclList(); 
				while (la.kind == 1) {
					VarDecl(lst);
					Expect(7);
				}
				o.ConstTypeVarDecls.Add(lst); 
			}
		}
		while (la.kind == 20) {
			if (la.kind == 20) {
				ProcDecl(o.ProcForwardDecls);
				Expect(7);
			} else {
				ForwardDecl(o.ProcForwardDecls);
				Expect(7);
			}
		}
	}

	void StatementSeq() {
		var o = new CPParser.Ast.StatementSeq(); 
		Statement();
		while (la.kind == 7) {
			Get();
			Statement();
		}
	}

	void ImportedModule(CPParser.Ast.AstList i) {
		var o = new CPParser.Ast.Import(); 
		Ident(out o.Name);
		if (la.kind == 12) {
			Get();
			Ident(out o.OriginalName);
		}
		i.Add(o); 
	}

	void ConstDecl(CPParser.Ast.AstList lst) {
		var o = new CPParser.Ast.ConstDecl(); 
		IdentDef(out o.IdentDef);
		Expect(18);
		ConstExpr();
		lst.Add(o); 
	}

	void TypeDecl(CPParser.Ast.AstList lst) {
		var o = new CPParser.Ast.TypeDecl(); 
		IdentDef(out o.IdentDef);
		Expect(18);
		Type(out o.Type_);
		lst.Add(o); 
	}

	void VarDecl(CPParser.Ast.AstList lst) {
		var o = new CPParser.Ast.VarDecl(); 
		IdentList(out o.IdentList);
		Expect(19);
		Type(out o.Type_);
		lst.Add(o); 
	}

	void ProcDecl(CPParser.Ast.AstList lst) {
		var o = new CPParser.Ast.ProcDecl(); 
		Expect(20);
		if (la.kind == 26) {
			Receiver();
		}
		IdentDef(out o.IdentDef);
		if (la.kind == 26) {
			FormalPars();
		}
		MethAttributes();
		if (la.kind == 7) {
			Get();
			DeclSeq(out o.DeclSeq);
			if (la.kind == 8) {
				Get();
				StatementSeq();
			}
			Expect(10);
			Expect(1);
		}
		lst.Add(o); 
	}

	void ForwardDecl(CPParser.Ast.AstList lst) {
		var o = new CPParser.Ast.ForwardDecl(); 
		Expect(20);
		Expect(21);
		if (la.kind == 26) {
			Receiver();
		}
		IdentDef(out o.IdentDef);
		if (la.kind == 26) {
			FormalPars();
		}
		MethAttributes();
		lst.Add(o); 
	}

	void IdentDef(out CPParser.Ast.IdentDef o) {
		o = new CPParser.Ast.IdentDef (); 
		Ident(out o.Ident);
		if (la.kind == 54 || la.kind == 66) {
			if (la.kind == 66) {
				Get();
				o.Export = CPParser.Ast.IdentDef.IdentExport.Export; 
			} else {
				Get();
				o.Export = CPParser.Ast.IdentDef.IdentExport.ExportReadonly; 
			}
		}
	}

	void ConstExpr() {
		var o = new CPParser.Ast.ConstExpr(); 
		Expr();
	}

	void Type(out CPParser.Ast.IType o) {
		o = null; 
		if (la.kind == 1) {
			var at = new CPParser.Ast.IType.SynonimType(); 
			Qualident(out at.Qualident);
			o = at; 
		} else if (la.kind == 30) {
			Get();
			var at = new CPParser.Ast.IType.ArrayType(); 
			if (StartOf(1)) {
				ConstExpr();
				while (la.kind == 14) {
					Get();
					ConstExpr();
				}
			}
			Expect(31);
			Type(out at.Type_);
			o = at; 
		} else if (StartOf(2)) {
			var at = new CPParser.Ast.IType.RecordType(); 
			if (la.kind == 23 || la.kind == 25 || la.kind == 32) {
				if (la.kind == 23) {
					Get();
				} else if (la.kind == 25) {
					Get();
				} else {
					Get();
				}
			}
			Expect(33);
			if (la.kind == 26) {
				Get();
				Qualident(out at.Qualident);
				Expect(27);
			}
			FieldList();
			while (la.kind == 7) {
				Get();
				FieldList();
			}
			Expect(10);
			o = at; 
		} else if (la.kind == 34) {
			var at = new CPParser.Ast.IType.PointerType(); 
			Get();
			Expect(35);
			Type(out at.Type_);
			o = at; 
		} else if (la.kind == 20) {
			var at = new CPParser.Ast.IType.ProcedureType(); 
			Get();
			if (la.kind == 26) {
				FormalPars();
			}
			o = at; 
		} else SynErr(76);
	}

	void IdentList(out CPParser.Ast.IdentList o) {
		o = new CPParser.Ast.IdentList(); var id = new CPParser.Ast.IdentDef(); 
		IdentDef(out id);
		o.IdentDefs.Add(id); 
		while (la.kind == 14) {
			Get();
			id = new CPParser.Ast.IdentDef(); 
			IdentDef(out id);
			o.IdentDefs.Add(id); 
		}
	}

	void Receiver() {
		var o = new CPParser.Ast.Receiver(); 
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
		var o = new CPParser.Ast.FormalPars(); 
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
			Type(out o.Type_);
		}
	}

	void MethAttributes() {
		var o = new CPParser.Ast.MethAttributes(); 
		if (la.kind == 14) {
			Get();
			Expect(22);
		}
		if (la.kind == 14) {
			Get();
			if (la.kind == 23) {
				Get();
			} else if (la.kind == 24) {
				Get();
			} else if (la.kind == 25) {
				Get();
			} else SynErr(77);
		}
	}

	void FPSection() {
		var o = new CPParser.Ast.FPSection(); 
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
		Type(out o.Type_);
	}

	void Qualident(out CPParser.Ast.Qualident o) {
		o = new CPParser.Ast.Qualident(); 
		if (la.kind == 1) {
			Ident(out o.Qualifier);
			Expect(11);
		}
		Ident(out o.Ident);
	}

	void FieldList() {
		CPParser.Ast.IdentList a; CPParser.Ast.IType t; 
		if (la.kind == 1) {
			IdentList(out a);
			Expect(19);
			Type(out t);
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
		var o = new CPParser.Ast.Designator(); 
		Qualident(out o.Qualident);
		while (StartOf(5)) {
			if (la.kind == 11) {
				Get();
				Expect(1);
			} else if (la.kind == 71) {
				Get();
				ExprList();
				Expect(72);
			} else if (la.kind == 21) {
				Get();
			} else if (la.kind == 26) {
				Get();
				Qualident(out o.Qualident);
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
			o.EndOfLine = true; 
		}
	}

	void Expr() {
		var o = new CPParser.Ast.Expr(); 
		SimpleExpr();
		if (StartOf(6)) {
			Relation();
			SimpleExpr();
		}
	}

	void ExprList() {
		var o = new CPParser.Ast.ExprList(); 
		Expr();
		while (la.kind == 14) {
			Get();
			Expr();
		}
	}

	void Case() {
		var o = new CPParser.Ast.Case(); 
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
		var o = new CPParser.Ast.Guard(); 
		Qualident(out o.VarQualident);
		Expect(19);
		Qualident(out o.TypeQualident);
	}

	void CaseLabels() {
		var o = new CPParser.Ast.CaseLabels(); 
		ConstExpr();
		if (la.kind == 52) {
			Get();
			ConstExpr();
		}
	}

	void SimpleExpr() {
		var o = new CPParser.Ast.SimpleExpr(); 
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
			var o = new CPParser.Ast.Relation(); 
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
		var o = new CPParser.Ast.Term(); 
		Factor();
		while (StartOf(7)) {
			MulOp();
			Factor();
		}
	}

	void AddOp() {
		if (la.kind == 53) {
			var o = new CPParser.Ast.AddOp(); 
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
			var o = new CPParser.Ast.IFactor.DesignatorFactor(); 
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
			var o = new CPParser.Ast.MulOp(); 
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
		var o = new CPParser.Ast.Set(); 
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
		var o = new CPParser.Ast.Element(); 
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
		CP();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _T,_x,_T,_x, _T,_x,_T,_x, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x},
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
			case 21: s = "\"^\" expected"; break;
			case 22: s = "\"NEW\" expected"; break;
			case 23: s = "\"ABSTRACT\" expected"; break;
			case 24: s = "\"EMPTY\" expected"; break;
			case 25: s = "\"EXTENSIBLE\" expected"; break;
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
