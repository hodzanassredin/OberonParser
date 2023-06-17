using AOParser.Types;
using Common.SymTable;
using System.Collections;
using System.Text;

namespace AOParser.Ast
{

	public abstract class AstElement
	{
		public AstList CommentsBefore = new AstList();
		public void AcceptWithComments(IAstVisitor v)
		{
			foreach (var c in CommentsBefore)
			{
				c.Accept(v);
			}
			this.Accept(v);
			foreach (var c in CommentsAfter)
			{
				c.Accept(v);
			}
		}
		public abstract void Accept(IAstVisitor v);
		public AstList CommentsAfter = new AstList();
	}

	public class AstList : IEnumerable<AstElement>
	{
		public List<AstElement> Value { get; set; } = new List<AstElement>();

        public void Add(AstElement obj)
        {
			if (obj != null)
			{
				Value.Add(obj);
			}
        }
		public List<T> Cast<T>()
			where T : AstElement
		{
			return Value.Cast<T>().ToList();
		}

        public IEnumerator<AstElement> GetEnumerator()
        {
			return Value.GetEnumerator();

		}

        IEnumerator IEnumerable.GetEnumerator()
        {
			return Value.GetEnumerator();
		}

        public override string ToString()
        {
			var sb = new StringBuilder();
			foreach (var item in Value) { 
				sb.Append(item.ToString());
			}	
            return sb.ToString();
        }
    }

	public class Ident : AstElement
	{
		public string Name { get; set; }

        public override void Accept(IAstVisitor v) => v.Visit(this);
        public override string ToString()
        {
            return Name;
        }
    }

	public class Qualident : AstElement
	{
		Dictionary<string, TypeDesc> baseTypes = new Dictionary<string, TypeDesc>
		{
			["BOOLEAN"] = TypeDesc.BOOL,
			["CHAR"] = TypeDesc.CHAR8,
			["SIGNED8"] = TypeDesc.INT8,
			["SIGNED16"] = TypeDesc.INT16,
			["SIGNED32"] = TypeDesc.INT32,
			["SIGNED64"] = TypeDesc.INT64,
			["UNSIGNED8"] = TypeDesc.UINT8,
			["UNSIGNED16"] = TypeDesc.UINT16,
			["UNSIGNED32"] = TypeDesc.UINT32,
			["UNSIGNED64"] = TypeDesc.UINT64,
			["FLOAT32"] = TypeDesc.FLOAT32,
			["FLOAT64"] = TypeDesc.FLOAT64,
			["REAL"] = TypeDesc.FLOAT64,
			["INTEGER"] = TypeDesc.INT32,
		};

		public bool IsSelf => Ident2 == null && scope.IsSelf(Ident1.Name);
        public Qualident(Scope scope)
        {
			this.scope = scope;

		}
		public Ident Ident1;
		public Ident Ident2;
        public override void Accept(IAstVisitor v) => v.Visit(this);
        public override string ToString()
        {
            return Ident2 != null ? $"{Ident1}.{Ident2}" : Ident1.ToString();
        }
		public Scope scope;

		public TypeDesc TypeDescr
		{ get
			{
				var name = ToString();
				if (baseTypes.ContainsKey(name)) {
					return baseTypes[name];
				}
				return TypeDesc.Predefined(ToString(), scope);
			}
		}
	}
	public class Guard : AstElement
	{
		public Qualident VarQualident;
		public Qualident TypeQualident;
		
		public override void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return $"{VarQualident}:{TypeQualident}";
		}
	}
	public class DefinitionProc : AstElement {
		public Ident Ident;
		public FormalPars FormalPars;

        public override void Accept(IAstVisitor v) => v.Visit(this);
        public override string ToString()
        {
            return $"PROCEDURE {Ident} {FormalPars}";
        }
    }
	

	public class FieldDecl : AstElement
	{
		public Common.SymTable.Obj[] Fields => IdentList.GetNames().Select(name => new Obj(ObjCLass.VAR, name, Type_.TypeDescr, "")).ToArray();
		public IdentList IdentList;
		public IType Type_;
		public override void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return $"{IdentList} : {Type_}";
		}
	}
	public class ProcHead : AstElement
	{

		public Flags Flags;
        public IdentDef IdentDef;
		public FormalPars FormalPars;
		public String Tag;
		public override void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return $"{Flags} {Tag} {IdentDef} {FormalPars}";
		}
	}

	public class Flag : AstElement
	{
		public Ident Ident;
        public Expr QualExpr;
        public Expr AssignExpr;

        public override void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append(Ident);
			if (QualExpr != null) {
				sb.Append($"({QualExpr})");
			}
			if (AssignExpr != null)
			{
				sb.Append($" = {AssignExpr}");
			}

			return sb.ToString();
		}
	}

	public class Flags : AstElement
	{
		public AstList Values = new();
		public override void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return $"{{{String.Join(",", Values)}}}";
		}
	}
	public class StatBlock : AstElement
	{
		public Flags Flags;
		public StatementSeq StatementSeq;

		public override void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return $"BEGIN {Flags} {StatementSeq} END";
		}
	}
	public class Body : AstElement
	{
		public StatBlock StatBlock;
		public override void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return StatBlock == null ? "END" : StatBlock.ToString();
		}
	}

	public class Definition : AstElement
	{
		public Ident Ident;
		public Qualident Qualident;
		public AstList Procs = new AstList();
		public override void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return $"DEFINITION {Ident} [REFINES{Qualident}] {Procs}";
		}
	}
	public class Module : AstElement
	{
		public void SetDefaultScope(SymTab tab) {
			tab.InsertFunc("CHR", TypeDesc.CHAR16, TypeDesc.None);
		}

		public Ident Ident;
		public AstList ImportList;
		public Definition Definition;
		public DeclSeq DeclSeq = new DeclSeq();
		public Body Body;
		public override void Accept(IAstVisitor v) => v.Visit(this);

		public override string ToString()
		{
			return $"MODULE {Ident}";
		}
	}

	public class Import : AstElement
	{
		public Ident Name;
		public Ident OriginalName;
		public override void Accept(IAstVisitor v) => v.Visit(this);

        public Obj GetObj()
        {
			return new Obj(ObjCLass.MODULE, Name.Name, TypeDesc.None, null);
		}

        public override string ToString()
		{
			return $"{Name} : {OriginalName}";
		}
	}

	public class IdentDef : AstElement
	{
		public enum IdentExport
		{
			ExportReadonly,
			Export
		}
		public Ident Ident;
		public IdentExport? Export;
		public override void Accept(IAstVisitor v) => v.Visit(this);

        public override string ToString()
        {
            return $"{Ident} {Export}";
        }
    }
	public abstract class IConstTypeVarListDecl : AstElement {
		public AstList Values = new AstList();
		public class ConstDeclList : IConstTypeVarListDecl
		{
			public override void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class TypeDeclList : IConstTypeVarListDecl
		{
			public override void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class VarDeclList : IConstTypeVarListDecl
		{
			public override void Accept(IAstVisitor v) => v.Visit(this);
		}
	}


	public class DeclSeq : AstElement
	{
		public AstList ConstTypeVarDecls = new ();
		public AstList ProcDecl = new();
		public override void Accept(IAstVisitor v) => v.Visit(this);

        public override string ToString()
        {
			return $"{String.Join(Environment.NewLine, ConstTypeVarDecls)} {Environment.NewLine}{String.Join(Environment.NewLine, ProcDecl)}";
        }
    }


	public class ConstDecl : AstElement
	{
		public IdentDef IdentDef;
		public ConstExpr ConstExpr;
		public override void Accept(IAstVisitor v) => v.Visit(this);

        public Obj GetObj()
        {
			return new Obj(ObjCLass.CONST, IdentDef.Ident.Name, ConstExpr.TypeDescr, ConstExpr.ToString());
		}

        public override string ToString()
        {
            return $"{IdentDef} = {ConstExpr}";
        }
    }
	public class TypeDecl : AstElement
	{
		public IdentDef IdentDef;
		public IType Type_;
        public override void Accept(IAstVisitor v) => v.Visit(this);

        public override string ToString()
        {
            return $"{IdentDef} = {Type_}";
		}

		public Obj GetObj()
		{
			return new Obj(ObjCLass.TYPE, IdentDef.Ident.Name, Type_.TypeDescr, null);
		}
	}
	public class VarDecl : AstElement
	{
		public IdentList IdentList;
		public IType Type_;
        public override void Accept(IAstVisitor v) => v.Visit(this);

		public override string ToString()
		{
			return $"{IdentList} = {Type_}";
		}

		public IEnumerable<Obj> GetObjects()
		{
			return IdentList.GetNames().Select(name => new Obj(ObjCLass.VAR, name, Type_.TypeDescr, null));
		}
	}

	public class ProcDecl : AstElement
	{
		public ProcHead ProcHead;
		public DeclSeq DeclSeq;
		public Body Body;
		public Ident Ident;
		public override void Accept(IAstVisitor v) => v.Visit(this);
        public override string ToString()
        {
            return $"PROCEDURE {ProcHead}; {DeclSeq} {Body} {Ident}";
        }

		public Obj GetObj(Scope scope)
		{
			return new Obj(ObjCLass.FUNC, ProcHead.IdentDef.Ident.Name, ProcHead.FormalPars?.TypeDescr(scope) ?? TypeDesc.Function(TypeDesc.None, null, scope), "")
			{
				scope = scope,
			};
		}
	}

	public class FormalPars : AstElement
	{
		public AstList FPSections = new AstList();
		public Qualident Qualident;
        public override void Accept(IAstVisitor v) => v.Visit(this);

		public override string ToString()
		{
			var str = Qualident != null ? $":{Qualident}" :"";
			return $"({String.Join(";", FPSections)} ){str}";
		}

		public TypeDesc TypeDescr(Scope scope)
		{
			var args = new List<Obj>();
            foreach (var sect in FPSections.Cast<FPSection>())
            {
                foreach (var item in sect.Idents.Value.Cast<Ident>())
                {
					args.Add(scope.Find(item.Name));

				}
				
            }
			return TypeDesc.Function(Qualident?.TypeDescr ?? TypeDesc.None, args.ToArray(), scope);
		}
	}

	public class FPSection : AstElement
	{
		public enum Prefix
		{
			VAR, CONST
		}
		public Prefix? FpSectionPrefix;
		public AstList Idents = new AstList();
		public IType Type_;
		public override void Accept(IAstVisitor v) => v.Visit(this);
        public override string ToString()
        {
            return $"{FpSectionPrefix} {String.Join(", ",Idents)}:{Type_}";
        }

		public IEnumerable<Obj> Objects()
		{

			foreach (var id in Idents.Cast<Ident>())
			{
				yield return new Obj(ObjCLass.PARAM, id.Name, Type_.TypeDescr, "");
			}
		}
	}
	
    public abstract class IType : AstElement
    {
		public abstract Common.SymTable.TypeDesc TypeDescr { get; }
		public class SynonimType : IType
		{
			public override TypeDesc TypeDescr => Qualident.TypeDescr;
			public Qualident? Qualident;
			public override void Accept(IAstVisitor v) => v.Visit(this);
            public override string ToString()
            {
                return Qualident?.ToString()??"";
            }
        }
		public class ArrayType : IType
		{
			public override TypeDesc TypeDescr
			{
				get
				{
					var sizes = ConstExprs.Cast<ConstExpr>().Count;//.Select(x => Int32.Parse(x.ToString())).ToArray();
					return TypeDesc.Array(Type_.TypeDescr, new int[sizes]);
				}
			}

			public AstList ConstExprs = new AstList();
			public IType Type_;
			public override void Accept(IAstVisitor v) => v.Visit(this);
            public override string ToString()
            {
                return $"ARRAY {String.Join(",", ConstExprs)} OF {Type_}";
            }

        }

		public class RecordType : IType
		{
            public RecordType(Scope scope)
            {
                this.scope = scope;
            }
			public Qualident Qualident;
			public FieldList FieldList;
            private readonly Scope scope;

            public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"RECORD ({Qualident}) {FieldList} END";
			}

			public override TypeDesc TypeDescr
			{
				get
				{
					return TypeDesc.Struct(Qualident?.TypeDescr, scope);
				}
			}
		}

		public class EnumType : IType
		{
			public EnumType(Scope scope)
			{
				this.scope = scope;
			}
			public Qualident Qualident;
			public AstList Enums = new AstList();
			private readonly Scope scope;

			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"ENUM ({Qualident}) {Enums} END";
			}

			public override TypeDesc TypeDescr
			{
				get
				{
					return TypeDesc.Enum(Qualident?.TypeDescr, scope);
				}
			}
		}

		public class ObjectType : IType
		{
            public ObjectType(Scope scope)
            {
                this.scope = scope;
            }
			public Flags Flags;
			public Qualident Qualident;
			public Qualident ImplementsQualident;
			public DeclSeq DeclSeq;
			public Body Body;

			public Ident Ident;
            private readonly Scope scope;

            public override void Accept(IAstVisitor v) => v.Visit(this);

            public override string ToString()
            {
				var impl = ImplementsQualident != null? $"IMPLEMENTS {ImplementsQualident}" : "";

				return $"OBJECT {Flags}({Qualident}) {impl} {DeclSeq} {Body} {Ident}";
            }

			public override TypeDesc TypeDescr
			{
				get
				{

					return TypeDesc.Pointer(TypeDesc.Struct(Qualident?.TypeDescr, scope));

				}
			}

		}
		public class PointerType : IType
		{
			public override TypeDesc TypeDescr => TypeDesc.Pointer(Type_.TypeDescr);

			public Flags Flags;
			public IType Type_;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"POINTER {Flags} TO {Type_}";
			}
		}
		public class ProcedureType : IType
		{
            public ProcedureType(Scope scope)
			{
                this.scope = scope;
            }
			public override TypeDesc TypeDescr => FormalPars.TypeDescr(scope);
			public Flags Flags;
			public FormalPars FormalPars;
            private readonly Scope scope;

            public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"PROCEDURE {Flags} {FormalPars}";
			}
		}
	}

    public class EnumItem : AstElement
    {
		public IdentDef IdentDef;
		public Expr Expr;

		public override void Accept(IAstVisitor v)
        {
			v.Visit(this);
        }
    }
    public class ExprList : AstElement {
		public AstList Exprs = new AstList();
		public override void Accept(IAstVisitor v) => v.Visit(this);

		public override string ToString()
		{
			return String.Join(',', Exprs);
		}
	}
	public class IdentList : AstElement
	{
		public string[] GetNames()
		{
			return IdentDefs.Cast<IdentDef>().Select(x => x.Ident.Name).ToArray();
		}

		public AstList IdentDefs = new AstList();
		public override void Accept(IAstVisitor v) => v.Visit(this);
        public override string ToString()
        {
            return String.Join(",", IdentDefs);
        }
    }
	public class FieldList : AstElement
	{
		public AstList FieldDecl = new AstList();
		public override void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return String.Join(";", FieldDecl);
		}
	}
	
	
	public class ConstExpr : AstElement {
		public Expr Expr;
		public Common.SymTable.TypeDesc TypeDescr => Expr.TypeDescr;
		public override void Accept(IAstVisitor v) => v.Visit(this);
        public override string ToString()
        {
            return Expr.ToString();
        }
    }
	public class CaseLabels : AstElement
	{
		public ConstExpr ConstExpr1;
		public ConstExpr ConstExpr2;
		public override void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			var str = ConstExpr2 != null ? $"..{ConstExpr2}" : "";
			return $"{ConstExpr1}{str}";
		}
	}
	public class StatementSeq : AstElement
	{
		public AstList Statements = new AstList();
        public override void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return String.Join(";" + Environment.NewLine, Statements);
		}
	}
	public class Set : AstElement {
		public AstList Elements = new AstList();
		public override void Accept(IAstVisitor v) => v.Visit(this);
		override public string ToString() => $"{{ {String.Join(", ", Elements)} }}";
	}
	public class Element : AstElement {
		public Expr Expr1;
		public Expr Expr2;
		public override void Accept(IAstVisitor v) => v.Visit(this);
        public override string ToString()
        {
            return Expr2 == null? Expr1.ToString(): $"{Expr1}..{Expr2}";
        }
    }
	
	public class AddOp : AstElement
	{
		public enum AddOps
		{
			Add, Sub, Or
		}

		public AddOps Op;
		public override void Accept(IAstVisitor v) => v.Visit(this);

        public override string ToString()
        {
            return Op.ToString();
        }

    }

	public class MulOp : AstElement
	{
		public enum MulOps
		{
			Mul, Division, DIV, MOD, AND
		}

		public MulOps Op;
		public override void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return Op.ToString();
		}
	}
	
	public class Relation : AstElement
	{
		public enum Relations
		{
			Eq, Neq, Lss, Leq, Gtr, Geq, In, Is
		}
		public Relations Op;
		public override void Accept(IAstVisitor v) => v.Visit(this);

        public override string ToString()
        {
            return Op.ToString();
        }
    }

	public class SimpleElementExpr : AstElement {
		public AddOp AddOp;
		public Term Term;
        public override void Accept(IAstVisitor v)
        {
			v.Visit(this);
        }
        public override string ToString()
        {
            return $"{AddOp} {Term} ";
        }
    }

	public class SimpleExpr : AstElement
	{
		public Common.SymTable.TypeDesc TypeDescr
		{
			get
			{
				var type = Term.TypeDescr;

				foreach (var item in SimpleExprElements.Cast<SimpleElementExpr>())
				{
					type = TypeResolver.ResolveAddOp(item.AddOp.Op, type.form, item.Term.TypeDescr.form);
				}

				return type;
			}
		}
		public Term Term;
		public AstList SimpleExprElements = new AstList();
		public override void Accept(IAstVisitor v) => v.Visit(this);

        public override string ToString()
        {
			var sb = new StringBuilder();
			
			
			sb.Append(Term.ToString());
			if (SimpleExprElements.Count() > 0)
			{
				sb.Append(" ");
				sb.Append(SimpleExprElements);
			}
			return sb.ToString();
        }
    }

	public class TermElementExpr : AstElement
	{
		public MulOp MulOp;
		public IFactor Factor;
		public override void Accept(IAstVisitor v)
		{
			v.Visit(this);
		}

        public override string ToString()
        {
            return $"{MulOp} {Factor}";
        }
    }
	public class Term : AstElement
	{
		public Common.SymTable.TypeDesc TypeDescr
		{
			get
			{
				var type = Factor.TypeDescr;

				foreach (var item in TermElements.Cast<TermElementExpr>())
				{
					type = TypeResolver.ResolveMulOp(item.MulOp.Op, type.form, item.Factor.TypeDescr.form);
				}

				return type;
			}
		}

		public IFactor Factor;
		public AstList TermElements = new AstList();
		public override void Accept(IAstVisitor v) => v.Visit(this);

		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.Append(Factor.ToString());
			if (TermElements.Count() > 0)
			{
				sb.Append(" ");
				sb.Append(TermElements);
			}
			return sb.ToString();
		}
	}

	public class Expr : AstElement
	{
		public Expr(Scope scope)
		{
            this.scope = scope;
        }

		public Common.SymTable.TypeDesc TypeDescr => SimpleExpr2 != null ? TypeResolver.ResolveRel(Relation.Op, SimpleExpr.TypeDescr.form, SimpleExpr2.TypeDescr.form) : SimpleExpr.TypeDescr;

		public SimpleExpr SimpleExpr;
		public Relation Relation;
		public SimpleExpr SimpleExpr2;
        public readonly Scope scope;

        public override void Accept(IAstVisitor v) => v.Visit(this);

		public override string ToString()
		{
			if (Relation!=null) return $"{SimpleExpr} {Relation} {SimpleExpr2}";
			return $"{SimpleExpr}";
		}
	}

	public class Case : AstElement
	{
		public AstList CaseLabels = new AstList();
		public StatementSeq StatementSeq;
		public override void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return $"{String.Join(",", CaseLabels)}:{StatementSeq}";
        }

    }
	public abstract class IStatement : AstElement
	{

		public class AssignmentStatement : IStatement
		{
			public Designator Designator;
			public Expr Expr;
			public override void Accept(IAstVisitor v) => v.Visit(this);
            public override string ToString()
            {
                return $"{Designator} := {Expr}";
            }
        }

		public class ProcCallStatement : IStatement
		{
			public Designator Designator;
			public ExprList ExprList;
			public override void Accept(IAstVisitor v) => v.Visit(this);
            public override string ToString()
            {
                return $"{Designator} ({ExprList})";
            }
        }
		public class IfStatement : IStatement
		{
			public class IfThen : AstElement
			{
				public Expr Cond;
				public StatementSeq ThenBody;

                public override void Accept(IAstVisitor v)
                {
					v.Visit(this);
                }
				public override string ToString()
				{
					return $"{Cond} THEN {ThenBody}";
				}
			}
			public IfThen If = new IfThen();
			public AstList ELSIFs = new AstList();
			public StatementSeq ElseBody;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				var elsifs = String.Join("", ELSIFs?.Select(x => $"ELSIF {x}"));
				return $"IF {If} {elsifs} " + (ElseBody!=null ? $"ELSE {ElseBody}":"") + " END";
			}
		}
		public class CaseStatement : IStatement
		{
			public Expr Expr;
			public AstList Cases = new AstList();
			public StatementSeq ElseBody;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"CASE {Expr} DO {String.Join(" | ", Cases)} ELSE {ElseBody} END";
			}
		}
		public class WhileStatement : IStatement
		{
			public Expr Expr;
			public StatementSeq StatementSeq;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"WHILE {Expr} DO {StatementSeq} END";
			}
		}
		public class RepeatStatement : IStatement
		{
			public StatementSeq StatementSeq;
			public Expr Expr;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"REPEAT {StatementSeq} UNTIL {Expr}";
			}
		}
		public class ForStatement : IStatement
		{
			public Ident Ident;
			public Expr Expr;
			public Expr ToExpr;
			public ConstExpr ByExpr;
			public StatementSeq StatementSeq;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"FOR {Ident}:={Expr} TO {ToExpr}[BY {ByExpr}] DO {StatementSeq} END";
			}
		}
		public class LoopStatement : IStatement
		{
			public StatementSeq StatementSeq;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"LOOP {StatementSeq} END";
			}
		}

		public class WithStatement : IStatement
		{
			public Qualident Qualident1;
			public Qualident Qualident2;
			public StatementSeq StatementSeq;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"WITH {Qualident1} : {Qualident2} DO {StatementSeq} END";
			}
		}
		
		public class StatBlockStatement : IStatement
		{
			public StatBlock StatBlock;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{StatBlock}";
			}
		}
		public class AwaitStatement : IStatement
		{
			public Expr Expr;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"AWAIT ({Expr})";
			}
		}
		public class ExitStatement : IStatement
		{
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"EXIT";
			}
		}
		
		public class IgnoreStatement : IStatement
		{
			public Expr Expr;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"IGNORE {Expr}";
			}
		}
		public class ReturnStatement : IStatement
		{
			public Expr Expr;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"RETURN {Expr}";
			}
		}
	}
	
	public class Number : AstElement
	{
		public TypeDesc TypeDescr => TypeResolver.ResolveNumber(Value);

		public string Value;
		public override void Accept(IAstVisitor v) => v.Visit(this);
        public override string ToString()
        {
            return Value;
        }
    }

	public abstract class IFactor : AstElement
	{
		public enum FactorPrefix
		{
			Add, Sub
		}
		public FactorPrefix? Prefix;
		public abstract Common.SymTable.TypeDesc TypeDescr { get; }
		public class DesignatorFactor : IFactor
		{
			public override TypeDesc TypeDescr => Value.TypeDescr;
			public Designator Value;
			public override void Accept(IAstVisitor v) => v.Visit(this);

			public override string ToString()
			{
				return $"{Prefix} {Value}";
			}
		}
		public class NumberFactor : IFactor
		{
			public override TypeDesc TypeDescr => Value.TypeDescr;
			public Number Value;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{Prefix} {Value}";
			}
		}
		public class CharacterFactor : IFactor
		{
			public override TypeDesc TypeDescr => TypeDesc.CHAR8;
			public String Value;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{Prefix} {Value}";
			}
		}
		public class StringFactor : IFactor
		{
			public override TypeDesc TypeDescr => TypeDesc.Array(TypeDesc.UINT8, Array.Empty<int>());//to do all are short then array of shortreal else real

			public String Value;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{Prefix} {Value}";
			}
		}
		public class NilFactor : IFactor
		{
			public override TypeDesc TypeDescr => TypeDesc.None;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"NIL";
			}
		}
		public class SetFactor : IFactor
		{
			public override TypeDesc TypeDescr => TypeDesc.Predefined("SET", null);
			public Set Value;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{Prefix} {Value}";
			}
		}
		public class ExprFactor : IFactor
		{
			public override TypeDesc TypeDescr => Value.TypeDescr;
			public Expr Value;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{Prefix} {Value}";
			}
		}
		
		public class SizeOfFactor : IFactor
		{
			public override TypeDesc TypeDescr => Value.TypeDescr;
			public IFactor Value;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"SIZE OF {Value}";
			}
		}
		public class NegFactor : IFactor
		{
			public override TypeDesc TypeDescr => Value.TypeDescr;
			public IFactor Value;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"~{Value}";
			}
		}
	}

	public class Designator : AstElement
	{
		public Designator(Common.SymTable.Scope scope)
		{
			this.scope = scope;
		}
		public abstract class IDesignatorSpec : AstElement
		{
			public abstract TypeDesc Specify(TypeDesc parent);
			public class RecordDesignatorSpec : IDesignatorSpec {
				public Ident Value;
				public override void Accept(IAstVisitor v) => v.Visit(this);
				public override string ToString()
				{
					return $".{Value}";
				}
				public override TypeDesc Specify(TypeDesc parent)
				{
					return parent.scope?.Find(Value.Name)?.type??TypeDesc.None;
				}
			}
			public class PointerDesignatorSpec : IDesignatorSpec
			{
				public override void Accept(IAstVisitor v) => v.Visit(this);

				public override TypeDesc Specify(TypeDesc parent)
				{
					return parent.elemType;
				}

				public override string ToString()
				{
					return $"^";
				}
			}
			public class ArrayDesignatorSpec : IDesignatorSpec {
				public ExprList Value;
				public override void Accept(IAstVisitor v) => v.Visit(this);
				public override string ToString()
				{
					return $"[{Value}]";
				}
				public override TypeDesc Specify(TypeDesc parent)
				{
					return parent.elemType;
				}
			}
			public class CastDesignatorSpec : IDesignatorSpec {
				public CastDesignatorSpec(Common.SymTable.Scope scope)
				{
					this.scope = scope;
				}
				private Scope scope;
				public Qualident Value;
				public override void Accept(IAstVisitor v) => v.Visit(this);
				public override string ToString()
				{
					return $"({Value})";
				}
				public override TypeDesc Specify(TypeDesc parent)
				{
					return scope.Find(this.Value.ToString())?.type;
				}
			}
			public class ProcCallDesignatorSpec : IDesignatorSpec {
				public ExprList Value;
				public override void Accept(IAstVisitor v) => v.Visit(this);
				public override string ToString()
				{
					return $"({Value})";
				}
				public override TypeDesc Specify(TypeDesc parent)
				{
					return parent.elemType;
				}
			}
		}
		private Scope scope;

		public TypeDesc TypeDescr
		{
			get
			{
				TypeDesc t = null;
				if (this.Qualident.Ident1.Name == "Math")
				{
					t = TypeDesc.Function(TypeDesc.FLOAT64, null, scope);
				}
				else {
					t = TypeResolver.Resolve(this.Qualident.TypeDescr);
				}
				if (t == null || t.form == TypeForm.NONE) return t;

				foreach (var spec in Specs.Cast<IDesignatorSpec>())
				{
					t = spec.Specify(t);
					if (t == null) return TypeDesc.None;
				}
				return t;
			}
		}

		public Qualident Qualident;
		public bool EndOfLine;
		public AstList Specs = new AstList();
		public override void Accept(IAstVisitor v) => v.Visit(this);

        public override string ToString()
        {
            return $"{Qualident}{Specs}" + (EndOfLine ? "$" : "");
        }
    }

}