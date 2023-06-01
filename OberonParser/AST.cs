using System.Collections;
using System.Text;

namespace AOParser.Ast
{

	public interface IAstElement
	{
		void Accept(IAstVisitor v);
	}

	public class AstList : IEnumerable<IAstElement>
	{
		public List<IAstElement> Value { get; set; } = new List<IAstElement>();

        public void Add(IAstElement obj)
        {
			if (obj != null)
			{
				Value.Add(obj);
			}
        }
		public List<T> Cast<T>()
			where T : IAstElement
		{
			return Value.Cast<T>().ToList();
		}

        public IEnumerator<IAstElement> GetEnumerator()
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

	public class Ident : IAstElement
	{
		public string Name { get; set; }

        public void Accept(IAstVisitor v) => v.Visit(this);
        public override string ToString()
        {
            return Name;
        }
    }

	public class Qualident : IAstElement
	{
		public Ident Ident1;
		public Ident Ident2;
        public void Accept(IAstVisitor v) => v.Visit(this);
        public override string ToString()
        {
            return Ident2 != null ? $"{Ident1}.{Ident2}" : Ident1.ToString();
        }
    }
	public class Guard : IAstElement
	{
		public Qualident VarQualident;
		public Qualident TypeQualident;
		
		public void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return $"{VarQualident}:{TypeQualident}";
		}
	}
	public class DefinitionProc : IAstElement {
		public Ident Ident;
		public FormalPars FormalPars;

        public void Accept(IAstVisitor v) => v.Visit(this);
        public override string ToString()
        {
            return $"PROCEDURE {Ident} {FormalPars}";
        }
    }
	

	public class FieldDecl : IAstElement
	{
		public IdentList IdentList;
		public IType Type_;
		public void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return $"{IdentList} : {Type_}";
		}
	}
	public class ProcHead : IAstElement
	{
		public enum Tags {
			Export, Initializer
		}
		public SysFlag SysFlag;
		public IdentDef IdentDef;
		public FormalPars FormalPars;
		public Tags? Tag;
		public void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return $"{SysFlag} {Tag} {IdentDef} {FormalPars}";
		}
	}

	public class SysFlag : IAstElement
	{
		public Ident Ident;
		public void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return $"[{Ident}]";
		}
	}
	public class StatBlock : IAstElement
	{
		public AstList IdentList = new AstList();
		public StatementSeq StatementSeq;

		public void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return $"BEGIN {IdentList} {StatementSeq} END";
		}
	}
	public class Body : IAstElement
	{
		public StatBlock StatBlock;
		public void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			
			return StatBlock == null ? "END" : StatBlock.ToString();
		}
	}

	public class Definition : IAstElement
	{
		public Ident Ident;
		public Qualident Qualident;
		public AstList Procs = new AstList();
		public void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return $"DEFINITION {Ident} [REFINES{Qualident}] {Procs}";
		}
	}
	public class Module : IAstElement
	{
		public Ident Ident;
		public AstList ImportList;
		public Definition Definition;
		public DeclSeq DeclSeq = new DeclSeq();
		public Body Body;
		public void Accept(IAstVisitor v) => v.Visit(this);

		public override string ToString()
		{
			return $"MODULE {Ident}";
		}
	}

	public class Import : IAstElement
	{
		public Ident Name;
		public Ident OriginalName;
		public void Accept(IAstVisitor v) => v.Visit(this);

		public override string ToString()
		{
			return $"{Name} : {OriginalName}";
		}
	}

	public class IdentDef : IAstElement
	{
		public enum IdentExport
		{
			Private,
			ExportReadonly,
			Export
		}
		public Ident Ident;
		public IdentExport Export;
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public interface IConstTypeVarListDecl : IAstElement {
		public class ConstDeclList : AstList, IAstElement, IConstTypeVarListDecl
		{
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class TypeDeclList : AstList, IAstElement, IConstTypeVarListDecl
		{
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class VarDeclList : AstList, IAstElement, IConstTypeVarListDecl
		{
			public void Accept(IAstVisitor v) => v.Visit(this);
		}

	}
	public interface IProcForwardDecl : IAstElement { 
	
	}

	public class DeclSeq : IAstElement
	{
		public AstList ConstTypeVarDecls = new ();
		public AstList ProcForwardDecls = new ();
		public AstList ProcDecl = new();
		public void Accept(IAstVisitor v) => v.Visit(this);
	}


	public class ConstDecl : IAstElement
	{
		public IdentDef IdentDef;
		public ConstExpr ConstExpr;
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class TypeDecl : IAstElement
	{
		public IdentDef IdentDef;
		public IType Type_;
        public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class VarDecl : IAstElement
	{
		public IdentList IdentList;
		public IType Type_;
        public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class ProcDecl : IAstElement, IProcForwardDecl
	{
		public ProcHead ProcHead;
		public DeclSeq DeclSeq;
		public Body Body;
		public void Accept(IAstVisitor v) => v.Visit(this);
	}


	public class MethAttributes : IAstElement
	{
		public enum MethodAttr
		{
			ABSTRACT, EMPTY, EXTENSIBLE
		}
		public bool IsNew;
		public MethodAttr? Attr;
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class ForwardDecl : IAstElement, IProcForwardDecl
	{
		public Receiver Receiver;
		public IdentDef IdentDef;
		public FormalPars FormalPars;
		public MethAttributes MethAttributes;
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class FormalPars : IAstElement
	{
		public AstList FPSections = new AstList();
		public Qualident Qualident;
        public void Accept(IAstVisitor v) => v.Visit(this);
	}

	

	public class FPSection : IAstElement
	{
		public enum Prefix
		{
			VAR, IN, OUT
		}
		public Prefix? FpSectionPrefix;
		public AstList Idents = new AstList();
		public IType Type_;
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	
	public class Receiver : IAstElement
	{
		public enum Prefix
		{
			VAR, IN
		}
		public Prefix? ReceiverPrefix;
		public Ident SelfIdent;
		public Ident TypeIdent;
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
    public interface IType : IAstElement
    {
		public class SynonimType : IType
		{
			public Qualident Qualident;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class ArrayType : IType
		{
			public SysFlag SysFlag;
			public AstList ConstExprs = new AstList();
			public IType Type_;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}

		public class RecordType : IType
		{
			public SysFlag SysFlag;
			public Qualident Qualident;
			public FieldList FieldList;
			public void Accept(IAstVisitor v) => v.Visit(this);

		}

		public class ObjectType : IType
		{
			public SysFlag SysFlag;
			public Qualident Qualident;
			public Qualident ImplementsQualident;
			public DeclSeq DeclSeq;
			public Body Body;
			public void Accept(IAstVisitor v) => v.Visit(this);

		}
		public class PointerType : IType
		{
			public SysFlag SysFlag;
			public IType Type_;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class ProcedureType : IType
		{
			public SysFlag SysFlag;
			public FormalPars FormalPars;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
	}
    

	public class ExprList : IAstElement {
		public AstList Exprs = new AstList();
		public void Accept(IAstVisitor v) => v.Visit(this);

		public override string ToString()
		{
			return String.Join(',', Exprs);
		}
	}
	public class IdentList : IAstElement
	{
		public AstList IdentDefs = new AstList();
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class FieldList : IAstElement
	{
		public AstList FieldDecl = new AstList();
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	
	
	public class ConstExpr : IAstElement {
		public Expr Expr;
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class CaseLabels : IAstElement
	{
		public ConstExpr ConstExpr1;
		public ConstExpr ConstExpr2;
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class StatementSeq : IAstElement
	{
		public AstList Statements = new AstList();
        public void Accept(IAstVisitor v) => v.Visit(this);
    }
	public class Set : IAstElement {
		public AstList Elements = new AstList();
		public void Accept(IAstVisitor v) => v.Visit(this);
		override public string ToString() => $"{{ {String.Join(", ", Elements)} }}";
	}
	public class Element : IAstElement {
		public Expr Expr1;
		public Expr Expr2;
		public void Accept(IAstVisitor v) => v.Visit(this);
        public override string ToString()
        {
            return Expr2 == null? Expr1.ToString(): $"{Expr1}..{Expr2}";
        }
    }
	
	public class AddOp : IAstElement
	{
		public enum AddOps
		{
			Add, Sub, Or
		}

		public AddOps Op;
		public void Accept(IAstVisitor v) => v.Visit(this);

        public override string ToString()
        {
            return Op.ToString();
        }

    }

	public class MulOp : IAstElement
	{
		public enum MulOps
		{
			Mul, Division, DIV, MOD, AND
		}

		public MulOps Op;
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	
	public class Relation : IAstElement
	{
		public enum Relations
		{
			Eq, Neq, Lss, Leq, Gtr, Geq, In, Is
		}
		public Relations Op;
		public void Accept(IAstVisitor v) => v.Visit(this);

        public override string ToString()
        {
            return Op.ToString();
        }
    }

	public class SimpleElementExpr : IAstElement {
		public AddOp AddOp;
		public Term Term;
        public void Accept(IAstVisitor v)
        {
			v.Visit(this);
        }
        public override string ToString()
        {
            return $"{AddOp} {Term} ";
        }
    }

	public class SimpleExpr : IAstElement
	{
		public enum SimpleExprPrefix
		{
			Add, Sub
		}
		public SimpleExprPrefix? Prefix;
		public Term Term;
		public AstList SimpleExprElements = new AstList();
		public void Accept(IAstVisitor v) => v.Visit(this);

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

	public class TermElementExpr : IAstElement
	{
		public MulOp MulOp;
		public IFactor Factor;
		public void Accept(IAstVisitor v)
		{
			v.Visit(this);
		}
	}
	public class Term : IAstElement
	{
		public IFactor Factor;
		public AstList TermElements = new AstList();
		public void Accept(IAstVisitor v) => v.Visit(this);

		public override string ToString()
		{
			return !TermElements.Any() ? Factor.ToString() : $"{Factor} {TermElements}";
		}
	}

	public class Expr : IAstElement
	{
		public SimpleExpr SimpleExpr;
		public Relation Relation;
		public SimpleExpr SimpleExpr2;
		public void Accept(IAstVisitor v) => v.Visit(this);

		public override string ToString()
		{
			if (Relation!=null) return $"{SimpleExpr} {Relation} {SimpleExpr2}";
			return $"{SimpleExpr}";
		}
	}

	public class Case : IAstElement
	{
		public AstList CaseLabels = new AstList();
		public StatementSeq StatementSeq;
		public void Accept(IAstVisitor v) => v.Visit(this);

	}
	public interface IStatement : IAstElement
	{

		public class AssignmentStatement : IStatement
		{
			public Designator Designator;
			public Expr Expr;
			public void Accept(IAstVisitor v) => v.Visit(this);
            public override string ToString()
            {
                return $"{Designator} := {Expr}";
            }
        }

		public class ProcCallStatement : IStatement
		{
			public Designator Designator;
			public ExprList ExprList;
			public void Accept(IAstVisitor v) => v.Visit(this);
            public override string ToString()
            {
                return $"{Designator}";
            }
        }
		public class IfStatement : IStatement
		{
			public class IfThen : IAstElement
			{
				public Expr Cond;
				public StatementSeq ThenBody;

                public void Accept(IAstVisitor v)
                {
					v.Visit(this);
                }
            }
			public IfThen If = new IfThen();
			public AstList ELSIFs = new AstList();
			public StatementSeq ElseBody;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class CaseStatement : IStatement
		{
			public Expr Expr;
			public AstList Cases = new AstList();
			public StatementSeq ElseBody;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class WhileStatement : IStatement
		{
			public Expr Expr;
			public StatementSeq StatementSeq;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class RepeatStatement : IStatement
		{
			public StatementSeq StatementSeq;
			public Expr Expr;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class ForStatement : IStatement
		{
			public Ident Ident;
			public Expr Expr;
			public Expr ToExpr;
			public ConstExpr ByExpr;
			public StatementSeq StatementSeq;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class LoopStatement : IStatement
		{
			public StatementSeq StatementSeq;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}

		public class WithAlternativeStatement : IStatement
		{
			public Guard Guard;
			public StatementSeq StatementSeq;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class WithStatement : IStatement
		{
			public AstList Alternatives = new AstList();
			public StatementSeq ElseStatementSeq;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class ExitStatement : IStatement
		{
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class ReturnStatement : IStatement
		{
			public Expr Expr;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
	}
	
	public class Number : IAstElement
	{
		public string Value;
		public void Accept(IAstVisitor v) => v.Visit(this);
        public override string ToString()
        {
            return Value;
        }
    }

	public interface IFactor : IAstElement
	{
		public class DesignatorFactor : IFactor
		{
			public Designator Value;
			public void Accept(IAstVisitor v) => v.Visit(this);

			public override string ToString()
			{
				return $"{Value}";
			}
		}
		public class NumberFactor : IFactor
		{
			public Number Value;
			public void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{Value}";
			}
		}
		public class CharacterFactor : IFactor
		{
			public String Value;
			public void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{Value}";
			}
		}
		public class StringFactor : IFactor
		{
			public String Value;
			public void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{Value}";
			}
		}
		public class NilFactor : IFactor
		{
			public void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"NIL";
			}
		}
		public class SetFactor : IFactor
		{
			public Set Value;
			public void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{Value}";
			}
		}
		public class ExprFactor : IFactor
		{
			public Expr Value;
			public void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{Value}";
			}
		}
		public class NegFactor : IFactor
		{
			public IFactor Value;
			public void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"~{Value}";
			}
		}
	}

	public class Designator : IAstElement
	{
		public interface IDesignatorSpec : IAstElement
		{
            public class RecordDesignatorSpec : IDesignatorSpec {
				public Ident Value;
				public void Accept(IAstVisitor v) => v.Visit(this);
				public override string ToString()
				{
					return $".{Value}";
				}
			}
			public class ArrayDesignatorSpec : IDesignatorSpec {
				public ExprList Value;
				public void Accept(IAstVisitor v) => v.Visit(this);
				public override string ToString()
				{
					return $"{Value}";
				}
			}
			public class PointerDesignatorSpec : IDesignatorSpec {
				public void Accept(IAstVisitor v) => v.Visit(this);
				public override string ToString()
				{
					return $"^";
				}
			}
			public class CastDesignatorSpec : IDesignatorSpec {
				public Qualident Value;
				public void Accept(IAstVisitor v) => v.Visit(this);
				public override string ToString()
				{
					return $"({Value})";
				}
			}
			public class ProcCallDesignatorSpec : IDesignatorSpec {
				public ExprList Value;
				public void Accept(IAstVisitor v) => v.Visit(this);
				public override string ToString()
				{
					return $"({Value})";
				}
			}
		}

		public Qualident Qualident;
		public bool EndOfLine;
		public AstList Specs = new AstList();
		public void Accept(IAstVisitor v) => v.Visit(this);

        public override string ToString()
        {
            return $"{Qualident}{Specs}" + (EndOfLine ? "$" : "");
        }
    }

}