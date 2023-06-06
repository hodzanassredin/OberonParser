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
				sb.AppendLine(item.ToString());
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
        public Qualident(SymTab tab)
        {
			this.tab = tab;

		}
		public Ident Ident1;
		public Ident Ident2;
        public override void Accept(IAstVisitor v) => v.Visit(this);
        public override string ToString()
        {
            return Ident2 != null ? $"{Ident1}.{Ident2}" : Ident1.ToString();
        }
		private readonly SymTab tab;

		public TypeDesc FindType()
		{
			return TypeDesc.Predefined($"{Ident1.Name}.{Ident2.Name}", tab.curScope);
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
		public enum Tags {
			Export, Initializer
		}
		public SysFlag SysFlag;
		public IdentDef IdentDef;
		public FormalPars FormalPars;
		public Tags? Tag;
		public override void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return $"{SysFlag} {Tag} {IdentDef} {FormalPars}";
		}
	}

	public class SysFlag : AstElement
	{
		public Ident Ident;
		public override void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return $"[{Ident}]";
		}
	}
	public class StatBlock : AstElement
	{
		public AstList IdentLists = new AstList();
		public StatementSeq StatementSeq;

		public override void Accept(IAstVisitor v) => v.Visit(this);
		public override string ToString()
		{
			return $"BEGIN {IdentLists} {StatementSeq} END";
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
	}

	public class FPSection : AstElement
	{
		public enum Prefix
		{
			VAR
		}
		public Prefix? FpSectionPrefix;
		public AstList Idents = new AstList();
		public IType Type_;
		public override void Accept(IAstVisitor v) => v.Visit(this);
        public override string ToString()
        {
            return $"{FpSectionPrefix} {String.Join(",",Idents)}:{Type_}";
        }
    }
	
    public abstract class IType : AstElement
    {
		public class SynonimType : IType
		{
			public Qualident? Qualident;
			public override void Accept(IAstVisitor v) => v.Visit(this);
            public override string ToString()
            {
                return Qualident?.ToString()??"";
            }
        }
		public class ArrayType : IType
		{
			public SysFlag SysFlag;
			public AstList ConstExprs = new AstList();
			public IType Type_;
			public override void Accept(IAstVisitor v) => v.Visit(this);
            public override string ToString()
            {
                return $"ARRAY {SysFlag} {String.Join(",", ConstExprs)} OF {Type_}";
            }
        }

		public class RecordType : IType
		{
			public SysFlag SysFlag;
			public Qualident Qualident;
			public FieldList FieldList;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"RECORD {SysFlag}({Qualident}) {FieldList} END";
			}
		}

		public class ObjectType : IType
		{
			public SysFlag SysFlag;
			public Qualident Qualident;
			public Qualident ImplementsQualident;
			public DeclSeq DeclSeq;
			public Body Body;

			public Ident Ident;
			public override void Accept(IAstVisitor v) => v.Visit(this);

            public override string ToString()
            {
				var impl = ImplementsQualident != null? $"IMPLEMENTS {ImplementsQualident}" : "";

				return $"OBJECT {SysFlag}({Qualident}) {impl} {DeclSeq} {Body} {Ident}";
            }

        }
		public class PointerType : IType
		{
			public SysFlag SysFlag;
			public IType Type_;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"POINTER {SysFlag} TO {Type_}";
			}
		}
		public class ProcedureType : IType
		{
			public SysFlag SysFlag;
			public FormalPars FormalPars;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"PROCEDURE {SysFlag} {FormalPars}";
			}
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
		public MulOp MulOp;
		public Term Term;
        public override void Accept(IAstVisitor v)
        {
			v.Visit(this);
        }
        public override string ToString()
        {
            return $"{MulOp} {Term} ";
        }
    }

	public class SimpleExpr : AstElement
	{
		
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
		public AddOp AddOp;
		public IFactor Factor;
		public override void Accept(IAstVisitor v)
		{
			v.Visit(this);
		}

        public override string ToString()
        {
            return $"{AddOp} {Factor}";
        }
    }
	public class Term : AstElement
	{
		public enum TermExprPrefix
		{
			Add, Sub
		}
		public TermExprPrefix? Prefix;
		public IFactor Factor;
		public AstList TermElements = new AstList();
		public override void Accept(IAstVisitor v) => v.Visit(this);

		public override string ToString()
		{
			var sb = new StringBuilder();
			if (Prefix.HasValue)
			{
				sb.Append(Prefix.Value.ToString());
				sb.Append(" ");
			}
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
		public string Value;
		public override void Accept(IAstVisitor v) => v.Visit(this);
        public override string ToString()
        {
            return Value;
        }
    }

	public abstract class IFactor : AstElement
	{
		public class DesignatorFactor : IFactor
		{
			public Designator Value;
			public override void Accept(IAstVisitor v) => v.Visit(this);

			public override string ToString()
			{
				return $"{Value}";
			}
		}
		public class NumberFactor : IFactor
		{
			public Number Value;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{Value}";
			}
		}
		public class CharacterFactor : IFactor
		{
			public String Value;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{Value}";
			}
		}
		public class StringFactor : IFactor
		{
			public String Value;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{Value}";
			}
		}
		public class NilFactor : IFactor
		{
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"NIL";
			}
		}
		public class SetFactor : IFactor
		{
			public Set Value;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{Value}";
			}
		}
		public class ExprFactor : IFactor
		{
			public Expr Value;
			public override void Accept(IAstVisitor v) => v.Visit(this);
			public override string ToString()
			{
				return $"{Value}";
			}
		}
		public class NegFactor : IFactor
		{
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
		public abstract class IDesignatorSpec : AstElement
		{
            public class RecordDesignatorSpec : IDesignatorSpec {
				public Ident Value;
				public override void Accept(IAstVisitor v) => v.Visit(this);
				public override string ToString()
				{
					return $".{Value}";
				}
			}
			public class ArrayDesignatorSpec : IDesignatorSpec {
				public ExprList Value;
				public override void Accept(IAstVisitor v) => v.Visit(this);
				public override string ToString()
				{
					return $"[{Value}]";
				}
			}
			public class CastDesignatorSpec : IDesignatorSpec {
				public Qualident Value;
				public override void Accept(IAstVisitor v) => v.Visit(this);
				public override string ToString()
				{
					return $"({Value})";
				}
			}
			public class ProcCallDesignatorSpec : IDesignatorSpec {
				public ExprList Value;
				public override void Accept(IAstVisitor v) => v.Visit(this);
				public override string ToString()
				{
					return $"({Value})";
				}
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