using AOParser;



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

public Common.SymTable.SymTab symTab = new ();
	public AOParser.Ast.Module module; 
	//todo up to https://gitlab.inf.ethz.ch/felixf/oberon/-/blob/main/docu/OberonLanguageReport.pdf

	bool IsModule(){
		var obj = symTab.Find(t.val);
		return obj.objClass == Common.SymTable.ObjCLass.MODULE;
	}

	bool IsSet(){
		return t.val == "." && la.val == ".";
	}

	bool IsSizeOf(){
		var r = la.val == "SIZE";
		if (r){
			Token x = scanner.Peek();
			return x.val == "OF";
		}
		return r;
	}

	bool IsCast(){
		var obj = symTab.Find(t.val);
		if (obj.objClass == Common.SymTable.ObjCLass.FUNC) return false;
		if (obj.objClass == Common.SymTable.ObjCLass.VAR) {
			if (obj.type.form == Common.SymTable.TypeForm.FUNC)
				return false;
			if (obj.type.form == Common.SymTable.TypeForm.PREDEFINED) {
				if (AOParser.Types.TypeResolver.Resolve(obj.type).form == Common.SymTable.TypeForm.FUNC)
					return false;
			}
		}
		return obj.objClass == Common.SymTable.ObjCLass.VAR;
	}



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
		Expect(7);
		Ident(out o.Ident);
		Expect(8);
		symTab.OpenScope(); o.SetDefaultScope(symTab);
		if (la.kind == 11) {
			ImportList(out o.ImportList);
		}
		while (la.kind == 13) {
			Definition(out o.Definition);
		}
		DeclSeq(out o.DeclSeq);
		Body(out o.Body);
		Expect(1);
		Expect(9);
	}

	void Ident(out AOParser.Ast.Ident o) {
		if (la.kind == 1) {
			Get();
		} else if (la.kind == 6) {
			Get();
		} else SynErr(75);
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
		} else SynErr(76);
	}

	void ImportList(out AOParser.Ast.AstList o) {
		o = new AOParser.Ast.AstList(); 
		Expect(11);
		ImportedModule(o);
		while (la.kind == 12) {
			Get();
			ImportedModule(o);
		}
		Expect(8);
	}

	void Definition(out AOParser.Ast.Definition o) {
		o = new AOParser.Ast.Definition(); 
		Expect(13);
		Ident(out o.Ident);
		if (la.kind == 14) {
			Get();
			Qualident(out o.Qualident);
		}
		while (la.kind == 15) {
			Get();
			var p = new AOParser.Ast.DefinitionProc(); 
			Ident(out p.Ident);
			if (la.kind == 26) {
				FormalPars(out p.FormalPars);
			}
			Expect(8);
			o.Procs.Add(p); 
		}
		Expect(16);
		Expect(1);
	}

	void DeclSeq(out AOParser.Ast.DeclSeq o) {
		o = new AOParser.Ast.DeclSeq(); 
		while (StartOf(1)) {
			if (la.kind == 17) {
				Get();
				var lst = new AOParser.Ast.IConstTypeVarListDecl.ConstDeclList(); 
				while (la.kind == 1 || la.kind == 6) {
					ConstDecl(lst.Values);
					Expect(8);
				}
				o.ConstTypeVarDecls.Add(lst); 
			} else if (la.kind == 18) {
				Get();
				var lst = new AOParser.Ast.IConstTypeVarListDecl.TypeDeclList(); 
				while (la.kind == 1 || la.kind == 6) {
					TypeDecl(lst.Values);
					Expect(8);
				}
				o.ConstTypeVarDecls.Add(lst); 
			} else if (la.kind == 19) {
				Get();
				var lst = new AOParser.Ast.IConstTypeVarListDecl.VarDeclList(); 
				while (la.kind == 1 || la.kind == 6) {
					VarDecl(lst.Values);
					Expect(8);
				}
				o.ConstTypeVarDecls.Add(lst); 
			} else {
				ProcDecl(o.ProcDecl);
				Expect(8);
			}
		}
	}

	void Body(out AOParser.Ast.Body o) {
		o = new AOParser.Ast.Body(); 
		if (la.kind == 38) {
			StatBlock(out o.StatBlock);
		} else if (la.kind == 16) {
			Get();
		} else SynErr(77);
	}

	void ImportedModule(AOParser.Ast.AstList i) {
		var o = new AOParser.Ast.Import(); 
		Ident(out o.Name);
		if (la.kind == 10) {
			Get();
			Ident(out o.OriginalName);
		}
		i.Add(o); 
		symTab.Insert(o.GetObj());  
	}

	void Qualident(out AOParser.Ast.Qualident o) {
		o = new AOParser.Ast.Qualident(symTab.curScope); 
		Ident(out o.Ident1);
		if (IsModule()) {
			Expect(9);
			Ident(out o.Ident2);
		}
	}

	void FormalPars(out AOParser.Ast.FormalPars o) {
		o = new AOParser.Ast.FormalPars(); 
		Expect(26);
		if (StartOf(2)) {
			FPSection(o.FPSections);
			while (la.kind == 8) {
				Get();
				FPSection(o.FPSections);
			}
		}
		Expect(27);
		if (la.kind == 21) {
			Get();
			Qualident(out o.Qualident);
		}
	}

	void ConstDecl(AOParser.Ast.AstList lst) {
		var o = new AOParser.Ast.ConstDecl(); 
		IdentDef(out o.IdentDef);
		Expect(20);
		ConstExpr(out o.ConstExpr);
		lst.Add(o); 
		symTab.Insert(o.GetObj());  
	}

	void TypeDecl(AOParser.Ast.AstList lst) {
		var o = new AOParser.Ast.TypeDecl(); 
		IdentDef(out o.IdentDef);
		Expect(20);
		Type(out o.Type_);
		lst.Add(o); 
		symTab.Insert(o.GetObj());  
	}

	void VarDecl(AOParser.Ast.AstList lst) {
		var o = new AOParser.Ast.VarDecl(); 
		IdentList(out o.IdentList);
		Expect(21);
		Type(out o.Type_);
		lst.Add(o); 
		symTab.Insert(o.GetObjects());  
	}

	void ProcDecl(AOParser.Ast.AstList lst) {
		Expect(15);
		var o = new AOParser.Ast.ProcDecl(); 
		ProcHead(out o.ProcHead);
		Expect(8);
		DeclSeq(out o.DeclSeq);
		Body(out o.Body);
		Ident(out o.Ident);
		lst.Add(o); 
		var scope = symTab.curScope; symTab.CloseScope();symTab.Insert(o.GetObj(scope)); 
	}

	void IdentDef(out AOParser.Ast.IdentDef o) {
		o = new AOParser.Ast.IdentDef (); 
		Ident(out o.Ident);
		if (la.kind == 25 || la.kind == 67) {
			if (la.kind == 67) {
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
		case 1: case 6: {
			var at = new AOParser.Ast.IType.SynonimType(); 
			Qualident(out at.Qualident);
			o = at; 
			break;
		}
		case 30: {
			Get();
			var at = new AOParser.Ast.IType.ArrayType(); AOParser.Ast.ConstExpr ce; 
			if (StartOf(3)) {
				ConstExpr(out ce);
				at.ConstExprs.Add(ce); 
				while (la.kind == 12) {
					Get();
					ConstExpr(out ce);
					at.ConstExprs.Add(ce); 
				}
			}
			Expect(31);
			Type(out at.Type_);
			o = at; 
			break;
		}
		case 32: {
			symTab.OpenScope(); var at = new AOParser.Ast.IType.RecordType(symTab.curScope); 
			Get();
			if (la.kind == 26) {
				Get();
				Qualident(out at.Qualident);
				Expect(27);
			}
			FieldList(out at.FieldList);
			Expect(16);
			o = at; symTab.CloseScope();
			break;
		}
		case 33: {
			var at = new AOParser.Ast.IType.PointerType(); 
			Get();
			if (la.kind == 28) {
				Flags(out at.Flags);
			}
			Expect(34);
			Type(out at.Type_);
			o = at; 
			break;
		}
		case 35: {
			symTab.OpenScope(true); var at = new AOParser.Ast.IType.ObjectType(symTab.curScope);
			Get();
			if (StartOf(4)) {
				if (la.kind == 28) {
					Flags(out at.Flags);
				}
				if (la.kind == 26) {
					Get();
					Qualident(out at.Qualident);
					Expect(27);
				}
				if (la.kind == 36) {
					Get();
					Qualident(out at.ImplementsQualident);
				}
				DeclSeq(out at.DeclSeq);
				Body(out at.Body);
				Ident(out at.Ident);
			}
			o = at; symTab.CloseScope();
			break;
		}
		case 15: {
			symTab.OpenScope(); var at = new AOParser.Ast.IType.ProcedureType(symTab.curScope); 
			Get();
			if (la.kind == 28) {
				Flags(out at.Flags);
			}
			if (la.kind == 26) {
				FormalPars(out at.FormalPars);
			}
			o = at; symTab.CloseScope(); 
			break;
		}
		case 37: {
			symTab.OpenScope(); var at = new AOParser.Ast.IType.EnumType(symTab.curScope);  AOParser.Ast.EnumItem e = new AOParser.Ast.EnumItem();
			Get();
			if (la.kind == 26) {
				Get();
				Qualident(out at.Qualident);
				Expect(27);
			}
			IdentDef(out e.IdentDef);
			if (la.kind == 20) {
				Get();
				Expr(out e.Expr);
			}
			at.Enums.Add(e); 
			while (la.kind == 12) {
				e = new AOParser.Ast.EnumItem(); 
				Get();
				IdentDef(out e.IdentDef);
				if (la.kind == 20) {
					Get();
					Expr(out e.Expr);
				}
				at.Enums.Add(e); 
			}
			Expect(16);
			o = at; symTab.CloseScope(); 
			break;
		}
		default: SynErr(78); break;
		}
	}

	void IdentList(out AOParser.Ast.IdentList o) {
		o = new AOParser.Ast.IdentList(); AOParser.Ast.IdentDef id; 
		IdentDef(out id);
		o.IdentDefs.Add(id); 
		while (la.kind == 12) {
			Get();
			IdentDef(out id);
			o.IdentDefs.Add(id); 
		}
	}

	void ProcHead(out AOParser.Ast.ProcHead o) {
		o = new AOParser.Ast.ProcHead(); 
		if (StartOf(5)) {
			if (la.kind == 22) {
				Get();
				o.Tag = "^"; 
			} else if (la.kind == 23) {
				Get();
				o.Tag = "&"; 
			} else if (la.kind == 24) {
				Get();
				o.Tag = "~"; 
			} else if (la.kind == 25) {
				Get();
				o.Tag = "-"; 
			} else {
				Flags(out o.Flags);
				if (la.kind == 25) {
					Get();
					o.Tag = "-"; 
				}
			}
		}
		IdentDef(out o.IdentDef);
		symTab.OpenScope(); 
		if (la.kind == 26) {
			FormalPars(out o.FormalPars);
		}
	}

	void Flags(out AOParser.Ast.Flags o) {
		o = new AOParser.Ast.Flags(); AOParser.Ast.Flag flag = null;
		Expect(28);
		if (la.kind == 1 || la.kind == 6) {
			Flag(out flag);
			o.Values.Add(flag); 
			while (la.kind == 12) {
				Get();
				Flag(out flag);
				o.Values.Add(flag); 
			}
		}
		Expect(29);
	}

	void Flag(out AOParser.Ast.Flag o) {
		o = new AOParser.Ast.Flag(); 
		Ident(out o.Ident);
		if (la.kind == 20 || la.kind == 26) {
			if (la.kind == 26) {
				Get();
				Expr(out o.QualExpr);
				Expect(27);
			} else {
				Get();
				Expr(out o.AssignExpr);
			}
		}
	}

	void Expr(out AOParser.Ast.Expr o) {
		o = new AOParser.Ast.Expr(symTab.curScope); 
		SimpleExpr(out o.SimpleExpr);
		if (StartOf(6)) {
			Relation(out o.Relation);
			SimpleExpr(out o.SimpleExpr2);
		}
	}

	void FPSection(AOParser.Ast.AstList lst) {
		var o = new AOParser.Ast.FPSection(); 
		if (la.kind == 17 || la.kind == 19) {
			if (la.kind == 19) {
				Get();
				o.FpSectionPrefix = AOParser.Ast.FPSection.Prefix.VAR; 
			} else {
				Get();
				o.FpSectionPrefix = AOParser.Ast.FPSection.Prefix.CONST; 
			}
		}
		AOParser.Ast.Ident i; 
		Ident(out i);
		o.Idents.Add(i); 
		while (la.kind == 12) {
			Get();
			Ident(out i);
			o.Idents.Add(i); 
		}
		Expect(21);
		Type(out o.Type_);
		lst.Add(o); 
		symTab.Insert(o.Objects());  
	}

	void FieldList(out AOParser.Ast.FieldList o) {
		o = new AOParser.Ast.FieldList(); AOParser.Ast.FieldDecl fd; 
		FieldDecl(out fd);
		o.FieldDecl.Add(fd); 
		while (la.kind == 8) {
			Get();
			FieldDecl(out fd);
			o.FieldDecl.Add(fd); 
		}
	}

	void FieldDecl(out AOParser.Ast.FieldDecl o) {
		o = new AOParser.Ast.FieldDecl(); 
		if (la.kind == 1 || la.kind == 6) {
			IdentList(out o.IdentList);
			Expect(21);
			Type(out o.Type_);
			symTab.Insert(o.Fields); 
		}
	}

	void StatBlock(out AOParser.Ast.StatBlock o) {
		o = new AOParser.Ast.StatBlock(); 
		Expect(38);
		if (la.kind == 28) {
			Flags(out o.Flags);
		}
		StatementSeq(out o.StatementSeq);
		Expect(16);
	}

	void StatementSeq(out AOParser.Ast.StatementSeq o) {
		o = new AOParser.Ast.StatementSeq(); AOParser.Ast.IStatement s; 
		Statement(out s);
		o.Statements.Add(s); 
		while (la.kind == 8) {
			Get();
			Statement(out s);
			o.Statements.Add(s); 
		}
	}

	void Statement(out AOParser.Ast.IStatement os) {
		os=null; 
		if (StartOf(7)) {
			switch (la.kind) {
			case 1: case 6: {
				AOParser.Ast.Designator d; 
				Designator(out d);
				if (la.kind == 10) {
					var o = new AOParser.Ast.IStatement.AssignmentStatement(); o.Designator = d; 
					Get();
					Expr(out o.Expr);
					os = o; 
				} else if (StartOf(8)) {
					var o = new AOParser.Ast.IStatement.ProcCallStatement(); o.Designator = d; 
					os = o;
				} else SynErr(79);
				break;
			}
			case 39: {
				var o = new AOParser.Ast.IStatement.IfStatement(); 
				Get();
				Expr(out o.If.Cond);
				Expect(40);
				StatementSeq(out o.If.ThenBody);
				while (la.kind == 41) {
					var elsif = new AOParser.Ast.IStatement.IfStatement.IfThen(); 
					Get();
					Expr(out elsif.Cond);
					Expect(40);
					StatementSeq(out elsif.ThenBody);
					o.ELSIFs.Add(elsif); 
				}
				if (la.kind == 42) {
					Get();
					StatementSeq(out o.ElseBody);
				}
				Expect(16);
				os = o; 
				break;
			}
			case 43: {
				var o = new AOParser.Ast.IStatement.CaseStatement(); 
				Get();
				Expr(out o.Expr);
				Expect(31);
				var c = new AOParser.Ast.Case(); 
				if (la.kind == 44) {
					Get();
				}
				Case(out c);
				o.Cases.Add(c); 
				while (la.kind == 44) {
					Get();
					Case(out c);
					o.Cases.Add(c); 
				}
				if (la.kind == 42) {
					Get();
					StatementSeq(out o.ElseBody);
				}
				Expect(16);
				os = o;
				break;
			}
			case 45: {
				var o = new AOParser.Ast.IStatement.WhileStatement(); 
				Get();
				Expr(out o.Expr);
				Expect(46);
				StatementSeq(out o.StatementSeq);
				Expect(16);
				os = o; 
				break;
			}
			case 47: {
				var o = new AOParser.Ast.IStatement.RepeatStatement(); 
				Get();
				StatementSeq(out o.StatementSeq);
				Expect(48);
				Expr(out o.Expr);
				os = o; 
				break;
			}
			case 49: {
				var o = new AOParser.Ast.IStatement.ForStatement(); 
				Get();
				Ident(out o.Ident);
				Expect(10);
				Expr(out o.Expr);
				Expect(34);
				Expr(out o.ToExpr);
				if (la.kind == 50) {
					Get();
					ConstExpr(out o.ByExpr);
				}
				Expect(46);
				StatementSeq(out o.StatementSeq);
				Expect(16);
				os = o;
				break;
			}
			case 51: {
				var o = new AOParser.Ast.IStatement.LoopStatement(); 
				Get();
				StatementSeq(out o.StatementSeq);
				Expect(16);
				os = o;
				break;
			}
			case 52: {
				var o = new AOParser.Ast.IStatement.WithStatement(); 
				Get();
				Qualident(out o.Qualident1);
				Expect(21);
				Qualident(out o.Qualident2);
				Expect(46);
				StatementSeq(out o.StatementSeq);
				Expect(16);
				os = o;
				break;
			}
			case 53: {
				var o = new AOParser.Ast.IStatement.ExitStatement(); 
				Get();
				os = o;
				break;
			}
			case 54: {
				var o = new AOParser.Ast.IStatement.ReturnStatement(); 
				Get();
				if (StartOf(3)) {
					Expr(out o.Expr);
				}
				os = o;
				break;
			}
			case 55: {
				var o = new AOParser.Ast.IStatement.IgnoreStatement(); 
				Get();
				if (StartOf(3)) {
					Expr(out o.Expr);
				}
				os = o;
				break;
			}
			case 56: {
				var o = new AOParser.Ast.IStatement.AwaitStatement(); 
				Get();
				Expect(26);
				Expr(out o.Expr);
				Expect(27);
				os = o;
				break;
			}
			case 38: {
				var o = new AOParser.Ast.IStatement.StatBlockStatement(); 
				StatBlock(out o.StatBlock);
				os = o;
				break;
			}
			}
		}
	}

	void Designator(out AOParser.Ast.Designator o) {
		o = new AOParser.Ast.Designator(this.symTab.curScope); 
		Qualident(out o.Qualident);
		while (StartOf(9)) {
			if (la.kind == 9) {
				Get();
				var s = new AOParser.Ast.Designator.IDesignatorSpec.RecordDesignatorSpec(); 
				Ident(out s.Value);
				o.Specs.Add(s); 
			} else if (la.kind == 72) {
				Get();
				var s = new AOParser.Ast.Designator.IDesignatorSpec.ArrayDesignatorSpec(); 
				ExprList(out s.Value);
				Expect(73);
				o.Specs.Add(s); 
			} else if (la.kind == 22) {
				Get();
				var s = new AOParser.Ast.Designator.IDesignatorSpec.PointerDesignatorSpec(); 
				o.Specs.Add(s); 
			} else if (IsCast()) {
				Expect(26);
				var s = new AOParser.Ast.Designator.IDesignatorSpec.CastDesignatorSpec(symTab.curScope); 
				Qualident(out s.Value);
				o.Specs.Add(s); 
				Expect(27);
			} else {
				var s = new AOParser.Ast.Designator.IDesignatorSpec.ProcCallDesignatorSpec(); 
				Get();
				if (StartOf(3)) {
					ExprList(out s.Value);
				}
				Expect(27);
				o.Specs.Add(s); 
			}
		}
	}

	void Case(out AOParser.Ast.Case o) {
		o = new AOParser.Ast.Case(); AOParser.Ast.CaseLabels cl; 
		CaseLabels(out cl);
		o.CaseLabels.Add(cl); 
		while (la.kind == 12) {
			Get();
			CaseLabels(out cl);
			o.CaseLabels.Add(cl); 
		}
		Expect(21);
		StatementSeq(out o.StatementSeq);
	}

	void CaseLabels(out AOParser.Ast.CaseLabels o) {
		o = new AOParser.Ast.CaseLabels(); 
		ConstExpr(out o.ConstExpr1);
		if (la.kind == 57) {
			Get();
			ConstExpr(out o.ConstExpr2);
		}
	}

	void SimpleExpr(out AOParser.Ast.SimpleExpr o) {
		o = new AOParser.Ast.SimpleExpr(); AOParser.Ast.SimpleElementExpr e; 
		Term(out o.Term);
		while (la.kind == 25 || la.kind == 58 || la.kind == 71) {
			e = new AOParser.Ast.SimpleElementExpr(); 
			AddOp(out e.AddOp);
			Term(out e.Term);
			o.SimpleExprElements.Add(e); 
		}
	}

	void Relation(out AOParser.Ast.Relation o) {
		o = new AOParser.Ast.Relation(); 
		switch (la.kind) {
		case 20: {
			Get();
			o.Op = AOParser.Ast.Relation.Relations.Eq ; 
			break;
		}
		case 60: {
			Get();
			o.Op = AOParser.Ast.Relation.Relations.Neq ; 
			break;
		}
		case 61: {
			Get();
			o.Op = AOParser.Ast.Relation.Relations.Lss ; 
			break;
		}
		case 62: {
			Get();
			o.Op = AOParser.Ast.Relation.Relations.Leq ; 
			break;
		}
		case 63: {
			Get();
			o.Op = AOParser.Ast.Relation.Relations.Gtr ; 
			break;
		}
		case 64: {
			Get();
			o.Op = AOParser.Ast.Relation.Relations.Geq ; 
			break;
		}
		case 65: {
			Get();
			o.Op = AOParser.Ast.Relation.Relations.In ; 
			break;
		}
		case 66: {
			Get();
			o.Op = AOParser.Ast.Relation.Relations.Is ; 
			break;
		}
		default: SynErr(80); break;
		}
	}

	void Term(out AOParser.Ast.Term o) {
		o = new AOParser.Ast.Term(); AOParser.Ast.TermElementExpr e; 
		Factor(out o.Factor);
		while (StartOf(10)) {
			e = new AOParser.Ast.TermElementExpr(); 
			MulOp(out e.MulOp);
			Factor(out e.Factor);
			o.TermElements.Add(e); 
		}
	}

	void AddOp(out AOParser.Ast.AddOp o) {
		o = new AOParser.Ast.AddOp(); 
		if (la.kind == 58) {
			Get();
			o.Op = AOParser.Ast.AddOp.AddOps.Add; 
		} else if (la.kind == 25) {
			Get();
			o.Op = AOParser.Ast.AddOp.AddOps.Sub; 
		} else if (la.kind == 71) {
			Get();
			o.Op = AOParser.Ast.AddOp.AddOps.Or; 
		} else SynErr(81);
	}

	void Factor(out AOParser.Ast.IFactor f) {
		f = null; AOParser.Ast.IFactor.FactorPrefix? p = null;
		if (la.kind == 25 || la.kind == 58) {
			if (la.kind == 58) {
				Get();
				p = AOParser.Ast.IFactor.FactorPrefix.Add; 
			} else {
				Get();
				p = AOParser.Ast.IFactor.FactorPrefix.Sub; 
			}
		}
		if (IsSizeOf()) {
			var o = new AOParser.Ast.IFactor.SizeOfFactor(); 
			Expect(6);
			Expect(31);
			Factor(out o.Value);
			f = o; 
		} else if (la.kind == 1 || la.kind == 6) {
			var o = new AOParser.Ast.IFactor.DesignatorFactor(); 
			Designator(out o.Value);
			f = o; 
		} else if (la.kind == 2 || la.kind == 3) {
			var o = new AOParser.Ast.IFactor.NumberFactor(); 
			Number(out o.Value);
			f = o; 
		} else if (la.kind == 4) {
			var o = new AOParser.Ast.IFactor.CharacterFactor(); 
			Get();
			o.Value = t.val; f = o; 
		} else if (la.kind == 5) {
			var o = new AOParser.Ast.IFactor.StringFactor(); 
			Get();
			o.Value = t.val; f = o; 
		} else if (la.kind == 59) {
			var o = new AOParser.Ast.IFactor.NilFactor(); 
			Get();
			f = o; 
		} else if (la.kind == 28) {
			var o = new AOParser.Ast.IFactor.SetFactor(); 
			Set(out o.Value);
			f = o; 
		} else if (la.kind == 26) {
			var o = new AOParser.Ast.IFactor.ExprFactor(); 
			Get();
			Expr(out o.Value);
			Expect(27);
			f = o; 
		} else if (la.kind == 24) {
			var o = new AOParser.Ast.IFactor.NegFactor(); 
			Get();
			Factor(out o.Value);
			f = o; 
		} else SynErr(82);
		f.Prefix = p; 
	}

	void MulOp(out AOParser.Ast.MulOp o) {
		o = new AOParser.Ast.MulOp(); 
		if (la.kind == 67) {
			Get();
			o.Op = AOParser.Ast.MulOp.MulOps.Mul; 
		} else if (la.kind == 68) {
			Get();
			o.Op = AOParser.Ast.MulOp.MulOps.Division; 
		} else if (la.kind == 69) {
			Get();
			o.Op = AOParser.Ast.MulOp.MulOps.DIV; 
		} else if (la.kind == 70) {
			Get();
			o.Op = AOParser.Ast.MulOp.MulOps.MOD; 
		} else if (la.kind == 23) {
			Get();
			o.Op = AOParser.Ast.MulOp.MulOps.AND; 
		} else SynErr(83);
	}

	void Set(out AOParser.Ast.Set o) {
		o = new AOParser.Ast.Set(); AOParser.Ast.Element e; 
		Expect(28);
		if (StartOf(3)) {
			Element(out e);
			o.Elements.Add(e); 
			while (la.kind == 12) {
				Get();
				Element(out e);
				o.Elements.Add(e); 
			}
		}
		Expect(29);
	}

	void Element(out AOParser.Ast.Element o) {
		o = new AOParser.Ast.Element(); 
		Expr(out o.Expr1);
		if (la.kind == 57) {
			Get();
			Expr(out o.Expr2);
		}
	}

	void ExprList(out AOParser.Ast.ExprList o) {
		o = new AOParser.Ast.ExprList(); AOParser.Ast.Expr e; 
		Expr(out e);
		o.Exprs.Add(e); 
		while (la.kind == 12) {
			Get();
			Expr(out e);
			o.Exprs.Add(e); 
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
		{_T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_T,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_T,_x, _T,_x,_x,_x, _x,_x,_x,_x, _T,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _T,_T,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_T,_T, _T,_T,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_x,_x,_T, _x,_T,_x,_T, _x,_T,_x,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_x, _T,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_T,_x, _x,_x,_x,_x}

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
			case 6: s = "\"SIZE\" expected"; break;
			case 7: s = "\"MODULE\" expected"; break;
			case 8: s = "\";\" expected"; break;
			case 9: s = "\".\" expected"; break;
			case 10: s = "\":=\" expected"; break;
			case 11: s = "\"IMPORT\" expected"; break;
			case 12: s = "\",\" expected"; break;
			case 13: s = "\"DEFINITION\" expected"; break;
			case 14: s = "\"REFINES\" expected"; break;
			case 15: s = "\"PROCEDURE\" expected"; break;
			case 16: s = "\"END\" expected"; break;
			case 17: s = "\"CONST\" expected"; break;
			case 18: s = "\"TYPE\" expected"; break;
			case 19: s = "\"VAR\" expected"; break;
			case 20: s = "\"=\" expected"; break;
			case 21: s = "\":\" expected"; break;
			case 22: s = "\"^\" expected"; break;
			case 23: s = "\"&\" expected"; break;
			case 24: s = "\"~\" expected"; break;
			case 25: s = "\"-\" expected"; break;
			case 26: s = "\"(\" expected"; break;
			case 27: s = "\")\" expected"; break;
			case 28: s = "\"{\" expected"; break;
			case 29: s = "\"}\" expected"; break;
			case 30: s = "\"ARRAY\" expected"; break;
			case 31: s = "\"OF\" expected"; break;
			case 32: s = "\"RECORD\" expected"; break;
			case 33: s = "\"POINTER\" expected"; break;
			case 34: s = "\"TO\" expected"; break;
			case 35: s = "\"OBJECT\" expected"; break;
			case 36: s = "\"IMPLEMENTS\" expected"; break;
			case 37: s = "\"ENUM\" expected"; break;
			case 38: s = "\"BEGIN\" expected"; break;
			case 39: s = "\"IF\" expected"; break;
			case 40: s = "\"THEN\" expected"; break;
			case 41: s = "\"ELSIF\" expected"; break;
			case 42: s = "\"ELSE\" expected"; break;
			case 43: s = "\"CASE\" expected"; break;
			case 44: s = "\"|\" expected"; break;
			case 45: s = "\"WHILE\" expected"; break;
			case 46: s = "\"DO\" expected"; break;
			case 47: s = "\"REPEAT\" expected"; break;
			case 48: s = "\"UNTIL\" expected"; break;
			case 49: s = "\"FOR\" expected"; break;
			case 50: s = "\"BY\" expected"; break;
			case 51: s = "\"LOOP\" expected"; break;
			case 52: s = "\"WITH\" expected"; break;
			case 53: s = "\"EXIT\" expected"; break;
			case 54: s = "\"RETURN\" expected"; break;
			case 55: s = "\"IGNORE\" expected"; break;
			case 56: s = "\"AWAIT\" expected"; break;
			case 57: s = "\"..\" expected"; break;
			case 58: s = "\"+\" expected"; break;
			case 59: s = "\"NIL\" expected"; break;
			case 60: s = "\"#\" expected"; break;
			case 61: s = "\"<\" expected"; break;
			case 62: s = "\"<=\" expected"; break;
			case 63: s = "\">\" expected"; break;
			case 64: s = "\">=\" expected"; break;
			case 65: s = "\"IN\" expected"; break;
			case 66: s = "\"IS\" expected"; break;
			case 67: s = "\"*\" expected"; break;
			case 68: s = "\"/\" expected"; break;
			case 69: s = "\"DIV\" expected"; break;
			case 70: s = "\"MOD\" expected"; break;
			case 71: s = "\"OR\" expected"; break;
			case 72: s = "\"[\" expected"; break;
			case 73: s = "\"]\" expected"; break;
			case 74: s = "??? expected"; break;
			case 75: s = "invalid Ident"; break;
			case 76: s = "invalid Number"; break;
			case 77: s = "invalid Body"; break;
			case 78: s = "invalid Type"; break;
			case 79: s = "invalid Statement"; break;
			case 80: s = "invalid Relation"; break;
			case 81: s = "invalid AddOp"; break;
			case 82: s = "invalid Factor"; break;
			case 83: s = "invalid MulOp"; break;

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
