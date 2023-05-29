namespace Ast {

	class Ident
	{
		public string Name { get; set; }
	}

	class Qualident
	{
		public List<Ident> Idents { get; set; }
	}
	class Guard
	{
		public Qualident VarQualident { get; set; }
		public Qualident TypeQualident { get; set; }
	}
	class Module {
		public Ident Ident { get; set; }
        public ImportList ImportList { get; set; }
		public DeclSeq DeclSeq { get; set; }
		public StatementSeq Begin { get; set; }
		public StatementSeq Close { get; set; }
	}

	class ImportList {
        public Ident Name { get; set; }
		public Ident OriginalName { get; set; }
	}

	enum IdentExport { 
		Private,
		ExportReadonly,
		Export
	}
	class IdentDef
	{
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
	}
	class ConstTypeVarDecl { }
	class ProcForwardDecl { }
	class DeclSeq {
		public List<ConstTypeVarDecl> ConstTypeVarDecls { get; set; }
		public List<ProcForwardDecl> ProcForwardDecls { get; set; }
	}
	class ConstDecl {
		public IdentDef IdentDef { get; set; }
		public ConstExpr ConstExpr { get; set; }
	}
	class TypeDecl {
		public IdentDef IdentDef { get; set; }
		public Type_ Type_ { get; set; }
	}
	class VarDecl {
		public IdentList IdentList { get; set; }
		public Type_ Type_ { get; set; }
	}
	class ProcDecl
	{
		public Receiver Receiver { get; set; }
		public IdentDef IdentDef { get; set; }
		public FormalPars FormalPars { get; set; }
		public MethAttributes MethAttributes { get; set; }
        public StatementSeq StatementSeq { get; set; }
	}

	enum MethodAttr {
		ABSTRACT , EMPTY , EXTENSIBLE
	}
	class MethAttributes {
		public bool IsNew { get; set; }
		public MethodAttr? MethodAttr { get; set; }
	}
	class ForwardDecl {
		public Receiver Receiver { get; set; }
		public IdentDef IdentDef { get; set; }
		public FormalPars FormalPars { get; set; }
		public MethAttributes MethAttributes { get; set; }
	}
	class FormalPars {
        public List<FPSection> FPSections { get; set; }
        public Type_ Type_ { get; set; }
	}

	enum FpSectionPrefix {
		VAR , IN , OUT
	}

	class FPSection {
        public FpSectionPrefix? FpSectionPrefix { get; set; }
        public List<Ident> Idents { get; set; }
		public Type_ Type_ { get; set; }
	}
	enum ReceiverPrefix
	{
		VAR, IN
	}
	class Receiver {
		public ReceiverPrefix? ReceiverPrefix { get; set; }
		public Ident SelfIdent { get; set; }
		public Ident TypeIdent { get; set; }
	}
	abstract class Type_
	{
	}
	class SynonimType : Type_
	{
        public Qualident Qualident { get; set; }
    }
	class ArrayType : Type_
	{
		public List<ConstExpr> ConstExprs { get; set; }
		public Type_ Type_ { get; set; }
	}

	class ExprList { public List<Expr> Exprs { get; set; } }
	class IdentList
	{
		public List<IdentDef> IdentDefs { get; set; }
	}
	class FieldList
    {
		public IdentList IdentList { get; set; }
		public Type_ Type_ { get; set; }
	}
	enum RecordMeta
	{
		ABSTRACT, EXTENSIBLE, LIMITED
	}
	class RecordType {
		public RecordMeta RecordMeta { get; set; }
		public Qualident Qualident { get; set; }
		public List<FieldList> FieldList { get; set; }
	}
	class PointerType : Type_
	{
		public Type_ Type_ { get; set; }
	}
	class ProcedureType : Type_ {
		public FormalPars FormalPars { get; set; }
	}
	class ConstExpr { public Expr Expr { get; set; } }
	class CaseLabels
	{
		public List<ConstExpr> ConstExprs { get; set; }
	}
	class StatementSeq {
		public List<Statement> Statements { get; set; }
    }
	class Set { public List<Element> Elements { get; set; } }
	class Element { public List<Expr> Exprs { get; set; } }
	enum AddOps
	{
		Add, Sub, Or
	}
	class AddOp
	{

		public void SetOp(String c)
		{
			switch (c)
			{
				case "+":
					Op = AddOps.Add;
					break;
				case "-":
					Op = AddOps.Sub;
					break;
				case "OR":
					Op = AddOps.Or;
					break;
				default:
					throw new Exception();
			}
		}
		public AddOps Op { get; set; }
	
	}
	enum MulOps
	{
		Mul, Division, DIV, MOD, AND
	}
	class MulOp
	{

		public void SetOp(String c)
		{
			switch (c)
			{
				case "*":
					Op = MulOps.Mul;
					break;
				case "/":
					Op = MulOps.Division;
					break;
				case "DIV":
					Op = MulOps.DIV;
					break;
				case "MOD":
					Op = MulOps.MOD;
					break;
				case "&":
					Op = MulOps.AND;
					break;
				default:
					throw new Exception();
			}
		}
		public MulOps Op { get; set; }

	}
	enum Relations
	{
		Eq, Neq, Lss, Leq, Gtr, Geq, In, Is
	}
	class Relation
	{
		public void Set(String c)
		{
			switch (c)
			{
				case "=":
					Op = Relations.Eq;
					break;
				case "#":
					Op = Relations.Neq;
					break;
				case "<":
					Op = Relations.Lss;
					break;
				case "<=":
					Op = Relations.Leq;
					break;
				case ">":
					Op = Relations.Gtr;
					break;
				case ">=":
					Op = Relations.Geq;
					break;
				case "IN":
					Op = Relations.In;
					break;
				case "IS":
					Op = Relations.Is;
					break;
				default:
					throw new Exception();
			}
		}
		public Relations Op { get; set; }

	}
	enum SimpleExprPrefix
	{
		Add, Sub
	}
	class SimpleExpr
	{
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
	}
	class Term
	{
		public Factor Factor { get; set; }
		public List<(MulOp, Factor)> Factors { get; set; }
	}

	class Expr
	{
		public SimpleExpr SimpleExpr { get; set; }
		public Relation Relation { get; set; }
		public SimpleExpr SimpleExpr2 { get; set; }
	}

	class Case
	{
		public CaseLabels CaseLabels { get; set; }
		public List<CaseLabels> CaseLabelsList { get; set; }
		public StatementSeq StatementSeq { get; set; }

	}
	abstract class Statement { }
	class AssignmentStatement : Statement
	{
        public Designator Designator { get; set; }
        public Expr Expr { get; set; }
	}

	class ProcCallStatement : Statement {
        public Designator Designator { get; set; }
        public ExprList ExprList { get; set; }
	}
	class IfStatement : Statement
	{
		public class IfThen {
			public Expr Cond { get; set; }
			public StatementSeq ThenBody { get; set; }
		}
        public IfThen If { get; set; }
        public List<IfThen> ELSIFs { get; set; }
        public StatementSeq ElseBody { get; set; }
	}
	class CaseStatement : Statement
	{
        public Expr Expr { get; set; }
        public List<Case> Cases { get; set; }
		public StatementSeq ElseBody { get; set; }
	}
	class WhileStatement : Statement
	{
		public Expr Expr { get; set; }
		public StatementSeq StatementSeq { get; set; }
	}
	class RepeatStatement : Statement {
        public StatementSeq StatementSeq { get; set; }
        public Expr Expr { get; set; }
	}
	class ForStatement : Statement {
        public Ident Ident { get; set; }
        public Expr Expr { get; set; }
        public Expr ToExpr { get; set; }
		public ConstExpr ByExpr { get; set; }
        public StatementSeq StatementSeq { get; set; }
	}
	class LoopStatement : Statement {
        public StatementSeq StatementSeq { get; set; }
    }
	class WithStatement : Statement {
        public Guard Guard { get; set; }
        public StatementSeq StatementSeq { get; set; }
        public List<(Guard, StatementSeq)> AdditionalGuards { get; set; }
        public StatementSeq ElseStatementSeq { get; set; }
	}
	class ExitStatement : Statement { }
	class ReturnStatement : Statement {
        public Expr Expr { get; set; }
    }
	abstract class Number {
        public string Value { get; set; }
    }
	abstract class Factor {
		class DesignatorFactor : Factor { public Designator Value { get; set; } }
		class NumberFactor : Factor { public Number Value { get; set; } }
		class CharacterFactor : Factor { public Char Value { get; set; } }
		class StringFactor : Factor { public string Value { get; set; } }
		class NilFactor : Factor { }
		class SetFactor : Factor { public Set Value { get; set; } }
		class ExprFactor : Factor { public Expr Value { get; set; } }
		class NegFactor : Factor { public Factor Value { get; set; } }
	}

	abstract class Designator    {
		public abstract class DesignatorSpec
		{
            public class RecordDesignatorSpec : DesignatorSpec { public Ident Value { get; set; }}
			public class ArrayDesignatorSpec : DesignatorSpec { public ExprList Value { get; set; } }
			public class PointerDesignatorSpec : DesignatorSpec {  }
			public class CastDesignatorSpec : DesignatorSpec { public Qualident Value { get; set; } }
			public class ProcCallDesignatorSpec : DesignatorSpec { public ExprList Value { get; set; } }
		}
	

        public Qualident Qualident { get; set; }
        public bool EndOfLine { get; set; }
        public List<DesignatorSpec> DesignatorSpecs { get; set; }
    }

}