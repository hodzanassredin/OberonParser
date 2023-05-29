namespace CPParser.Ast
{

	public interface IAstElement
	{
		void Accept(IAstVisitor v);
	}

	public abstract class AstList<T> where T : IAstElement
	{
		public List<T> Value { get; set; } = new List<T>();
    }

	public class Ident : IAstElement
	{
		public string Name { get; set; }

        public void Accept(IAstVisitor v) => v.Visit(this);
    }

	public class Qualident : IAstElement
	{
		public List<Ident> Idents { get; set; }
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
		public ImportList ImportList;
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
	public class ImportList : IAstElement
	{
        public List<Import> Imports { get; set; } = new List<Import>();
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
		public Ident Ident { get; set; }
		public void SetExport(String c) {
            switch (c)
            {
				case "*":
					Export = IdentExport.Export;
					break;
				case "-":
					Export = IdentExport.ExportReadonly;
					break;
				default:
					Export = IdentExport.Private;
					break;
            }
        }
		public IdentExport Export { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public interface IConstTypeVarListDecl : IAstElement {
		public class ConstDeclList : AstList<ConstDecl>, IAstElement, IConstTypeVarListDecl
		{
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class TypeDeclList : AstList<TypeDecl>, IAstElement, IConstTypeVarListDecl
		{
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class VarDeclList : AstList<VarDecl>, IAstElement, IConstTypeVarListDecl
		{
			public void Accept(IAstVisitor v) => v.Visit(this);
		}

	}
	public interface IProcForwardDecl : IAstElement { 
	
	}
	public class DeclSeq : IAstElement
	{
		public List<IConstTypeVarListDecl> ConstTypeVarDecls { get; set; } = new List<IConstTypeVarListDecl>();
		public List<IProcForwardDecl> ProcForwardDecls { get; set; } = new List<IProcForwardDecl>();
		public void Accept(IAstVisitor v) => v.Visit(this);
	}


	public class ConstDecl : IAstElement
	{
		public IdentDef IdentDef { get; set; }
		public ConstExpr ConstExpr { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class TypeDecl : IAstElement
	{
		public IdentDef IdentDef { get; set; }
		public IType Type_ { get; set; }
        public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class VarDecl : IAstElement
	{
		public IdentList IdentList { get; set; }
		public IType Type_ { get; set; }
        public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class ProcDecl : IAstElement, IProcForwardDecl
	{
		public Receiver Receiver { get; set; }
		public IdentDef IdentDef { get; set; }
		public FormalPars FormalPars { get; set; }
		public MethAttributes MethAttributes { get; set; }
        public StatementSeq StatementSeq { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}


	public class MethAttributes : IAstElement
	{
		public enum MethodAttr
		{
			ABSTRACT, EMPTY, EXTENSIBLE
		}
		public bool IsNew { get; set; }
		public MethodAttr? Attr { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class ForwardDecl : IAstElement, IProcForwardDecl
	{
		public Receiver Receiver { get; set; }
		public IdentDef IdentDef { get; set; }
		public FormalPars FormalPars { get; set; }
		public MethAttributes MethAttributes { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class FormalPars : IAstElement
	{
        public List<FPSection> FPSections { get; set; }
        public IType Type_ { get; set; }
        public void Accept(IAstVisitor v) => v.Visit(this);
	}

	

	public class FPSection : IAstElement
	{
		public enum Prefix
		{
			VAR, IN, OUT
		}
		public Prefix? FpSectionPrefix { get; set; }
        public List<Ident> Idents { get; set; }
		public IType Type_ { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	
	public class Receiver : IAstElement
	{
		public enum Prefix
		{
			VAR, IN
		}
		public Prefix? ReceiverPrefix { get; set; }
		public Ident SelfIdent { get; set; }
		public Ident TypeIdent { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
    public interface IType : IAstElement
    {
		public class SynonimType : IType
		{
			public Qualident Qualident { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class ArrayType : IType
		{
			public List<ConstExpr> ConstExprs { get; set; }
			public IType Type_ { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}

		public class RecordType : IType
		{
			public enum Meta
			{
				ABSTRACT, EXTENSIBLE, LIMITED
			}
			public Meta RecordMeta { get; set; }
			public Qualident Qualident { get; set; }
			public List<FieldList> FieldList { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);

		}
		public class PointerType : IType
		{
			public IType Type_ { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class ProcedureType : IType
		{
			public FormalPars FormalPars { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
	}
    

	public class ExprList : IAstElement { 
		public List<Expr> Exprs { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class IdentList : IAstElement
	{
		public List<IdentDef> IdentDefs { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class FieldList : IAstElement
	{
		public IdentList IdentList { get; set; }
		public IType Type_ { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	
	
	public class ConstExpr : IAstElement { 
		public Expr Expr { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class CaseLabels : IAstElement
	{
		public List<ConstExpr> ConstExprs { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class StatementSeq : IAstElement
	{
		public List<IStatement> Statements { get; set; }
        public void Accept(IAstVisitor v) => v.Visit(this);
    }
	public class Set : IAstElement { 
		public List<Element> Elements { get; set; }
        public void Accept(IAstVisitor v) => v.Visit(this);		
	}
	public class Element : IAstElement { 
		public List<Expr> Exprs { get; set; }
        public void Accept(IAstVisitor v) => v.Visit(this); 
	}
	
	public class AddOp : IAstElement
	{
		public enum AddOps
		{
			Add, Sub, Or
		}

		public AddOps Op { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);

	}

	public class MulOp : IAstElement
	{
		public enum MulOps
		{
			Mul, Division, DIV, MOD, AND
		}

		public MulOps Op { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	
	public class Relation : IAstElement
	{
		public enum Relations
		{
			Eq, Neq, Lss, Leq, Gtr, Geq, In, Is
		}
		public Relations Op { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	
	public class SimpleExpr : IAstElement
	{
		public enum SimpleExprPrefix
		{
			Add, Sub
		}
		public void SetPrefix(string c)
		{
			switch (c)
			{
				case "+":
					Prefix = SimpleExprPrefix.Add;
					break;
				case "-":
					Prefix = SimpleExprPrefix.Sub;
					break;
				default:
					throw new Exception();
			}

		}
		public SimpleExprPrefix? Prefix { get; set; }
		public Term Term { get; set; }
		public List<(AddOp, Term)> Terms { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}
	public class Term : IAstElement
	{
		public IFactor Factor { get; set; }
		public List<(MulOp, IFactor)> Factors { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}

	public class Expr : IAstElement
	{
		public SimpleExpr SimpleExpr { get; set; }
		public Relation Relation { get; set; }
		public SimpleExpr SimpleExpr2 { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}

	public class Case : IAstElement
	{
		public CaseLabels CaseLabels { get; set; }
		public List<CaseLabels> CaseLabelsList { get; set; }
		public StatementSeq StatementSeq { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);

	}
	public interface IStatement : IAstElement
	{

		public class AssignmentStatement : IStatement
		{
			public Designator Designator { get; set; }
			public Expr Expr { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}

		public class ProcCallStatement : IStatement
		{
			public Designator Designator { get; set; }
			public ExprList ExprList { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class IfStatement : IStatement
		{
			public class IfThen
			{
				public Expr Cond { get; set; }
				public StatementSeq ThenBody { get; set; }
			}
			public IfThen If { get; set; }
			public List<IfThen> ELSIFs { get; set; }
			public StatementSeq ElseBody { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class CaseStatement : IStatement
		{
			public Expr Expr { get; set; }
			public List<Case> Cases { get; set; }
			public StatementSeq ElseBody { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class WhileStatement : IStatement
		{
			public Expr Expr { get; set; }
			public StatementSeq StatementSeq { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class RepeatStatement : IStatement
		{
			public StatementSeq StatementSeq { get; set; }
			public Expr Expr { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class ForStatement : IStatement
		{
			public Ident Ident { get; set; }
			public Expr Expr { get; set; }
			public Expr ToExpr { get; set; }
			public ConstExpr ByExpr { get; set; }
			public StatementSeq StatementSeq { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class LoopStatement : IStatement
		{
			public StatementSeq StatementSeq { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class WithStatement : IStatement
		{
			public Guard Guard { get; set; }
			public StatementSeq StatementSeq { get; set; }
			public List<(Guard, StatementSeq)> AdditionalGuards { get; set; }
			public StatementSeq ElseStatementSeq { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class ExitStatement : IStatement
		{
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class ReturnStatement : IStatement
		{
			public Expr Expr { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
	}
	
	public abstract class Number : IAstElement
	{
        public string Value { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}

	public interface IFactor : IAstElement
	{
		public class DesignatorFactor : IFactor
		{ 
			public Designator Value { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class NumberFactor : IFactor
		{ 
			public Number Value { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class CharacterFactor : IFactor
		{ 
			public Char Value { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class StringFactor : IFactor
		{ 
			public string Value { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class NilFactor : IFactor
		{
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class SetFactor : IFactor
		{ 
			public Set Value { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class ExprFactor : IFactor
		{ 
			public Expr Value { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
		public class NegFactor : IFactor
		{ 
			public IFactor Value { get; set; }
			public void Accept(IAstVisitor v) => v.Visit(this);
		}
	}

	public abstract class Designator : IAstElement
	{
		public interface IDesignatorSpec : IAstElement
		{
            public class RecordDesignatorSpec : IDesignatorSpec { 
				public Ident Value { get; set; }
				public void Accept(IAstVisitor v) => v.Visit(this);
			}
			public class ArrayDesignatorSpec : IDesignatorSpec { 
				public ExprList Value { get; set; }
				public void Accept(IAstVisitor v) => v.Visit(this);
			}
			public class PointerDesignatorSpec : IDesignatorSpec {
				public void Accept(IAstVisitor v) => v.Visit(this);
			}
			public class CastDesignatorSpec : IDesignatorSpec { 
				public Qualident Value { get; set; }
				public void Accept(IAstVisitor v) => v.Visit(this);
			}
			public class ProcCallDesignatorSpec : IDesignatorSpec { 
				public ExprList Value { get; set; }
				public void Accept(IAstVisitor v) => v.Visit(this);
			}
		}

        public Qualident Qualident { get; set; }
        public bool EndOfLine { get; set; }
        public List<IDesignatorSpec> DesignatorSpecs { get; set; }
		public void Accept(IAstVisitor v) => v.Visit(this);
	}

}