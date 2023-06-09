using Common.Cp;
using Common.SymTable;
using System.Collections;
using System.Text;

namespace CPParser.Ast
{

	public abstract class AstElement
	{
		public AstList CommentsBefore = new AstList();
		public void AcceptWithComments(IAstVisitor v) {
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
		public List<AstElement> Value { get; set; }

        public AstList()
        {
			Value = new List<AstElement>();
		}
		public AstList(List<AstElement> vals)
		{
			Value = vals;
		}
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
				sb.AppendLine(item.ToString());
			}	
            return sb.ToString();
        }
    }
	public class Comment : AstElement
	{
		public string Content { get; set; }

		public override void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return $"(*{Content}*)";
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
        public Qualident(Common.SymTable.SymTab tab)
        {
            this.tab = tab;
        }
		//module ident
		public Ident Ident1;
		public Ident Ident2;
        private readonly SymTab tab;

		public TypeDesc FindType()
		{
			return TypeDesc.Predefined(ToString(), tab.curScope);
		}
		public override void Accept(IAstVisitor v) => v.Visit(this);
        public override string ToString()
        {
            return Ident2 != null ? $"{Ident1}.{Ident2}" : Ident1.ToString();
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

	public class Module : AstElement
	{

		public Ident Ident;
		public AstList ImportList;
		public DeclSeq DeclSeq = new DeclSeq();
		public StatementSeq Begin;
		public StatementSeq Close;
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

		public override string ToString()
		{
			return $"{Name} : {OriginalName}";
		}

		public Obj GetObj()
		{
			return new Obj(ObjCLass.MODULE, Name.Name, TypeDesc.None, null);
		}
	}

	public class IdentDef : AstElement
	{
		public enum IdentExport
		{
			Private,
			ExportReadonly,
			Export
		}
		public Ident Ident;
		public IdentExport Export;
		public override void Accept(IAstVisitor v) => v.Visit(this);
	}
	public abstract class IConstTypeVarListDecl : AstElement {
		public class ConstDeclList : IConstTypeVarListDecl
		{
			public AstList Values = new();
			public override void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class TypeDeclList : IConstTypeVarListDecl
		{
			public AstList Values = new();
			public override void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class VarDeclList : IConstTypeVarListDecl
		{
			public AstList Values = new();
			public override void Accept(IAstVisitor v) => v.Visit(this);
		}

	}
	public abstract class IProcForwardDecl : AstElement { 
	
	}

	public class DeclSeq : AstElement
	{
		public AstList ConstTypeVarDecls = new ();
		public AstList ProcForwardDecls = new ();
		public override void Accept(IAstVisitor v) => v.Visit(this);
	}


	public class ConstDecl : AstElement
	{
		public IdentDef IdentDef;
		public ConstExpr ConstExpr;
		public override void Accept(IAstVisitor v) => v.Visit(this);

		public Obj GetObj() {
			return new Obj(ObjCLass.CONST, IdentDef.Ident.Name, ConstExpr.TypeDescr, ConstExpr.ToString());
		}
	}
	public class TypeDecl : AstElement
	{
		public IdentDef IdentDef;
		public IType Type_;
        public override void Accept(IAstVisitor v) => v.Visit(this);
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
		public IEnumerable<Obj> GetObjects()
		{
			return IdentList.GetNames().Select(name=> new Obj(ObjCLass.VAR, name, Type_.TypeDescr, null));
		}
	}
	public class ProcDecl : IProcForwardDecl
	{
		public Receiver? Receiver;
		public IdentDef IdentDef;
		public FormalPars? FormalPars;
		public MethAttributes MethAttributes;
		public DeclSeq DeclSeq;
        public StatementSeq StatementSeq;
		public override void Accept(IAstVisitor v) => v.Visit(this);
		public Obj GetObj(Scope scope)
		{
			return new Obj(ObjCLass.FUNC, (Receiver != null ? $"({Receiver}) " : "") + IdentDef.Ident.Name, FormalPars?.TypeDescr(scope) ?? TypeDesc.Function(TypeDesc.None, scope), "") { 
				scope = scope,
			};
		}
	}


	public class MethAttributes : AstElement
	{
		public enum MethodAttr
		{
			ABSTRACT, EMPTY, EXTENSIBLE
		}
		public bool IsNew;
		public MethodAttr? Attr;
		public override void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class ForwardDecl : IProcForwardDecl
	{
		public Receiver Receiver;
		public IdentDef IdentDef;
		public FormalPars FormalPars;
		public MethAttributes MethAttributes;
		public override void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class FormalPars : AstElement
	{
		public AstList FPSections = new AstList();
		public IType Type_;
        public override void Accept(IAstVisitor v) => v.Visit(this);

		public TypeDesc TypeDescr(Scope scope) {
				return TypeDesc.Function(Type_?.TypeDescr?? TypeDesc.None, scope);
		} 
	}

	

	public class FPSection : AstElement
	{
		public enum Prefix
		{
			VAR, IN, OUT
		}
		public Prefix? FpSectionPrefix;
		public AstList Idents = new AstList();
		public IType Type_;
		public override void Accept(IAstVisitor v) => v.Visit(this);

		public IEnumerable<Obj> Objects() { 

            foreach (var id in Idents.Cast<Ident>())
            {
				yield return new Obj(ObjCLass.PARAM, id.Name, Type_.TypeDescr, "");
            }
		}
	}
	
	public class Receiver : AstElement
	{
        public Receiver(SymTab tab)
        {
            this.tab = tab;
        }
		public enum Prefix
		{
			VAR, IN
		}
		public Prefix? ReceiverPrefix;
		public Ident SelfIdent;
		public Ident TypeIdent;
        private readonly SymTab tab;

        public override void Accept(IAstVisitor v) => v.Visit(this);

		public TypeDesc GetType() {
			return tab.Find(TypeIdent.Name).type;
		}
		public Obj GetObj()
		{
			
			return new Obj(ObjCLass.RECIEVER, SelfIdent.Name, GetType(), null);
		}
        public override string ToString()
        {
            return $"{SelfIdent.Name} : {TypeIdent}";
        }
    }
    public abstract class IType : AstElement
    {
		public abstract Common.SymTable.TypeDesc TypeDescr { get; }
		public class SynonimType : IType
		{
			public Qualident Qualident;
			public override TypeDesc TypeDescr => Qualident.FindType();

            public override void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class ArrayType : IType
		{
			public AstList ConstExprs = new AstList();
			public IType Type_;

            public override TypeDesc TypeDescr
			{
				get
				{
					var sizes = ConstExprs.Cast<ConstExpr>().Select(x => Int32.Parse(x.ToString())).ToArray();
					return TypeDesc.Array(Type_.TypeDescr, sizes);
				}
			}

            public override void Accept(IAstVisitor v) => v.Visit(this);
		}

		public class RecordType : IType
		{
            public RecordType(Scope scope)
            {
                this.scope = scope;
            }
			public enum Meta
			{
				ABSTRACT, EXTENSIBLE, LIMITED
			}
			public Meta? RecordMeta;
			//base record
			public Qualident Qualident;
			public AstList FieldList = new AstList();
            private readonly Scope scope;

            public override TypeDesc TypeDescr { get {
					return TypeDesc.Struct(Qualident?.FindType(), scope);

				} }

            public override void Accept(IAstVisitor v) => v.Visit(this);

		}
		public class PointerType : IType
		{
			public IType Type_;

            public override TypeDesc TypeDescr => TypeDesc.Pointer(Type_.TypeDescr);

            public override void Accept(IAstVisitor v) => v.Visit(this);
		}

		public class ProcedureType : IType
		{
            public ProcedureType(Scope scope)
            {
                this.scope = scope;
            }
			public FormalPars FormalPars;
            private readonly Scope scope;

            public override TypeDesc TypeDescr => FormalPars.TypeDescr(scope);

            public override void Accept(IAstVisitor v) => v.Visit(this);
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
		public AstList IdentDefs = new AstList();
		public string[] GetNames() {
			return IdentDefs.Cast<IdentDef>().Select(x => x.Ident.Name).ToArray();
		}
		public override void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class FieldList : AstElement
	{
		public Obj[] Fields => IdentList.GetNames().Select(name=> new Obj(ObjCLass.VAR, name, Type_.TypeDescr, "")).ToArray();
		public IdentList IdentList;
		public IType Type_;
		public override void Accept(IAstVisitor v) => v.Visit(this);
	}
	
	
	public class ConstExpr : AstElement {
		public Common.SymTable.TypeDesc TypeDescr => Expr.TypeDescr;
		public Expr Expr;
		public override void Accept(IAstVisitor v) => v.Visit(this);
        public override string ToString()
        {
            return Expr?.ToString();
        }
    }
	public class CaseLabels : AstElement
	{
		public ConstExpr ConstExpr1;
		public ConstExpr ConstExpr2;
		public override void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class StatementSeq : AstElement
	{
		public AstList Statements = new AstList();
        public override void Accept(IAstVisitor v) => v.Visit(this);
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
			get {
				var type = Term.TypeDescr;

                foreach (var item in SimpleExprElements.Cast<SimpleElementExpr>())
                {
					type = TypeResolver.ResolveAddOp(item.AddOp.Op, type.form, item.Term.TypeDescr.form);
                }

				return type;
			}
		}
		public enum SimpleExprPrefix
		{
			Add, Sub
		}
		public SimpleExprPrefix? Prefix;
		public Term Term;
		public AstList SimpleExprElements = new AstList();
		public override void Accept(IAstVisitor v) => v.Visit(this);

        public override string ToString()
        {
			var sb = new StringBuilder();
			if (Prefix.HasValue)
			{
				sb.Append(Prefix.Value.ToString());
				sb.Append(" ");
			}
			
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
			return !TermElements.Any() ? Factor.ToString() : $"{Factor} {TermElements}";
		}
	}

	public class Expr : AstElement
	{
		public Common.SymTable.TypeDesc TypeDescr => SimpleExpr2 != null ? TypeResolver.ResolveRel(Relation.Op, SimpleExpr.TypeDescr.form,SimpleExpr2.TypeDescr.form) : SimpleExpr.TypeDescr;

		public SimpleExpr SimpleExpr;
		public Relation Relation;
		public SimpleExpr SimpleExpr2;
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
                return $"{Designator}";
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
            }
			public IfThen If = new IfThen();
			public AstList ELSIFs = new AstList();
			public StatementSeq ElseBody;
			public override void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class CaseStatement : IStatement
		{
			public Expr Expr;
			public AstList Cases = new AstList();
			public StatementSeq ElseBody;
			public override void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class WhileStatement : IStatement
		{
			public Expr Expr;
			public StatementSeq StatementSeq;
			public override void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class RepeatStatement : IStatement
		{
			public StatementSeq StatementSeq;
			public Expr Expr;
			public override void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class ForStatement : IStatement
		{
			public Ident Ident;
			public Expr Expr;
			public Expr ToExpr;
			public ConstExpr ByExpr;
			public StatementSeq StatementSeq;
			public override void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class LoopStatement : IStatement
		{
			public StatementSeq StatementSeq;
			public override void Accept(IAstVisitor v) => v.Visit(this);
		}

		public class WithAlternativeStatement : IStatement
		{
			public Guard Guard;
			public StatementSeq StatementSeq;
			public override void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class WithStatement : IStatement
		{
			public AstList Alternatives = new AstList();
			public StatementSeq ElseStatementSeq;
			public override void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class ExitStatement : IStatement
		{
			public override void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class ReturnStatement : IStatement
		{
			public Expr Expr;
			public override void Accept(IAstVisitor v) => v.Visit(this);
		}
	}
	
	public class Number : AstElement
	{
		public string Value;
		public override void Accept(IAstVisitor v) => v.Visit(this);
        public override string ToString()
        {
            return Value;
        }
    }

	public abstract class IFactor : AstElement
	{
		public abstract Common.SymTable.TypeDesc TypeDescr { get; }
		public class DesignatorFactor : IFactor
		{
			public Designator Value;

            public override TypeDesc TypeDescr => Value.TypeDescr;

			public override void Accept(IAstVisitor v) => v.Visit(this);

			public override string ToString()
			{
				return $"{Value}";
			}
		}
		public class NumberFactor : IFactor
		{
			public Number Value;

            public override TypeDesc TypeDescr => TypeDesc.Predefined("NUMBER", null);//todo

            public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{Value}";
			}
		}
		public class CharacterFactor : IFactor
		{
			public String Value;

            public override TypeDesc TypeDescr => TypeDesc.Predefined("CHAR", null);

			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{Value}";
			}
		}
		public class StringFactor : IFactor
		{
			public String Value;

            public override TypeDesc TypeDescr => TypeDesc.Array(TypeDesc.Predefined("CHAR", null), Array.Empty<int>());

            public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{Value}";
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
			public Set Value;

            public override TypeDesc TypeDescr => TypeDesc.Predefined("SET", null);

			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{Value}";
			}
		}
		public class ExprFactor : IFactor
		{
			public Expr Value;

            public override TypeDesc TypeDescr => Value.TypeDescr;

            public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{Value}";
			}
		}
		public class NegFactor : IFactor
		{
			public IFactor Value;

            public override TypeDesc TypeDescr => Value.TypeDescr;

            public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"~{Value}";
			}
		}
	}

	public class Designator : AstElement
	{
        public Designator(Common.SymTable.SymTab tab)
        {
            this.tab = tab;
        }
		public abstract class IDesignatorSpec : AstElement
		{
			public abstract TypeDesc Specify(TypeDesc parent);

            public class RecordDesignatorSpec : IDesignatorSpec {
				public Ident Value;
				public override void Accept(IAstVisitor v) => v.Visit(this);

                public override TypeDesc Specify(TypeDesc parent)
                {
					return parent.scope.Find(Value.Name).type;
                }

                public override string ToString()
				{
					return $".{Value}";
				}
			}
			public class ArrayDesignatorSpec : IDesignatorSpec {
				public ExprList Value;
				public override void Accept(IAstVisitor v) => v.Visit(this);

                public override TypeDesc Specify(TypeDesc parent)
                {
					return parent.elemType;
				}

                public override string ToString()
				{
					return $"{Value}";
				}
			}
			public class PointerDesignatorSpec : IDesignatorSpec {
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
			public class CastDesignatorSpec : IDesignatorSpec {

                public CastDesignatorSpec(Common.SymTable.SymTab tab)
				{
					this.tab = tab;
				}
				public Qualident Value;
                private SymTab tab;

                public override void Accept(IAstVisitor v) => v.Visit(this);

                public override TypeDesc Specify(TypeDesc parent)
                {
					return tab.Find(this.Value.Ident2.Name)?.type;
				}

                public override string ToString()
				{
					return $"({Value})";
				}
			}
			public class ProcCallDesignatorSpec : IDesignatorSpec {
				public ExprList Value;
				public override void Accept(IAstVisitor v) => v.Visit(this);

                public override TypeDesc Specify(TypeDesc parent)
                {
					return parent.elemType;
				}

                public override string ToString()
				{
					return $"({Value})";
				}
			}
		}
		private readonly SymTab tab;
		public TypeDesc TypeDescr { get {
				var t = tab.Find(this.Qualident.ToString())?.type;
				if (t == null) return t;
				
                foreach (var spec in Specs.Cast<IDesignatorSpec>())
                {
					t = spec.Specify(t);
					if (t == null) return t;
				}
				return t;
			} }
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