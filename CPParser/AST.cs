using System.Collections;

namespace CPParser.Ast
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
			Value.Add(obj);
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
    }

	public class Ident : IAstElement
	{
		public string Name { get; set; }

        public void Accept(IAstVisitor v) => v.Visit(this);
    }

	public class Qualident : IAstElement
	{
		public AstList Idents { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class Guard : IAstElement
	{
		public Qualident VarQualident { get; set; }
		public Qualident TypeQualident { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}

	public class Module : IAstElement
	{
		public Ident Ident;
		public AstList ImportList;
		public DeclSeq DeclSeq = new DeclSeq();
		public StatementSeq Begin;
		public StatementSeq Close;
		public void Accept(IAstVisitor v) => v.Visit(this);
	}

	public class Import : IAstElement
	{
		public Ident Name;
		public Ident OriginalName;
		public void Accept(IAstVisitor v) => v.Visit(this);
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
		public Receiver Receiver;
		public IdentDef IdentDef;
		public FormalPars FormalPars;
		public MethAttributes MethAttributes;
		public DeclSeq DeclSeq;
        public StatementSeq StatementSeq;
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
		public AstList FPSections;
        public IType Type_ { get; set; }
        public void Accept(IAstVisitor v) => v.Visit(this);
	}

	

	public class FPSection : IAstElement
	{
		public enum Prefix
		{
			VAR, IN, OUT
		}
		public Prefix? FpSectionPrefix;
		public AstList Idents;
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
			public AstList ConstExprs;
			public IType Type_ { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}

		public class RecordType : IType
		{
			public enum Meta
			{
				ABSTRACT, EXTENSIBLE, LIMITED
			}
			public Meta RecordMeta;
			public Qualident Qualident;
			public AstList FieldList;
			public void Accept(IAstVisitor v) => v.Visit(this);

		}
		public class PointerType : IType
		{
			public IType Type_;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class ProcedureType : IType
		{
			public FormalPars FormalPars;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
	}
    

	public class ExprList : IAstElement {
		public AstList Exprs;
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class IdentList : IAstElement
	{
		public AstList IdentDefs = new AstList();
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class FieldList : IAstElement
	{
		public IdentList IdentList;
		public IType Type_;
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	
	
	public class ConstExpr : IAstElement {
		public Expr Expr;
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class CaseLabels : IAstElement
	{
		public AstList ConstExprs;
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class StatementSeq : IAstElement
	{
		public AstList Statements;
        public void Accept(IAstVisitor v) => v.Visit(this);
    }
	public class Set : IAstElement {
		public AstList Elements;
        public void Accept(IAstVisitor v) => v.Visit(this);		
	}
	public class Element : IAstElement {
		public AstList Exprs;
        public void Accept(IAstVisitor v) => v.Visit(this); 
	}
	
	public class AddOp : IAstElement
	{
		public enum AddOps
		{
			Add, Sub, Or
		}

		public AddOps Op;
		public void Accept(IAstVisitor v) => v.Visit(this);

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
	}
	
	public class SimpleExpr : IAstElement
	{
		public enum SimpleExprPrefix
		{
			Add, Sub
		}
		public SimpleExprPrefix? Prefix;
		public Term Term;
		public AstList Terms;
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class Term : IAstElement
	{
		public IFactor Factor;
		public AstList Factors;
		public void Accept(IAstVisitor v) => v.Visit(this);
	}

	public class Expr : IAstElement
	{
		public SimpleExpr SimpleExpr;
		public Relation Relation;
		public SimpleExpr SimpleExpr2;
		public void Accept(IAstVisitor v) => v.Visit(this);
	}

	public class Case : IAstElement
	{
		public CaseLabels CaseLabels;
		public AstList CaseLabelsList;
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
		}

		public class ProcCallStatement : IStatement
		{
			public Designator Designator;
			public ExprList ExprList;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class IfStatement : IStatement
		{
			public class IfThen
			{
				public Expr Cond;
				public StatementSeq ThenBody;
			}
			public IfThen If;
			public AstList ELSIFs;
			public StatementSeq ElseBody;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class CaseStatement : IStatement
		{
			public Expr Expr;
			public AstList Cases;
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
		public class WithStatement : IStatement
		{
			public Guard Guard;
			public StatementSeq StatementSeq;
			public AstList AdditionalGuards;
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
	
	public abstract class Number : IAstElement
	{
		public string Value;
		public void Accept(IAstVisitor v) => v.Visit(this);
	}

	public interface IFactor : IAstElement
	{
		public class DesignatorFactor : IFactor
		{
			public Designator Value;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class NumberFactor : IFactor
		{
			public Number Value;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class CharacterFactor : IFactor
		{
			public Char Value;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class StringFactor : IFactor
		{
			public string Value;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class NilFactor : IFactor
		{
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class SetFactor : IFactor
		{
			public Set Value;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class ExprFactor : IFactor
		{
			public Expr Value;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class NegFactor : IFactor
		{
			public IFactor Value;
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
	}

	public abstract class Designator : IAstElement
	{
		public interface IDesignatorSpec : IAstElement
		{
            public class RecordDesignatorSpec : IDesignatorSpec {
				public Ident Value;
				public void Accept(IAstVisitor v) => v.Visit(this);
			}
			public class ArrayDesignatorSpec : IDesignatorSpec {
				public ExprList Value;
				public void Accept(IAstVisitor v) => v.Visit(this);
			}
			public class PointerDesignatorSpec : IDesignatorSpec {
				public void Accept(IAstVisitor v) => v.Visit(this);
			}
			public class CastDesignatorSpec : IDesignatorSpec {
				public Qualident Value;
				public void Accept(IAstVisitor v) => v.Visit(this);
			}
			public class ProcCallDesignatorSpec : IDesignatorSpec {
				public ExprList Value;
				public void Accept(IAstVisitor v) => v.Visit(this);
			}
		}

		public Qualident Qualident;
		public bool EndOfLine;
		public AstList DesignatorSpecs;
		public void Accept(IAstVisitor v) => v.Visit(this);
	}

}