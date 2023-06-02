using AOParser;



using System;



public class Parser {
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _integer = 2;
	public const int _real = 3;
	public const int _character = 4;
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

public AOParser.Ast.Module module; 



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
		Module(out module);
	}

	void Module(out AOParser.Ast.Module o) {
		o = new AOParser.Ast.Module(); 
		Expect(6);
		Ident(out o.Ident);
		Expect(7);
		if (la.kind == 10) {
			ImportList(out o.ImportList);
		}
		while (la.kind == 12) {
			Definition(out o.Definition);
		}
		DeclSeq(out o.DeclSeq);
		Body(out o.Body);
		Expect(1);
		Expect(8);
	}

	void Ident(out AOParser.Ast.Ident o) {
		Expect(1);
		o = new AOParser.Ast.Ident{Name = t.val}; 
	}

	void Number(out AOParser.Ast.Number o) {
		o = null; 
		if (la.kind == 2) {
			Get();
			o = new AOParser.Ast.Number{Value = t.val}; 
		} else if (la.kind == 3) {
			Get();
			o = new AOParser.Ast.Number{Value = t.val}; 
		} else SynErr(71);
	}

	void ImportList(out AOParser.Ast.AstList o) {
		o = new AOParser.Ast.AstList(); 
		Expect(10);
		ImportedModule(o);
		while (la.kind == 11) {
			Get();
			ImportedModule(o);
		}
		Expect(7);
	}

	void Definition(out AOParser.Ast.Definition o) {
		o = new AOParser.Ast.Definition(); 
		Expect(12);
		Ident(out o.Ident);
		if (la.kind == 13) {
			Get();
			Qualident(out o.Qualident);
		}
		while (la.kind == 14) {
			Get();
			var p = new AOParser.Ast.DefinitionProc(); 
			Ident(out p.Ident);
			if (la.kind == 25) {
				FormalPars(out p.FormalPars);
			}
			Expect(7);
			o.Procs.Add(p); 
		}
		Expect(15);
		Expect(1);
	}

	void DeclSeq(out AOParser.Ast.DeclSeq o) {
		o = new AOParser.Ast.DeclSeq(); 
		while (la.kind == 16 || la.kind == 17 || la.kind == 18) {
			if (la.kind == 16) {
				Get();
				var lst = new AOParser.Ast.IConstTypeVarListDecl.ConstDeclList(); 
				while (la.kind == 1) {
					ConstDecl(lst);
					Expect(7);
				}
				o.ConstTypeVarDecls.Add(lst); 
			} else if (la.kind == 17) {
				Get();
				var lst = new AOParser.Ast.IConstTypeVarListDecl.TypeDeclList(); 
				while (la.kind == 1) {
					TypeDecl(lst);
					Expect(7);
				}
				o.ConstTypeVarDecls.Add(lst); 
			} else {
				Get();
				var lst = new AOParser.Ast.IConstTypeVarListDecl.VarDeclList(); 
				while (la.kind == 1) {
					VarDecl(lst);
					Expect(7);
				}
				o.ConstTypeVarDecls.Add(lst); 
				while (la.kind == 14) {
					ProcDecl(o.ProcDecl);
					Expect(7);
				}
			}
		}
	}

	void Body(out AOParser.Ast.Body o) {
		o = new AOParser.Ast.Body(); 
		if (la.kind == 34) {
			StatBlock(out o.StatBlock);
		} else if (la.kind == 15) {
			Get();
		} else SynErr(72);
	}

	void ImportedModule(AOParser.Ast.AstList i) {
		var o = new AOParser.Ast.Import(); 
		Ident(out o.Name);
		if (la.kind == 9) {
			Get();
			Ident(out o.OriginalName);
		}
		i.Add(o); 
	}

	void Qualident(out AOParser.Ast.Qualident o) {
		o = new AOParser.Ast.Qualident(); 
		Ident(out o.Ident1);
		if (la.kind == 8) {
			Get();
			Ident(out o.Ident2);
		}
	}

	void FormalPars(out AOParser.Ast.FormalPars o) {
		o = new AOParser.Ast.FormalPars(); 
		Expect(25);
		if (la.kind == 1 || la.kind == 18) {
			FPSection(o.FPSections);
			while (la.kind == 7) {
				Get();
				FPSection(o.FPSections);
			}
		}
		Expect(26);
		if (la.kind == 20) {
			Get();
			Qualident(out o.Qualident);
		}
	}

	void ConstDecl(AOParser.Ast.AstList lst) {
		var o = new AOParser.Ast.ConstDecl(); 
		IdentDef(out o.IdentDef);
		Expect(19);
		ConstExpr(out o.ConstExpr);
		lst.Add(o); 
	}

	void TypeDecl(AOParser.Ast.AstList lst) {
		var o = new AOParser.Ast.TypeDecl(); 
		IdentDef(out o.IdentDef);
		Expect(19);
		Type(out o.Type_);
		lst.Add(o); 
	}

	void VarDecl(AOParser.Ast.AstList lst) {
		var o = new AOParser.Ast.VarDecl(); 
		IdentList(out o.IdentList);
		Expect(20);
		Type(out o.Type_);
		lst.Add(o); 
	}

	void ProcDecl(AOParser.Ast.AstList lst) {
		Expect(14);
		var o = new AOParser.Ast.ProcDecl(); 
		ProcHead(out o.ProcHead);
		Expect(7);
		DeclSeq(out o.DeclSeq);
		Body(out o.Body);
		Ident(out o.Ident);
		lst.Add(o); 
	}

	void IdentDef(out AOParser.Ast.IdentDef o) {
		o = new AOParser.Ast.IdentDef (); 
		Ident(out o.Ident);
		if (la.kind == 21 || la.kind == 56) {
			if (la.kind == 21) {
				Get();
				o.Export = AOParser.Ast.IdentDef.IdentExport.Export; 
			} else {
				Get();
				o.Export = AOParser.Ast.IdentDef.IdentExport.ExportReadonly; 
			}
		}
	}

	void ConstExpr(out AOParser.Ast.ConstExpr o) {
		o = new AOParser.Ast.ConstExpr(); 
		Expr(out o.Expr);
	}

	void Type(out AOParser.Ast.IType o) {
		o = null; 
		switch (la.kind) {
		case 1: {
			var at = new AOParser.Ast.IType.SynonimType(); 
			Qualident(out at.Qualident);
			o = at; 
			break;
		}
		case 27: {
			Get();
			var at = new AOParser.Ast.IType.ArrayType(); AOParser.Ast.ConstExpr ce; 
			if (la.kind == 23) {
				SysFlag(out at.SysFlag);
			}
			if (StartOf(1)) {
				ConstExpr(out ce);
				at.ConstExprs.Add(ce); 
				while (la.kind == 11) {
					Get();
					ConstExpr(out ce);
					at.ConstExprs.Add(ce); 
				}
			}
			Expect(28);
			Type(out at.Type_);
			o = at; 
			break;
		}
		case 29: {
			var at = new AOParser.Ast.IType.RecordType(); 
			Get();
			if (la.kind == 23) {
				SysFlag(out at.SysFlag);
			}
			if (la.kind == 25) {
				Get();
				Qualident(out at.Qualident);
				Expect(26);
			}
			if (la.kind == 1 || la.kind == 7 || la.kind == 15) {
				FieldList(out at.FieldList);
			}
			Expect(15);
			o = at; 
			break;
		}
		case 30: {
			var at = new AOParser.Ast.IType.PointerType(); 
			Get();
			if (la.kind == 23) {
				SysFlag(out at.SysFlag);
			}
			Expect(31);
			Type(out at.Type_);
			o = at; 
			break;
		}
		case 32: {
			var at = new AOParser.Ast.IType.ObjectType(); 
			Get();
			if (StartOf(2)) {
				if (la.kind == 23) {
					SysFlag(out at.SysFlag);
				}
				if (la.kind == 25) {
					Get();
					Qualident(out at.Qualident);
					Expect(26);
				}
				if (la.kind == 33) {
					Get();
					Qualident(out at.ImplementsQualident);
				}
				DeclSeq(out at.DeclSeq);
				Body(out at.Body);
				Ident(out at.Ident);
			}
			o = at; 
			break;
		}
		case 14: {
			var at = new AOParser.Ast.IType.ProcedureType(); 
			Get();
			if (la.kind == 23) {
				SysFlag(out at.SysFlag);
			}
			if (la.kind == 25) {
				FormalPars(out at.FormalPars);
			}
			o = at; 
			break;
		}
		default: SynErr(73); break;
		}
	}

	void IdentList(out AOParser.Ast.IdentList o) {
		o = new AOParser.Ast.IdentList(); AOParser.Ast.IdentDef id; 
		IdentDef(out id);
		o.IdentDefs.Add(id); 
		while (la.kind == 11) {
			Get();
			IdentDef(out id);
			o.IdentDefs.Add(id); 
		}
	}

	void ProcHead(out AOParser.Ast.ProcHead o) {
		o = new AOParser.Ast.ProcHead(); 
		if (la.kind == 23) {
			SysFlag(out o.SysFlag);
		}
		if (la.kind == 21 || la.kind == 22) {
			if (la.kind == 21) {
				Get();
				o.Tag = AOParser.Ast.ProcHead.Tags.Export; 
			} else {
				Get();
				o.Tag = AOParser.Ast.ProcHead.Tags.Initializer; 
			}
		}
		IdentDef(out o.IdentDef);
		if (la.kind == 25) {
			FormalPars(out o.FormalPars);
		}
	}

	void SysFlag(out AOParser.Ast.SysFlag o) {
		o = new AOParser.Ast.SysFlag(); 
		Expect(23);
		Ident(out o.Ident);
		Expect(24);
	}

	void FPSection(AOParser.Ast.AstList lst) {
		var o = new AOParser.Ast.FPSection(); 
		if (la.kind == 18) {
			Get();
			o.FpSectionPrefix = AOParser.Ast.FPSection.Prefix.VAR; 
		}
		AOParser.Ast.Ident i; 
		Ident(out i);
		o.Idents.Add(i); 
		while (la.kind == 11) {
			Get();
			Ident(out i);
			o.Idents.Add(i); 
		}
		Expect(20);
		Type(out o.Type_);
		lst.Add(o); 
	}

	void FieldList(out AOParser.Ast.FieldList o) {
		o = new AOParser.Ast.FieldList(); AOParser.Ast.FieldDecl fd; 
		FieldDecl(out fd);
		o.FieldDecl.Add(fd); 
		while (la.kind == 7) {
			Get();
			FieldDecl(out fd);
			o.FieldDecl.Add(fd); 
		}
	}

	void FieldDecl(out AOParser.Ast.FieldDecl o) {
		o = new AOParser.Ast.FieldDecl(); 
		if (la.kind == 1) {
			IdentList(out o.IdentList);
			Expect(20);
			Type(out o.Type_);
		}
	}

	void StatBlock(out AOParser.Ast.StatBlock o) {
		o = new AOParser.Ast.StatBlock(); AOParser.Ast.IdentList il; 
		Expect(34);
		if (la.kind == 35) {
			Get();
			IdentList(out il);
			o.IdentLists.Add(il); 
			Expect(36);
		}
		if (StartOf(3)) {
			StatementSeq(out o.StatementSeq);
		}
		Expect(15);
	}

	void StatementSeq(out AOParser.Ast.StatementSeq o) {
		o = new AOParser.Ast.StatementSeq(); AOParser.Ast.IStatement s; 
		Statement(out s);
		o.Statements.Add(s); 
		while (la.kind == 7) {
			Get();
			Statement(out s);
			o.Statements.Add(s); 
		}
	}

	void Statement(out AOParser.Ast.IStatement os) {
		os=null; 
		if (StartOf(4)) {
			switch (la.kind) {
			case 1: {
				AOParser.Ast.Designator d; 
				Designator(out d);
				if (la.kind == 9) {
					var o = new AOParser.Ast.IStatement.AssignmentStatement(); o.Designator = d; 
					Get();
					Expr(out o.Expr);
					os = o; 
				} else if (StartOf(5)) {
					var o = new AOParser.Ast.IStatement.ProcCallStatement(); o.Designator = d; 
					if (la.kind == 25) {
						Get();
						if (StartOf(1)) {
							ExprList(out o.ExprList);
						}
						Expect(26);
					}
					os = o;
				} else SynErr(74);
				break;
			}
			case 37: {
				var o = new AOParser.Ast.IStatement.IfStatement(); 
				Get();
				Expr(out o.If.Cond);
				Expect(38);
				StatementSeq(out o.If.ThenBody);
				while (la.kind == 39) {
					var elsif = new AOParser.Ast.IStatement.IfStatement.IfThen(); 
					Get();
					Expr(out elsif.Cond);
					Expect(38);
					StatementSeq(out elsif.ThenBody);
					o.ELSIFs.Add(elsif); 
				}
				if (la.kind == 40) {
					Get();
					StatementSeq(out o.ElseBody);
				}
				Expect(15);
				os = o; 
				break;
			}
			case 41: {
				var o = new AOParser.Ast.IStatement.CaseStatement(); 
				Get();
				Expr(out o.Expr);
				Expect(42);
				var c = new AOParser.Ast.Case(); 
				Case(out c);
				o.Cases.Add(c); 
				while (la.kind == 43) {
					Get();
					Case(out c);
					o.Cases.Add(c); 
				}
				if (la.kind == 40) {
					Get();
					StatementSeq(out o.ElseBody);
				}
				Expect(15);
				os = o;
				break;
			}
			case 44: {
				var o = new AOParser.Ast.IStatement.WhileStatement(); 
				Get();
				Expr(out o.Expr);
				Expect(42);
				StatementSeq(out o.StatementSeq);
				Expect(15);
				os = o; 
				break;
			}
			case 45: {
				var o = new AOParser.Ast.IStatement.RepeatStatement(); 
				Get();
				StatementSeq(out o.StatementSeq);
				Expect(46);
				Expr(out o.Expr);
				os = o; 
				break;
			}
			case 47: {
				var o = new AOParser.Ast.IStatement.ForStatement(); 
				Get();
				Ident(out o.Ident);
				Expect(9);
				Expr(out o.Expr);
				Expect(31);
				Expr(out o.ToExpr);
				if (la.kind == 48) {
					Get();
					ConstExpr(out o.ByExpr);
				}
				Expect(42);
				StatementSeq(out o.StatementSeq);
				Expect(15);
				os = o;
				break;
			}
			case 49: {
				var o = new AOParser.Ast.IStatement.LoopStatement(); 
				Get();
				StatementSeq(out o.StatementSeq);
				Expect(15);
				os = o;
				break;
			}
			case 50: {
				var o = new AOParser.Ast.IStatement.WithStatement(); 
				Get();
				Qualident(out o.Qualident1);
				Expect(20);
				Qualident(out o.Qualident2);
				Expect(42);
				StatementSeq(out o.StatementSeq);
				Expect(15);
				os = o;
				break;
			}
			case 51: {
				var o = new AOParser.Ast.IStatement.ExitStatement(); 
				Get();
				os = o;
				break;
			}
			case 52: {
				var o = new AOParser.Ast.IStatement.ReturnStatement(); 
				Get();
				if (StartOf(1)) {
					Expr(out o.Expr);
				}
				os = o;
				break;
			}
			case 53: {
				var o = new AOParser.Ast.IStatement.AwaitStatement(); 
				Get();
				Expect(25);
				Expr(out o.Expr);
				Expect(26);
				os = o;
				break;
			}
			case 34: {
				var o = new AOParser.Ast.IStatement.StatBlockStatement(); 
				StatBlock(out o.StatBlock);
				os = o;
				break;
			}
			}
		}
	}

	void Designator(out AOParser.Ast.Designator o) {
		o = new AOParser.Ast.Designator(); 
		Qualident(out o.Qualident);
		while (la.kind == 8 || la.kind == 23 || la.kind == 25) {
			if (la.kind == 8) {
				Get();
				var s = new AOParser.Ast.Designator.IDesignatorSpec.RecordDesignatorSpec(); 
				Ident(out s.Value);
				o.Specs.Add(s); 
			} else if (la.kind == 23) {
				Get();
				var s = new AOParser.Ast.Designator.IDesignatorSpec.ArrayDesignatorSpec(); 
				ExprList(out s.Value);
				Expect(24);
				o.Specs.Add(s); 
			} else {
				Get();
				var s = new AOParser.Ast.Designator.IDesignatorSpec.ProcCallDesignatorSpec(); 
				if (StartOf(1)) {
					ExprList(out s.Value);
				}
				o.Specs.Add(s); 
				Expect(26);
			}
		}
	}

	void Expr(out AOParser.Ast.Expr o) {
		o = new AOParser.Ast.Expr(); 
		SimpleExpr(out o.SimpleExpr);
		if (StartOf(6)) {
			Relation(out o.Relation);
			SimpleExpr(out o.SimpleExpr2);
		}
	}

	void ExprList(out AOParser.Ast.ExprList o) {
		o = new AOParser.Ast.ExprList(); AOParser.Ast.Expr e; 
		Expr(out e);
		o.Exprs.Add(e); 
		while (la.kind == 11) {
			Get();
			Expr(out e);
			o.Exprs.Add(e); 
		}
	}

	void Case(out AOParser.Ast.Case o) {
		o = new AOParser.Ast.Case(); AOParser.Ast.CaseLabels cl; 
		if (StartOf(1)) {
			CaseLabels(out cl);
			o.CaseLabels.Add(cl); 
			while (la.kind == 11) {
				Get();
				CaseLabels(out cl);
				o.CaseLabels.Add(cl); 
			}
			Expect(20);
			StatementSeq(out o.StatementSeq);
		}
	}

	void CaseLabels(out AOParser.Ast.CaseLabels o) {
		o = new AOParser.Ast.CaseLabels(); 
		ConstExpr(out o.ConstExpr1);
		if (la.kind == 54) {
			Get();
			ConstExpr(out o.ConstExpr2);
		}
	}

	void SimpleExpr(out AOParser.Ast.SimpleExpr o) {
		o = new AOParser.Ast.SimpleExpr(); AOParser.Ast.SimpleElementExpr e; 
		Term(out o.Term);
		while (StartOf(7)) {
			e = new AOParser.Ast.SimpleElementExpr(); 
			MulOp(out e.MulOp);
			Term(out e.Term);
			o.SimpleExprElements.Add(e); 
		}
	}

	void Relation(out AOParser.Ast.Relation o) {
		o = new AOParser.Ast.Relation(); 
		switch (la.kind) {
		case 19: {
			Get();
			o.Op = AOParser.Ast.Relation.Relations.Eq ; 
			break;
		}
		case 59: {
			Get();
			o.Op = AOParser.Ast.Relation.Relations.Neq ; 
			break;
		}
		case 60: {
			Get();
			o.Op = AOParser.Ast.Relation.Relations.Lss ; 
			break;
		}
		case 61: {
			Get();
			o.Op = AOParser.Ast.Relation.Relations.Leq ; 
			break;
		}
		case 62: {
			Get();
			o.Op = AOParser.Ast.Relation.Relations.Gtr ; 
			break;
		}
		case 63: {
			Get();
			o.Op = AOParser.Ast.Relation.Relations.Geq ; 
			break;
		}
		case 64: {
			Get();
			o.Op = AOParser.Ast.Relation.Relations.In ; 
			break;
		}
		case 65: {
			Get();
			o.Op = AOParser.Ast.Relation.Relations.Is ; 
			break;
		}
		default: SynErr(75); break;
		}
	}

	void Term(out AOParser.Ast.Term o) {
		o = new AOParser.Ast.Term(); AOParser.Ast.TermElementExpr e; 
		if (la.kind == 55 || la.kind == 56) {
			if (la.kind == 55) {
				Get();
				o.Prefix = AOParser.Ast.Term.TermExprPrefix.Add; 
			} else {
				Get();
				o.Prefix = AOParser.Ast.Term.TermExprPrefix.Sub; 
			}
		}
		Factor(out o.Factor);
		while (la.kind == 55 || la.kind == 56 || la.kind == 69) {
			e = new AOParser.Ast.TermElementExpr(); 
			AddOp(out e.AddOp);
			Factor(out e.Factor);
			o.TermElements.Add(e); 
		}
	}

	void MulOp(out AOParser.Ast.MulOp o) {
		o = new AOParser.Ast.MulOp(); 
		if (la.kind == 21) {
			Get();
			o.Op = AOParser.Ast.MulOp.MulOps.Mul; 
		} else if (la.kind == 66) {
			Get();
			o.Op = AOParser.Ast.MulOp.MulOps.Division; 
		} else if (la.kind == 67) {
			Get();
			o.Op = AOParser.Ast.MulOp.MulOps.DIV; 
		} else if (la.kind == 68) {
			Get();
			o.Op = AOParser.Ast.MulOp.MulOps.MOD; 
		} else if (la.kind == 22) {
			Get();
			o.Op = AOParser.Ast.MulOp.MulOps.AND; 
		} else SynErr(76);
	}

	void Factor(out AOParser.Ast.IFactor f) {
		f = null;
		switch (la.kind) {
		case 1: {
			var o = new AOParser.Ast.IFactor.DesignatorFactor(); 
			Designator(out o.Value);
			f = o; 
			break;
		}
		case 2: case 3: {
			var o = new AOParser.Ast.IFactor.NumberFactor(); 
			Number(out o.Value);
			f = o; 
			break;
		}
		case 4: {
			var o = new AOParser.Ast.IFactor.CharacterFactor(); 
			Get();
			o.Value = t.val; f = o; 
			break;
		}
		case 5: {
			var o = new AOParser.Ast.IFactor.StringFactor(); 
			Get();
			o.Value = t.val; f = o; 
			break;
		}
		case 57: {
			var o = new AOParser.Ast.IFactor.NilFactor(); 
			Get();
			f = o; 
			break;
		}
		case 35: {
			var o = new AOParser.Ast.IFactor.SetFactor(); 
			Set(out o.Value);
			f = o; 
			break;
		}
		case 25: {
			var o = new AOParser.Ast.IFactor.ExprFactor(); 
			Get();
			Expr(out o.Value);
			Expect(26);
			f = o; 
			break;
		}
		case 58: {
			var o = new AOParser.Ast.IFactor.NegFactor(); 
			Get();
			Factor(out o.Value);
			f = o; 
			break;
		}
		default: SynErr(77); break;
		}
	}

	void AddOp(out AOParser.Ast.AddOp o) {
		o = new AOParser.Ast.AddOp(); 
		if (la.kind == 55) {
			Get();
			o.Op = AOParser.Ast.AddOp.AddOps.Add; 
		} else if (la.kind == 56) {
			Get();
			o.Op = AOParser.Ast.AddOp.AddOps.Sub; 
		} else if (la.kind == 69) {
			Get();
			o.Op = AOParser.Ast.AddOp.AddOps.Or; 
		} else SynErr(78);
	}

	void Set(out AOParser.Ast.Set o) {
		o = new AOParser.Ast.Set(); AOParser.Ast.Element e; 
		Expect(35);
		if (StartOf(1)) {
			Element(out e);
			o.Elements.Add(e); 
			while (la.kind == 11) {
				Get();
				Element(out e);
				o.Elements.Add(e); 
			}
		}
		Expect(36);
	}

	void Element(out AOParser.Ast.Element o) {
		o = new AOParser.Ast.Element(); 
		Expr(out o.Expr1);
		if (la.kind == 54) {
			Get();
			Expr(out o.Expr2);
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
		{_x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_T,_x, _x,_x,_x,_T, _x,_T,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_T,_x,_x, _x,_T,_x,_x, _T,_T,_x,_T, _x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_T,_x,_x, _x,_T,_x,_x, _T,_T,_x,_T, _x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_x,_x,_T, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
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
			case 4: s = "character expected"; break;
			case 5: s = "string expected"; break;
			case 6: s = "\"MODULE\" expected"; break;
			case 7: s = "\";\" expected"; break;
			case 8: s = "\".\" expected"; break;
			case 9: s = "\":=\" expected"; break;
			case 10: s = "\"IMPORT\" expected"; break;
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
			case 42: s = "\"DO\" expected"; break;
			case 43: s = "\"|\" expected"; break;
			case 44: s = "\"WHILE\" expected"; break;
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
			case 66: s = "\"/\" expected"; break;
			case 67: s = "\"DIV\" expected"; break;
			case 68: s = "\"MOD\" expected"; break;
			case 69: s = "\"OR\" expected"; break;
			case 70: s = "??? expected"; break;
			case 71: s = "invalid Number"; break;
			case 72: s = "invalid Body"; break;
			case 73: s = "invalid Type"; break;
			case 74: s = "invalid Statement"; break;
			case 75: s = "invalid Relation"; break;
			case 76: s = "invalid MulOp"; break;
			case 77: s = "invalid Factor"; break;
			case 78: s = "invalid AddOp"; break;

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
