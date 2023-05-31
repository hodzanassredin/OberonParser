using CPParser.Ast;

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
            throw new NotImplementedException();
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
                sw.Write("BEGIN");
                EnterScope();
                o.Begin.Accept(this);
                ExitScope();
            }

            if (o.Close != null)
            {
                sw.Write("CLOSE");
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
            sw.WriteLine();

        }

        public void Visit(IConstTypeVarListDecl.TypeDeclList o)
        {
            WriteTabs();sw.WriteLine("TYPE");
            EnterScope();
            this.VisitList(o, () => { }, () => { sw.WriteLine(";"); }, true);
            ExitScope();
            sw.WriteLine();
        }

        public void Visit(IConstTypeVarListDecl.VarDeclList o)
        {
            WriteTabs();sw.WriteLine("VAR");
            EnterScope();
            this.VisitList(o, () => { }, () => { sw.WriteLine(";"); }, true);
            ExitScope();
            sw.WriteLine();
        }
        public void Visit(ConstDecl o)
        {
            o.IdentDef.Accept(this);sw.Write(" = ");o.ConstExpr.Accept(this);
        }

        public void Visit(TypeDecl o)
        {
            WriteTabs();o.IdentDef.Accept(this);sw.Write(" = ");o.Type_.Accept(this);
        }

        public void Visit(VarDecl o)
        {
            WriteTabs();o.IdentList.Accept(this);sw.Write(" : ");o.Type_.Accept(this);
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
            WriteTabs(); sw.Write("END ");
            o.IdentDef.Ident.Accept(this);
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
            sw.Write("^");
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
                sw.Write(":");
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
            }

            VisitList(o.Idents, () => { }, () => sw.Write(", "));
            sw.Write(" : ");
            o.Type_.Accept(this);
        }

        public void Visit(Receiver o)
        {
            throw new NotImplementedException();
        }

        public void Visit(ExprList o)
        {
            VisitList(o.Exprs, () => { }, () => sw.Write(", "));
        }

        public void Visit(IdentList o)
        {
            VisitList(o.IdentDefs, () => { }, () => sw.Write(", "));
        }

        private void VisitList(AstList lst, Action before, Action after, bool doAfterForLast = false) {
            for (int i = 0; i < lst.Value.Count; i++)
            {
                before();
                lst.Value[i]?.Accept(this);
                if (i != (lst.Value.Count - 1) || doAfterForLast) after();
            }
        }

        public void Visit(FieldList o)
        {
            throw new NotImplementedException();
        }

        public void Visit(ConstExpr o)
        {
            throw new NotImplementedException();
        }

        public void Visit(CaseLabels o)
        {
            throw new NotImplementedException();
        }

        public void Visit(StatementSeq o)
        {
            VisitList(o.Statements, () => {  }, ()=>sw.WriteLine(";"));
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
            throw new NotImplementedException();
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
                o.Relation.Accept(this);
                o.SimpleExpr2.Accept(this);
            }
        }

        public void Visit(Case o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IStatement.AssignmentStatement o)
        {
            WriteTabs();o.Designator.Accept(this);
            sw.Write(":=");
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
            WriteTabs();
            sw.Write("IF");
            o.If.Accept(this);
            VisitList(o.ELSIFs, () => sw.Write("ELSIF"), () => { });
            if (o.ElseBody != null) {
                sw.Write("ELSE");
                o.ElseBody.Accept(this);
            }
            sw.Write("END");
        }

        public void Visit(IStatement.CaseStatement o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IStatement.WhileStatement o)
        {
            WriteTabs(); sw.Write("WHILE");
                EnterScope();
                o.Expr.Accept(this);
                ExitScope();
            WriteTabs(); sw.Write("DO");
            EnterScope();
            o.StatementSeq.Accept(this);
            ExitScope();
            WriteTabs(); sw.Write("END");
        }

        public void Visit(IStatement.LoopStatement o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IStatement.RepeatStatement o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IStatement.WithStatement o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IStatement.ExitStatement o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IStatement.ReturnStatement o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IStatement.ForStatement o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IType.ArrayType o)
        {
            sw.Write("ARRAY ");
            if (o.ConstExprs != null)
            {
                VisitList(o.ConstExprs, () => { }, () => sw.Write(", "));
            }
            sw.Write("OF ");
            o.Type_.Accept(this);
        }

        public void Visit(IType.PointerType o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IType.ProcedureType o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IType.RecordType o)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void Visit(IFactor.DesignatorFactor o)
        {
            o.Value.Accept(this);
        }

        public void Visit(IFactor.ExprFactor o)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            sw.Write("THEN");
            o.ThenBody.Accept(this);
        }

        public void Visit(SimpleElementExpr o)
        {
            o.AddOp.Accept(this);
            o.Term.Accept(this);
        }

        public void Visit(TermElementExpr o)
        {
            o.MulOp.Accept(this);
            o.Factor.Accept(this);
        }

        public void Visit(IStatement.WithAlternativeStatement o)
        {
            throw new NotImplementedException();
        }
    }
}
