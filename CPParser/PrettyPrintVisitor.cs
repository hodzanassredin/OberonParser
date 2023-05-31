﻿using CPParser.Ast;

namespace CPParser
{
    public class PrettyPrintVisitor : IAstVisitor
    {
        private readonly StreamWriter sw;
        private int tabLevel;

        private void WriteTabs() {
            for (int i = 0; i < tabLevel; i++)
            {
                sw.Write("\t");
            }
        }

        private void EnterScope() => tabLevel++;
        private void ExitScope() => tabLevel--;

        public PrettyPrintVisitor(StreamWriter sw)
        {
            this.sw = sw;
            this.tabLevel = 0;
        } 
        public void Visit(Ident o)
        {
            sw.Write(o.Name);
        }

        public void Visit(Qualident o)
        {
            o.Ident1.Accept(this);
            if (o.Ident2 != null) {
                sw.Write('.');
                o.Ident2.Accept(this);
            }
        }

        public void Visit(Guard o)
        {
            o.VarQualident.Accept(this);
            sw.Write(" : ");
            o.TypeQualident.Accept(this);
        }

        public void Visit(Module o)
        {
            sw.WriteLine($"MODULE {o.Ident.Name};");
            sw.WriteLine();
            EnterScope();
            if (o.ImportList != null) {
                WriteTabs(); sw.Write("IMPORT ");
                VisitList(o.ImportList, () => { }, () => sw.Write(", "));
                sw.WriteLine(";");
                sw.WriteLine();
            }
            
            o.DeclSeq.Accept(this);
            ExitScope();
            if (o.Begin != null) {
                sw.WriteLine("BEGIN");
                EnterScope();
                o.Begin.Accept(this);
                ExitScope();
            }

            if (o.Close != null)
            {
                sw.WriteLine("CLOSE");
                EnterScope();
                o.Close.Accept(this);
                ExitScope();
            }
            
            sw.WriteLine($"END {o.Ident.Name}.");
        }

        public void Visit(IdentDef o)
        {
            o.Ident.Accept(this);

            switch (o.Export)
            {
                case IdentDef.IdentExport.Private:
                    break;
                case IdentDef.IdentExport.ExportReadonly:
                    sw.Write("-");
                    break;
                case IdentDef.IdentExport.Export:
                    sw.Write("*");
                    break;
                default:
                    break;
            }
        }

        public void Visit(IConstTypeVarListDecl o)
        {
            o.Accept(this);
        }

        public void Visit(IProcForwardDecl o)
        {
            o.Accept(this);
            sw.WriteLine(";");
        }

        public void Visit(DeclSeq o)
        {

            foreach (var item in o.ConstTypeVarDecls)
            {
                item.Accept(this);
                sw.WriteLine();
            }
            foreach (var item in o.ProcForwardDecls)
            {
                WriteTabs();sw.Write("PROCEDURE ");
                item.Accept(this);
                sw.WriteLine(";");
                sw.WriteLine();
            }
        }
        public void Visit(IConstTypeVarListDecl.ConstDeclList o)
        {
            WriteTabs();sw.WriteLine("CONST");
            EnterScope();
            this.VisitList(o, () => { }, () => { sw.WriteLine(";"); }, true);
            ExitScope();
        }

        public void Visit(IConstTypeVarListDecl.TypeDeclList o)
        {
            WriteTabs();sw.WriteLine("TYPE");
            EnterScope();
            this.VisitList(o, () => { }, () => { sw.WriteLine(";"); }, true);
            ExitScope();
        }

        public void Visit(IConstTypeVarListDecl.VarDeclList o)
        {
            WriteTabs();sw.WriteLine("VAR");
            EnterScope();
            this.VisitList(o, () => { }, () => { sw.WriteLine(";"); }, true);
            ExitScope();
        }
        public void Visit(ConstDecl o)
        {
            WriteTabs();o.IdentDef.Accept(this);sw.Write(" = ");o.ConstExpr.Accept(this);
        }

        public void Visit(TypeDecl o)
        {
            WriteTabs();o.IdentDef.Accept(this);sw.Write(" = ");o.Type_.Accept(this);
        }

        public void Visit(VarDecl o)
        {
            WriteTabs();o.IdentList.Accept(this);sw.Write(": ");o.Type_.Accept(this);
        }

        public void Visit(ProcDecl o)
        {
            if (o.Receiver != null)
            {
                o.Receiver?.Accept(this);
                sw.Write(" ");
            }
            
            o.IdentDef.Accept(this);
            sw.Write(" ");
            o.FormalPars?.Accept(this);
            o.MethAttributes.Accept(this);
            sw.WriteLine(";");
            EnterScope();
            o.DeclSeq.Accept(this);
            ExitScope();
            if (o.StatementSeq != null) {
                WriteTabs(); sw.WriteLine("BEGIN");
                EnterScope();
                o.StatementSeq.Accept(this);
                ExitScope();
            }
            WriteTabs();sw.Write("END ");o.IdentDef.Ident.Accept(this);
        }

        public void Visit(MethAttributes o)
        {
            if (o.IsNew) {
                sw.Write(", NEW");
            }
            if (o.Attr != null) {
                switch (o.Attr.Value)
                {
                    case MethAttributes.MethodAttr.ABSTRACT:
                        sw.Write(", ABSTRACT");
                        break;
                    case MethAttributes.MethodAttr.EMPTY:
                        sw.Write(", EMPTY");
                        break;
                    case MethAttributes.MethodAttr.EXTENSIBLE:
                        sw.Write(", EXTENSIBLE");
                        break;
                    default:
                        break;
                }
            }
        }

        public void Visit(ForwardDecl o)
        {
            sw.Write("^ ");
            if (o.Receiver != null)
            {
                o.Receiver?.Accept(this);
                sw.Write(" ");
            }

            o.IdentDef.Accept(this);
            sw.Write(" ");
            o.FormalPars?.Accept(this);
            o.MethAttributes.Accept(this);
        }

        public void Visit(FormalPars o)
        {
            sw.Write("(");
            VisitList(o.FPSections, () => { }, ()=> sw.Write("; "), false);
            
            sw.Write(")");
            if (o.Type_ != null) {
                sw.Write(": ");
                o.Type_.Accept(this);
            }
        }

        public void Visit(FPSection o)
        {
            if (o.FpSectionPrefix != null) {
                switch (o.FpSectionPrefix)
                {
                    case FPSection.Prefix.VAR:
                        sw.Write("VAR");
                        break;
                    case FPSection.Prefix.IN:
                        sw.Write("IN");
                        break;
                    case FPSection.Prefix.OUT:
                        sw.Write("OUT");
                        break;
                    default:
                        break;
                }
                sw.Write(" ");
            }
            
            VisitList(o.Idents, () => { }, () => sw.Write(", "));
            sw.Write(" : ");
            o.Type_.Accept(this);
        }

        public void Visit(Receiver o)
        {
            sw.Write("(");
            if (o.ReceiverPrefix != null) {
                switch (o.ReceiverPrefix.Value)
                {
                    case Receiver.Prefix.VAR:
                        sw.Write("VAR ");
                        break;
                    case Receiver.Prefix.IN:
                        sw.Write("IN ");
                        break;
                    default:
                        break;
                }
            }
            o.SelfIdent.Accept(this);
            sw.Write(": ");
            o.TypeIdent.Accept(this);
            sw.Write(")");
        }

        public void Visit(ExprList o)
        {
            VisitList(o.Exprs, () => { }, () => sw.Write(", "));
        }

        public void Visit(IdentList o)
        {
            VisitList(o.IdentDefs, () => { }, () => sw.Write(", "));
        }

        private void VisitList(AstList lst, Action before, Action after, bool doAfterForLast = false, bool removeNulls = false) {
            var stats = removeNulls ? lst.Value.Where(x => x != null).ToList() : lst.Value;
            
            for (int i = 0; i < stats.Count; i++)
            {
                before();
                stats[i]?.Accept(this);
                if (i != (stats.Count - 1) || doAfterForLast) after();
            }
        }

        public void Visit(FieldList o)
        {
            if (o.IdentList != null)
            {
                WriteTabs();
                o.IdentList.Accept(this);
                sw.Write(" : ");
                o.Type_.Accept(this);
            }
        }

        public void Visit(ConstExpr o)
        {
            o.Expr.Accept(this);
        }

        public void Visit(CaseLabels o)
        {
            o.ConstExpr1.Accept(this);
            if (o.ConstExpr2 != null) {
                sw.Write("..");
                o.ConstExpr2.Accept(this);
            }
        }

        public void Visit(StatementSeq o)
        {
            VisitList(o.Statements, () => {  }, ()=>sw.WriteLine(";"), removeNulls:true);
            sw.WriteLine();
        }

        public void Visit(Set o)
        {
            sw.Write("{");
            VisitList(o.Elements, () => { }, () => sw.Write(", "));
            sw.Write("}");
        }

        public void Visit(Element o)
        {
            o.Expr1.Accept(this);
            if (o.Expr2 != null) {
                sw.Write("..");
                o.Expr2.Accept(this);
            }
        }

        public void Visit(AddOp o)
        {
            switch (o.Op)
            {
                case AddOp.AddOps.Add:
                    sw.Write(" + ");
                    break;
                case AddOp.AddOps.Sub:
                    sw.Write(" - ");
                    break;
                case AddOp.AddOps.Or:
                    sw.Write(" OR ");
                    break;
                default:
                    break;
            }
        }

        public void Visit(MulOp o)
        {
            switch (o.Op)
            {
                case MulOp.MulOps.Mul:
                    sw.Write("*");
                    break;
                case MulOp.MulOps.Division:
                    sw.Write("/");
                    break;
                case MulOp.MulOps.DIV:
                    sw.Write("DIV");
                    break;
                case MulOp.MulOps.MOD:
                    sw.Write("MOD");
                    break;
                case MulOp.MulOps.AND:
                    sw.Write("&");
                    break;
                default:
                    break;
            }
        }

        public void Visit(Relation o)
        {
            switch (o.Op)
            {
                case Relation.Relations.Eq:
                    sw.Write("=");
                    break;
                case Relation.Relations.Neq:
                    sw.Write("#");
                    break;
                case Relation.Relations.Lss:
                    sw.Write("<");
                    break;
                case Relation.Relations.Leq:
                    sw.Write("<=");
                    break;
                case Relation.Relations.Gtr:
                    sw.Write(">");
                    break;
                case Relation.Relations.Geq:
                    sw.Write(">=");
                    break;
                case Relation.Relations.In:
                    sw.Write("IN");
                    break;
                case Relation.Relations.Is:
                    sw.Write("IS");
                    break;
                default:
                    break;
            }
        }

        public void Visit(SimpleExpr o)
        {
            if (o.Prefix != null) {
                switch (o.Prefix.Value)
                {
                    case SimpleExpr.SimpleExprPrefix.Add:
                        sw.Write("+");
                        break;
                    case SimpleExpr.SimpleExprPrefix.Sub:
                        sw.Write("-");
                        break;
                    default:
                        break;
                }
            }
            o.Term.Accept(this);
            VisitList(o.SimpleExprElements, () => { }, () => { });
        }

        public void Visit(Term o)
        {
            o.Factor.Accept(this);
            VisitList(o.TermElements, () => { }, () => { });
        }

        public void Visit(Expr o)
        {
            o.SimpleExpr.Accept(this);
            if (o.Relation != null) { 
                sw.Write(' ');
                o.Relation.Accept(this);
                sw.Write(' ');
                o.SimpleExpr2.Accept(this);
            }
        }

        public void Visit(Case o)
        {
            if (o.CaseLabels.Any()) {
                VisitList(o.CaseLabels, () => { }, () => sw.Write(","));
                sw.Write(" : ");
                o.StatementSeq.Accept(this);
            }
        }

        public void Visit(IStatement.AssignmentStatement o)
        {
            WriteTabs();o.Designator.Accept(this);
            sw.Write(" := ");
            o.Expr.Accept(this);
        }

        public void Visit(IStatement.ProcCallStatement o)
        {
            WriteTabs();o.Designator.Accept(this);
            if (o.ExprList != null)
            {
                sw.Write("(");
                o.ExprList.Accept(this);
                sw.Write(")");
            }
        }

        public void Visit(IStatement.IfStatement o)
        {
            WriteTabs();sw.Write("IF ");o.If.Accept(this);
            VisitList(o.ELSIFs, () => { WriteTabs(); sw.Write("ELSIF"); }, () => { });
            if (o.ElseBody != null) {
                WriteTabs(); sw.WriteLine("ELSE");
                EnterScope();
                o.ElseBody.Accept(this);
                ExitScope();
            }
            WriteTabs(); sw.Write("END");
        }

        public void Visit(IStatement.CaseStatement o)
        {
            WriteTabs(); sw.Write("CASE "); o.Expr.Accept(this); sw.WriteLine(" OF");
            for (int i = 0; i < o.Cases.Value.Count; i++)
            {
                WriteTabs();
                if (i != 0)
                {
                    sw.Write("|"); 
                }
                sw.Write("\t"); o.Cases.Value[i].Accept(this);
            }
            if (o.ElseBody != null) {
                WriteTabs(); sw.WriteLine("ELSE");
                EnterScope();
                o.ElseBody.Accept (this);
                ExitScope();
            }
            WriteTabs(); sw.Write("END");
        }

        public void Visit(IStatement.WhileStatement o)
        {
            WriteTabs();sw.Write("WHILE ");o.Expr.Accept(this);sw.WriteLine(" DO");
            EnterScope();
            o.StatementSeq.Accept(this);
            ExitScope();
            WriteTabs(); sw.Write("END");
        }

        public void Visit(IStatement.LoopStatement o)
        {
            WriteTabs(); sw.WriteLine("LOOP");
            EnterScope();
            o.StatementSeq.Accept(this);
            ExitScope();
            WriteTabs(); sw.Write("END");
        }

        public void Visit(IStatement.RepeatStatement o)
        {
            WriteTabs(); sw.WriteLine("REPEAT");
            EnterScope();
            o.StatementSeq.Accept(this);
            ExitScope();
            WriteTabs(); sw.Write("UNTIL "); o.Expr.Accept(this);
        }

        public void Visit(IStatement.WithStatement o)
        {
            WriteTabs(); sw.Write("WITH ");
            for (int i = 0; i < o.Alternatives.Value.Count; i++)
            {
                if (i != 0) {
                    WriteTabs(); sw.Write("|");
                }
                EnterScope();
                o.Alternatives.Value[i].Accept(this);
                ExitScope();
            }
            if (o.ElseStatementSeq != null)
            {
                WriteTabs(); sw.WriteLine("ELSE");
                EnterScope();
                o.ElseStatementSeq.Accept(this);
                ExitScope();
            }
            WriteTabs(); sw.Write("END");
        }

        public void Visit(IStatement.ExitStatement o)
        {
            WriteTabs(); sw.Write("EXIT");
        }

        public void Visit(IStatement.ReturnStatement o)
        {
            WriteTabs(); sw.Write("RETURN"); 
            if (o.Expr != null)
            {
                sw.Write(" ");
                o.Expr.Accept(this);
            }
            
        }

        public void Visit(IStatement.ForStatement o)
        {
            WriteTabs(); sw.Write("FOR "); o.Ident.Accept(this); 
                sw.Write(" := "); o.Expr.Accept(this); sw.Write(" TO "); o.ToExpr.Accept(this); 
            if (o.ByExpr != null) {
                sw.Write(" BY "); o.ByExpr.Accept(this);
            }
            sw.WriteLine(" DO");
            EnterScope();
            o.StatementSeq.Accept(this);
            ExitScope();
            WriteTabs(); sw.Write("END");
        }

        public void Visit(IType.ArrayType o)
        {
            sw.Write("ARRAY ");
            if (o.ConstExprs.Any())
            {
                VisitList(o.ConstExprs, () => { }, () => sw.Write(", "));
                sw.Write(" ");
            }
            sw.Write("OF ");
            o.Type_.Accept(this);
        }

        public void Visit(IType.PointerType o)
        {
            sw.Write("POINTER TO ");
            o.Type_.Accept(this);
        }

        public void Visit(IType.ProcedureType o)
        {
            sw.Write("PROCEDURE");
            o.FormalPars?.Accept(this);
        }

        public void Visit(IType.RecordType o)
        {
            if (o.RecordMeta.HasValue)
            {
                switch (o.RecordMeta.Value)
                {
                    case IType.RecordType.Meta.ABSTRACT:
                        sw.Write("ABSTRACT ");
                        break;
                    case IType.RecordType.Meta.EXTENSIBLE:
                        sw.Write("EXTENSIBLE ");
                        break;
                    case IType.RecordType.Meta.LIMITED:
                        sw.Write("LIMITED ");
                        break;
                    default:
                        break;
                }
            }
            sw.Write("RECORD");
            if (o.Qualident != null) {
                sw.Write("(");
                o.Qualident.Accept(this);
                sw.Write(")");
            }
            if (o.FieldList.Any())
            {
                sw.WriteLine();
                EnterScope();
                VisitList(o.FieldList, () => { }, () => sw.WriteLine(";"));
                ExitScope();
                sw.WriteLine();
                WriteTabs(); sw.Write("END");
            }
            else {
                sw.Write(" END");
            }

            
        }

        public void Visit(IType.SynonimType o)
        {
            o.Qualident.Accept(this);
        }

        public void Visit(Number o)
        {
            sw.Write(o.Value);
        }

        public void Visit(IFactor.CharacterFactor o)
        {
            sw.Write(o.Value);
        }

        public void Visit(IFactor.DesignatorFactor o)
        {
            o.Value.Accept(this);
        }

        public void Visit(IFactor.ExprFactor o)
        {
            sw.Write("("); o.Value.Accept(this); sw.Write(")");
        }

        public void Visit(IFactor.NegFactor o)
        {
            sw.Write("~");
            o.Value.Accept(this);
        }

        public void Visit(IFactor.NilFactor o)
        {
            sw.Write("NIL");
        }

        public void Visit(IFactor.NumberFactor o)
        {
            o.Value.Accept(this);
        }

        public void Visit(IFactor.SetFactor o)
        {
            o.Value.Accept(this);
        }

        public async void Visit(IFactor.StringFactor o)
        {
            sw.Write(o.Value);
        }

        public void Visit(Designator o)
        {
            o.Qualident.Accept(this);
            VisitList(o.Specs, () => { }, () => { });
            if (o.EndOfLine) {
                sw.Write("$");
            }
        }

        public void Visit(Designator.IDesignatorSpec.RecordDesignatorSpec o)
        {
            sw.Write(".");
            o.Value.Accept(this);
        }

        public void Visit(Designator.IDesignatorSpec.ArrayDesignatorSpec o)
        {
            sw.Write("[");
            o.Value.Accept(this);
            sw.Write("]");
        }

        public void Visit(Designator.IDesignatorSpec.CastDesignatorSpec o)
        {
            throw new NotImplementedException();
        }

        public void Visit(Designator.IDesignatorSpec.PointerDesignatorSpec o)
        {
            sw.Write("^");
        }

        public void Visit(Designator.IDesignatorSpec.ProcCallDesignatorSpec o)
        {
            sw.Write("(");
            o.Value?.Accept(this);
            sw.Write(")");
        }

        public void Visit(Import o)
        {
            sw.Write(o.Name.Name);
            if (o.OriginalName != null)
            {
                sw.Write(" := ");
                sw.Write(o.OriginalName.Name);
            }
            
        }

        public void Visit(IStatement.IfStatement.IfThen o)
        {
            o.Cond.Accept(this);
            sw.WriteLine(" THEN");
            EnterScope();
            o.ThenBody.Accept(this);
            ExitScope();
        }

        public void Visit(SimpleElementExpr o)
        {
            o.AddOp.Accept(this);
            o.Term.Accept(this);
        }

        public void Visit(TermElementExpr o)
        {
            sw.Write(" ");o.MulOp.Accept(this);sw.Write(" ");o.Factor.Accept(this);
        }

        public void Visit(IStatement.WithAlternativeStatement o)
        {
            if (o.Guard != null) {
                o.Guard.Accept(this); sw.WriteLine(" DO");
                o.StatementSeq.Accept(this);
            }
        }
    }
}
