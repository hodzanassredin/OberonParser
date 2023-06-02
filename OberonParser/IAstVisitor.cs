﻿using AOParser.Ast;

namespace AOParser
{
	public interface IAstVisitor
	{
		void Visit(IStatement.IfStatement.IfThen o);
		void Visit(Ident o);
		void Visit(Qualident o);
		void Visit(Guard o);
		void Visit(Module o);
		void Visit(Import o);
		void Visit(IdentDef o);
		void Visit(IConstTypeVarListDecl o);
		void Visit(IProcForwardDecl o);
		void Visit(DeclSeq o);
		void Visit(IConstTypeVarListDecl.ConstDeclList o);
		void Visit(IConstTypeVarListDecl.TypeDeclList o);
		void Visit(IConstTypeVarListDecl.VarDeclList o);
		void Visit(ConstDecl o);
		void Visit(TypeDecl o);
		void Visit(VarDecl o);
		void Visit(ProcDecl o);
		void Visit(FormalPars o);
		void Visit(FPSection o);
		void Visit(ExprList o);
		void Visit(IdentList o);
		void Visit(FieldList o);
		void Visit(ConstExpr o);
		void Visit(CaseLabels o);
		void Visit(StatementSeq o);
		void Visit(Set o);
		void Visit(Element o);
		void Visit(AddOp o);
		void Visit(MulOp o);
		void Visit(Relation o);
		void Visit(SimpleExpr o);
		void Visit(Term o);
		void Visit(Expr o);
		void Visit(Case o);
		void Visit(IStatement.AssignmentStatement o);
		void Visit(IStatement.ProcCallStatement o);
		void Visit(IStatement.IfStatement o);
		void Visit(IStatement.CaseStatement o);
		void Visit(IStatement.WhileStatement o);
		void Visit(IStatement.LoopStatement o);
		void Visit(IStatement.RepeatStatement o);
		void Visit(IStatement.WithStatement o);
		void Visit(IStatement.ExitStatement o);
		void Visit(IStatement.ReturnStatement o);
		void Visit(IStatement.ForStatement o);
		void Visit(IType.ArrayType o);
		void Visit(IType.PointerType o);
		void Visit(IType.ProcedureType o);
		void Visit(IType.RecordType o);
		void Visit(IType.SynonimType o);
		void Visit(Number o);
		void Visit(IFactor.CharacterFactor o);
        void Visit(DefinitionProc definitionProc);
        void Visit(IFactor.DesignatorFactor o);
        void Visit(Definition definition);
        void Visit(IFactor.ExprFactor o);
		void Visit(IFactor.NegFactor o);
        void Visit(SysFlag sysFlag);
        void Visit(Body body);
        void Visit(IFactor.NilFactor o);
        void Visit(FieldDecl fieldDecl);
        void Visit(StatBlock statBlock);
        void Visit(ProcHead procHead);
        void Visit(IFactor.NumberFactor o);
		void Visit(IFactor.SetFactor o);
		void Visit(IFactor.StringFactor o);
		void Visit(Designator o);
		void Visit(Designator.IDesignatorSpec.RecordDesignatorSpec o);
		void Visit(Designator.IDesignatorSpec.ArrayDesignatorSpec o);
		void Visit(Designator.IDesignatorSpec.CastDesignatorSpec o);
		void Visit(Designator.IDesignatorSpec.PointerDesignatorSpec o);
		void Visit(Designator.IDesignatorSpec.ProcCallDesignatorSpec o);
        void Visit(SimpleElementExpr simpleElementExpr);
        void Visit(TermElementExpr termElementExpr);
        void Visit(IType.ObjectType objectType);
        void Visit(IStatement.AwaitStatement awaitStatement);
        void Visit(IStatement.StatBlockStatement statBlockStatement);
    }
}
