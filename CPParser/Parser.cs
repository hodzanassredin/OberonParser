using CPParser;



using System;



public class Parser {
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _integer = 2;
	public const int _real = 3;
	public const int _character = 4;
	public const int _string = 5;
	public const int _colon = 6;
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

	bool FollowedByColon() { 
	  Token x = la; 
	  while (x.kind == _ident) 
		x = scanner.Peek(); 
	  return x.kind == _colon; 
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

	
	void CP() {
		Module();
	}

	void Module() {
		Expect(7);
		Ident(out builder.Module.Ident);
		Expect(8);
		if (la.kind == 14) {
			ImportList(out builder.Module.ImportList);
		}
		DeclSeq(out builder.Module.DeclSeq);
		if (la.kind == 9) {
			Get();
			StatementSeq(out builder.Module.Begin);
		}
		if (la.kind == 10) {
			Get();
			StatementSeq(out builder.Module.Close);
		}
		Expect(11);
		Expect(1);
		Expect(12);
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
		Expect(14);
		ImportedModule(o);
		while (la.kind == 15) {
			Get();
			ImportedModule(o);
		}
		Expect(8);
	}

	void DeclSeq(out CPParser.Ast.DeclSeq o) {
		o = new CPParser.Ast.DeclSeq(); 
		while (la.kind == 16 || la.kind == 17 || la.kind == 18) {
			if (la.kind == 16) {
				Get();
				var lst = new CPParser.Ast.IConstTypeVarListDecl.ConstDeclList(); 
				while (la.kind == 1) {
					ConstDecl(lst);
					Expect(8);
				}
				o.ConstTypeVarDecls.Add(lst); 
			} else if (la.kind == 17) {
				Get();
				var lst = new CPParser.Ast.IConstTypeVarListDecl.TypeDeclList(); 
				while (la.kind == 1) {
					TypeDecl(lst);
					Expect(8);
				}
				o.ConstTypeVarDecls.Add(lst); 
			} else {
				Get();
				var lst = new CPParser.Ast.IConstTypeVarListDecl.VarDeclList(); 
				while (la.kind == 1) {
					VarDecl(lst);
					Expect(8);
				}
				o.ConstTypeVarDecls.Add(lst); 
			}
		}
		while (la.kind == 19) {
			Get();
			if (la.kind == 1 || la.kind == 26) {
				ProcDecl(o.ProcForwardDecls);
			} else if (la.kind == 21) {
				ForwardDecl(o.ProcForwardDecls);
			} else SynErr(76);
			Expect(8);
		}
	}

	void StatementSeq(out CPParser.Ast.StatementSeq o) {
		o = new CPParser.Ast.StatementSeq(); CPParser.Ast.IStatement s; 
		Statement(out s);
		o.Statements.Add(s); 
		while (la.kind == 8) {
			Get();
			Statement(out s);
			o.Statements.Add(s); 
		}
	}

	void ImportedModule(CPParser.Ast.AstList i) {
		var o = new CPParser.Ast.Import(); 
		Ident(out o.Name);
		if (la.kind == 13) {
			Get();
			Ident(out o.OriginalName);
		}
		i.Add(o); 
	}

	void ConstDecl(CPParser.Ast.AstList lst) {
		var o = new CPParser.Ast.ConstDecl(); 
		IdentDef(out o.IdentDef);
		Expect(20);
		ConstExpr(out o.ConstExpr);
		lst.Add(o); 
	}

	void TypeDecl(CPParser.Ast.AstList lst) {
		var o = new CPParser.Ast.TypeDecl(); 
		IdentDef(out o.IdentDef);
		Expect(20);
		Type(out o.Type_);
		lst.Add(o); 
	}

	void VarDecl(CPParser.Ast.AstList lst) {
		var o = new CPParser.Ast.VarDecl(); 
		IdentList(out o.IdentList);
		Expect(6);
		Type(out o.Type_);
		lst.Add(o); 
	}

	void ProcDecl(CPParser.Ast.AstList lst) {
		var o = new CPParser.Ast.ProcDecl(); 
		if (la.kind == 26) {
			Receiver(out o.Receiver);
		}
		IdentDef(out o.IdentDef);
		if (la.kind == 26) {
			FormalPars(out o.FormalPars);
		}
		MethAttributes(out o.MethAttributes);
		if (la.kind == 8) {
			Get();
			DeclSeq(out o.DeclSeq);
			if (la.kind == 9) {
				Get();
				StatementSeq(out o.StatementSeq);
			}
			Expect(11);
			Expect(1);
		}
		lst.Add(o); 
	}

	void ForwardDecl(CPParser.Ast.AstList lst) {
		var o = new CPParser.Ast.ForwardDecl(); 
		Expect(21);
		if (la.kind == 26) {
			Receiver(out o.Receiver);
		}
		IdentDef(out o.IdentDef);
		if (la.kind == 26) {
			FormalPars(out o.FormalPars);
		}
		MethAttributes(out o.MethAttributes);
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

	void ConstExpr(out CPParser.Ast.ConstExpr o) {
		o = new CPParser.Ast.ConstExpr(); 
		Expr(out o.Expr);
	}

	void Type(out CPParser.Ast.IType o) {
		o = null; 
		if (la.kind == 1) {
			var at = new CPParser.Ast.IType.SynonimType(); 
			Qualident(out at.Qualident);
			o = at; 
		} else if (la.kind == 30) {
			Get();
			var at = new CPParser.Ast.IType.ArrayType(); CPParser.Ast.ConstExpr ce; 
			if (StartOf(1)) {
				ConstExpr(out ce);
				at.ConstExprs.Add(ce); 
				while (la.kind == 15) {
					Get();
					ConstExpr(out ce);
					at.ConstExprs.Add(ce); 
				}
			}
			Expect(31);
			Type(out at.Type_);
			o = at; 
		} else if (StartOf(2)) {
			var at = new CPParser.Ast.IType.RecordType(); 
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
				Qualident(out at.Qualident);
				Expect(27);
			}
			var fl = new CPParser.Ast.FieldList(); 
			FieldList(out fl);
			at.FieldList.Add(fl); 
			while (la.kind == 8) {
				Get();
				FieldList(out fl);
				at.FieldList.Add(fl); 
			}
			Expect(11);
			o = at; 
		} else if (la.kind == 34) {
			var at = new CPParser.Ast.IType.PointerType(); 
			Get();
			Expect(35);
			Type(out at.Type_);
			o = at; 
		} else if (la.kind == 19) {
			var at = new CPParser.Ast.IType.ProcedureType(); 
			Get();
			if (la.kind == 26) {
				FormalPars(out at.FormalPars);
			}
			o = at; 
		} else SynErr(77);
	}

	void IdentList(out CPParser.Ast.IdentList o) {
		o = new CPParser.Ast.IdentList(); var id = new CPParser.Ast.IdentDef(); 
		IdentDef(out id);
		o.IdentDefs.Add(id); 
		while (la.kind == 15) {
			Get();
			id = new CPParser.Ast.IdentDef(); 
			IdentDef(out id);
			o.IdentDefs.Add(id); 
		}
	}

	void Receiver(out CPParser.Ast.Receiver o) {
		o = new CPParser.Ast.Receiver(); 
		Expect(26);
		if (la.kind == 18 || la.kind == 28) {
			if (la.kind == 18) {
				Get();
				o.ReceiverPrefix = CPParser.Ast.Receiver.Prefix.VAR; 
			} else {
				Get();
				o.ReceiverPrefix = CPParser.Ast.Receiver.Prefix.IN; 
			}
		}
		Ident(out o.SelfIdent);
		Expect(6);
		Ident(out o.TypeIdent);
		Expect(27);
	}

	void FormalPars(out CPParser.Ast.FormalPars o) {
		o = new CPParser.Ast.FormalPars(); 
		Expect(26);
		if (StartOf(3)) {
			FPSection(o.FPSections);
			while (la.kind == 8) {
				Get();
				FPSection(o.FPSections);
			}
		}
		Expect(27);
		if (la.kind == 6) {
			Get();
			Type(out o.Type_);
		}
	}

	void MethAttributes(out CPParser.Ast.MethAttributes o) {
		o = new CPParser.Ast.MethAttributes(); 
		if (la.kind == 15) {
			Get();
			if (la.kind == 25) {
				NewMethAttr(o);
				Expect(15);
				MethAttr(out o.Attr);
			} else if (la.kind == 22 || la.kind == 23 || la.kind == 24) {
				MethAttr(out o.Attr);
			} else SynErr(78);
		}
	}

	void MethAttr(out CPParser.Ast.MethAttributes.MethodAttr? o) {
		o = null; 
		if (la.kind == 22) {
			Get();
			o = CPParser.Ast.MethAttributes.MethodAttr.ABSTRACT; 
		} else if (la.kind == 23) {
			Get();
			o = CPParser.Ast.MethAttributes.MethodAttr.EMPTY; 
		} else if (la.kind == 24) {
			Get();
			o = CPParser.Ast.MethAttributes.MethodAttr.EXTENSIBLE; 
		} else SynErr(79);
	}

	void NewMethAttr(CPParser.Ast.MethAttributes o) {
		Expect(25);
		o.IsNew = true; 
	}

	void FPSection(CPParser.Ast.AstList lst) {
		var o = new CPParser.Ast.FPSection(); 
		if (la.kind == 18 || la.kind == 28 || la.kind == 29) {
			if (la.kind == 18) {
				Get();
				o.FpSectionPrefix = CPParser.Ast.FPSection.Prefix.VAR; 
			} else if (la.kind == 28) {
				Get();
				o.FpSectionPrefix = CPParser.Ast.FPSection.Prefix.IN; 
			} else {
				Get();
				o.FpSectionPrefix = CPParser.Ast.FPSection.Prefix.OUT; 
			}
		}
		CPParser.Ast.Ident i; 
		Ident(out i);
		o.Idents.Add(i); 
		while (la.kind == 15) {
			Get();
			Ident(out i);
			o.Idents.Add(i); 
		}
		Expect(6);
		Type(out o.Type_);
		lst.Add(o); 
	}

	void Qualident(out CPParser.Ast.Qualident o) {
		o = new CPParser.Ast.Qualident(); 
		Ident(out o.Ident1);
		if (la.kind == 12) {
			Get();
			Ident(out o.Ident2);
		}
	}

	void FieldList(out CPParser.Ast.FieldList o) {
		o = new CPParser.Ast.FieldList(); 
		if (la.kind == 1) {
			IdentList(out o.IdentList);
			Expect(6);
			Type(out o.Type_);
		}
	}

	void Statement(out CPParser.Ast.IStatement os) {
		os=null; 
		if (StartOf(4)) {
			switch (la.kind) {
			case 1: {
				CPParser.Ast.Designator d; 
				Designator(out d);
				if (la.kind == 13) {
					var o = new CPParser.Ast.IStatement.AssignmentStatement(); o.Designator = d; 
					Get();
					Expr(out o.Expr);
					os = o;
				} else if (StartOf(5)) {
					var o = new CPParser.Ast.IStatement.ProcCallStatement(); o.Designator = d; 
					if (la.kind == 26) {
						Get();
						if (StartOf(1)) {
							ExprList(out o.ExprList);
						}
						Expect(27);
					}
					os = o;
				} else SynErr(80);
				break;
			}
			case 37: {
				var o = new CPParser.Ast.IStatement.IfStatement(); 
				Get();
				Expr(out o.If.Cond);
				Expect(38);
				StatementSeq(out o.If.ThenBody);
				while (la.kind == 39) {
					var elsif = new CPParser.Ast.IStatement.IfStatement.IfThen(); 
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
				Expect(11);
				os = o; 
				break;
			}
			case 41: {
				var o = new CPParser.Ast.IStatement.CaseStatement(); 
				Get();
				Expr(out o.Expr);
				Expect(31);
				var c = new CPParser.Ast.Case(); 
				Case(out c);
				o.Cases.Add(c); 
				while (la.kind == 42) {
					Get();
					Case(out c);
					o.Cases.Add(c); 
				}
				if (la.kind == 40) {
					Get();
					StatementSeq(out o.ElseBody);
				}
				Expect(11);
				os = o;
				break;
			}
			case 43: {
				var o = new CPParser.Ast.IStatement.WhileStatement(); 
				Get();
				Expr(out o.Expr);
				Expect(36);
				StatementSeq(out o.StatementSeq);
				Expect(11);
				os = o; 
				break;
			}
			case 44: {
				var o = new CPParser.Ast.IStatement.RepeatStatement(); 
				Get();
				StatementSeq(out o.StatementSeq);
				Expect(45);
				Expr(out o.Expr);
				os = o; 
				break;
			}
			case 46: {
				var o = new CPParser.Ast.IStatement.ForStatement(); 
				Get();
				Ident(out o.Ident);
				Expect(13);
				Expr(out o.Expr);
				Expect(35);
				Expr(out o.ToExpr);
				if (la.kind == 47) {
					Get();
					ConstExpr(out o.ByExpr);
				}
				Expect(36);
				StatementSeq(out o.StatementSeq);
				Expect(11);
				os = o;
				break;
			}
			case 48: {
				var o = new CPParser.Ast.IStatement.LoopStatement(); 
				Get();
				StatementSeq(out o.StatementSeq);
				Expect(11);
				os = o;
				break;
			}
			case 49: {
				var o = new CPParser.Ast.IStatement.WithStatement(); CPParser.Ast.IStatement.WithAlternativeStatement alt; 
				Get();
				WithAlt(out alt);
				o.Alternatives.Add(alt); 
				while (la.kind == 42) {
					Get();
					WithAlt(out alt);
					o.Alternatives.Add(alt); 
				}
				if (la.kind == 40) {
					Get();
					StatementSeq(out o.ElseStatementSeq);
				}
				Expect(11);
				os = o;
				break;
			}
			case 50: {
				var o = new CPParser.Ast.IStatement.ExitStatement(); 
				Get();
				os = o;
				break;
			}
			case 51: {
				var o = new CPParser.Ast.IStatement.ReturnStatement(); 
				Get();
				if (StartOf(1)) {
					Expr(out o.Expr);
				}
				os = o;
				break;
			}
			}
		}
	}

	void WithAlt(out CPParser.Ast.IStatement.WithAlternativeStatement o) {
		o = new CPParser.Ast.IStatement.WithAlternativeStatement(); 
		if (la.kind == 1) {
			Guard(out o.Guard);
			Expect(36);
			StatementSeq(out o.StatementSeq);
		}
	}

	void Guard(out CPParser.Ast.Guard o) {
		o = new CPParser.Ast.Guard(); 
		Qualident(out o.VarQualident);
		Expect(6);
		Qualident(out o.TypeQualident);
	}

	void Designator(out CPParser.Ast.Designator o) {
		o = new CPParser.Ast.Designator(); 
		Qualident(out o.Qualident);
		while (StartOf(6)) {
			if (la.kind == 12) {
				Get();
				var s = new CPParser.Ast.Designator.IDesignatorSpec.RecordDesignatorSpec(); 
				Ident(out s.Value);
				o.Specs.Add(s); 
			} else if (la.kind == 71) {
				Get();
				var s = new CPParser.Ast.Designator.IDesignatorSpec.ArrayDesignatorSpec(); 
				ExprList(out s.Value);
				Expect(72);
				o.Specs.Add(s); 
			} else if (la.kind == 21) {
				Get();
				var s = new CPParser.Ast.Designator.IDesignatorSpec.PointerDesignatorSpec(); 
				o.Specs.Add(s); 
			} else {
				Get();
				if (la.kind == 1) {
					var s = new CPParser.Ast.Designator.IDesignatorSpec.CastDesignatorSpec(); 
					Qualident(out s.Value);
					o.Specs.Add(s); 
				} else if (StartOf(7)) {
					var s = new CPParser.Ast.Designator.IDesignatorSpec.ProcCallDesignatorSpec(); 
					if (StartOf(1)) {
						ExprList(out s.Value);
					}
					o.Specs.Add(s); 
				} else SynErr(81);
				Expect(27);
			}
		}
		if (la.kind == 73) {
			Get();
			o.EndOfLine = true; 
		}
	}

	void Expr(out CPParser.Ast.Expr o) {
		o = new CPParser.Ast.Expr(); 
		SimpleExpr(out o.SimpleExpr);
		if (StartOf(8)) {
			Relation(out o.Relation);
			SimpleExpr(out o.SimpleExpr2);
		}
	}

	void ExprList(out CPParser.Ast.ExprList o) {
		o = new CPParser.Ast.ExprList(); CPParser.Ast.Expr e; 
		Expr(out e);
		o.Exprs.Add(e); 
		while (la.kind == 15) {
			Get();
			Expr(out e);
			o.Exprs.Add(e); 
		}
	}

	void Case(out CPParser.Ast.Case o) {
		o = new CPParser.Ast.Case(); CPParser.Ast.CaseLabels cl; 
		if (StartOf(1)) {
			CaseLabels(out cl);
			o.CaseLabels.Add(cl); 
			while (la.kind == 15) {
				Get();
				CaseLabels(out cl);
				o.CaseLabels.Add(cl); 
			}
			Expect(6);
			StatementSeq(out o.StatementSeq);
		}
	}

	void CaseLabels(out CPParser.Ast.CaseLabels o) {
		o = new CPParser.Ast.CaseLabels(); 
		ConstExpr(out o.ConstExpr1);
		if (la.kind == 52) {
			Get();
			ConstExpr(out o.ConstExpr2);
		}
	}

	void SimpleExpr(out CPParser.Ast.SimpleExpr o) {
		o = new CPParser.Ast.SimpleExpr(); CPParser.Ast.SimpleElementExpr e; 
		if (la.kind == 53 || la.kind == 54) {
			if (la.kind == 53) {
				Get();
			} else {
				Get();
			}
		}
		Term(out o.Term);
		while (la.kind == 53 || la.kind == 54 || la.kind == 65) {
			e = new CPParser.Ast.SimpleElementExpr(); 
			AddOp(out e.AddOp);
			Term(out e.Term);
			o.SimpleExprElements.Add(e); 
		}
	}

	void Relation(out CPParser.Ast.Relation o) {
		o = null; 
		switch (la.kind) {
		case 20: {
			o = new CPParser.Ast.Relation(); 
			Get();
			o.Op = CPParser.Ast.Relation.Relations.Eq ; 
			break;
		}
		case 59: {
			Get();
			o.Op = CPParser.Ast.Relation.Relations.Neq ; 
			break;
		}
		case 60: {
			Get();
			o.Op = CPParser.Ast.Relation.Relations.Lss ; 
			break;
		}
		case 61: {
			Get();
			o.Op = CPParser.Ast.Relation.Relations.Leq ; 
			break;
		}
		case 62: {
			Get();
			o.Op = CPParser.Ast.Relation.Relations.Gtr ; 
			break;
		}
		case 63: {
			Get();
			o.Op = CPParser.Ast.Relation.Relations.Geq ; 
			break;
		}
		case 28: {
			Get();
			o.Op = CPParser.Ast.Relation.Relations.In ; 
			break;
		}
		case 64: {
			Get();
			o.Op = CPParser.Ast.Relation.Relations.Is ; 
			break;
		}
		default: SynErr(82); break;
		}
	}

	void Term(out CPParser.Ast.Term o) {
		o = new CPParser.Ast.Term(); CPParser.Ast.TermElementExpr e; 
		Factor(out o.Factor);
		while (StartOf(9)) {
			e = new CPParser.Ast.TermElementExpr(); 
			MulOp(out e.MulOp);
			Factor(out e.Factor);
			o.TermElements.Add(e); 
		}
	}

	void AddOp(out CPParser.Ast.AddOp o) {
		o = new CPParser.Ast.AddOp(); 
		if (la.kind == 53) {
			Get();
			o.Op = CPParser.Ast.AddOp.AddOps.Add; 
		} else if (la.kind == 54) {
			Get();
			o.Op = CPParser.Ast.AddOp.AddOps.Sub; 
		} else if (la.kind == 65) {
			Get();
			o.Op = CPParser.Ast.AddOp.AddOps.Or; 
		} else SynErr(83);
	}

	void Factor(out CPParser.Ast.IFactor f) {
		f = null;
		switch (la.kind) {
		case 1: {
			var o = new CPParser.Ast.IFactor.DesignatorFactor(); 
			Designator(out o.Value);
			f = o; 
			break;
		}
		case 2: case 3: {
			var o = new CPParser.Ast.IFactor.NumberFactor(); 
			number();
			f = o; 
			break;
		}
		case 4: {
			var o = new CPParser.Ast.IFactor.CharacterFactor(); 
			Get();
			f = o; 
			break;
		}
		case 5: {
			var o = new CPParser.Ast.IFactor.StringFactor(); 
			Get();
			f = o; 
			break;
		}
		case 55: {
			var o = new CPParser.Ast.IFactor.NilFactor(); 
			Get();
			f = o; 
			break;
		}
		case 57: {
			var o = new CPParser.Ast.IFactor.SetFactor(); 
			Set(out o.Value);
			f = o; 
			break;
		}
		case 26: {
			var o = new CPParser.Ast.IFactor.ExprFactor(); 
			Get();
			Expr(out o.Value);
			Expect(27);
			f = o; 
			break;
		}
		case 56: {
			var o = new CPParser.Ast.IFactor.NegFactor(); 
			Get();
			Factor(out o.Value);
			f = o; 
			break;
		}
		default: SynErr(84); break;
		}
	}

	void MulOp(out CPParser.Ast.MulOp o) {
		o = new CPParser.Ast.MulOp(); 
		if (la.kind == 66) {
			Get();
			o.Op = CPParser.Ast.MulOp.MulOps.Mul; 
		} else if (la.kind == 67) {
			Get();
			o.Op = CPParser.Ast.MulOp.MulOps.Division; 
		} else if (la.kind == 68) {
			Get();
			o.Op = CPParser.Ast.MulOp.MulOps.DIV; 
		} else if (la.kind == 69) {
			Get();
			o.Op = CPParser.Ast.MulOp.MulOps.MOD; 
		} else if (la.kind == 70) {
			Get();
			o.Op = CPParser.Ast.MulOp.MulOps.AND; 
		} else SynErr(85);
	}

	void Set(out CPParser.Ast.Set o) {
		o = new CPParser.Ast.Set(); CPParser.Ast.Element e; 
		Expect(57);
		if (StartOf(1)) {
			Element(out e);
			o.Elements.Add(e); 
			while (la.kind == 15) {
				Get();
				Element(out e);
				o.Elements.Add(e); 
			}
		}
		Expect(58);
	}

	void Element(out CPParser.Ast.Element o) {
		o = new CPParser.Ast.Element(); 
		Expr(out o.Expr1);
		if (la.kind == 52) {
			Get();
			Expr(out o.Expr2);
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
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _T,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_T,_x,_T, _T,_x,_T,_x, _T,_T,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_x,_T,_x, _x,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_T,_x,_x, _x,_x,_T,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _x,_x,_x,_x},
		{_x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_T,_T, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_T,_T,_T, _T,_T,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
		{_x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_T, _T,_T,_T,_T, _T,_x,_x,_x, _x,_x,_x,_x, _x,_x,_x,_x},
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
			case 6: s = "colon expected"; break;
			case 7: s = "\"MODULE\" expected"; break;
			case 8: s = "\";\" expected"; break;
			case 9: s = "\"BEGIN\" expected"; break;
			case 10: s = "\"CLOSE\" expected"; break;
			case 11: s = "\"END\" expected"; break;
			case 12: s = "\".\" expected"; break;
			case 13: s = "\":=\" expected"; break;
			case 14: s = "\"IMPORT\" expected"; break;
			case 15: s = "\",\" expected"; break;
			case 16: s = "\"CONST\" expected"; break;
			case 17: s = "\"TYPE\" expected"; break;
			case 18: s = "\"VAR\" expected"; break;
			case 19: s = "\"PROCEDURE\" expected"; break;
			case 20: s = "\"=\" expected"; break;
			case 21: s = "\"^\" expected"; break;
			case 22: s = "\"ABSTRACT\" expected"; break;
			case 23: s = "\"EMPTY\" expected"; break;
			case 24: s = "\"EXTENSIBLE\" expected"; break;
			case 25: s = "\"NEW\" expected"; break;
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
			case 36: s = "\"DO\" expected"; break;
			case 37: s = "\"IF\" expected"; break;
			case 38: s = "\"THEN\" expected"; break;
			case 39: s = "\"ELSIF\" expected"; break;
			case 40: s = "\"ELSE\" expected"; break;
			case 41: s = "\"CASE\" expected"; break;
			case 42: s = "\"|\" expected"; break;
			case 43: s = "\"WHILE\" expected"; break;
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
			case 76: s = "invalid DeclSeq"; break;
			case 77: s = "invalid Type"; break;
			case 78: s = "invalid MethAttributes"; break;
			case 79: s = "invalid MethAttr"; break;
			case 80: s = "invalid Statement"; break;
			case 81: s = "invalid Designator"; break;
			case 82: s = "invalid Relation"; break;
			case 83: s = "invalid AddOp"; break;
			case 84: s = "invalid Factor"; break;
			case 85: s = "invalid MulOp"; break;

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
