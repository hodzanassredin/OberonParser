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
        private void ExitScope() => tabLevel++;

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
            if (o.Qualifier != null) {
                o.Qualifier.Accept(this);
                sw.Write('.');
            }
            o.Ident.Accept(this);
        }

        public void Visit(Guard o)
        {
            throw new NotImplementedException();
        }

        public void Visit(Module o)
        {
            sw.WriteLine($"MODULE {o.Ident.Name};");
            EnterScope();
            if (o.ImportList != null) {
                WriteTabs(); sw.Write("IMPORT ");
                VisitList(o.ImportList, () => { }, () => sw.Write(", "));
                sw.WriteLine(";");
            }
            o.DeclSeq.Accept(this);
            if (o.Begin != null) {
                sw.Write("BEGIN");
                o.Begin.Accept(this);
            }

            if (o.Close != null)
            {
                sw.Write("CLOSE");
                o.Close.Accept(this);
            }
            ExitScope();
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
                item.Accept(this);
                
            }
            
        }
        public void Visit(IConstTypeVarListDecl.ConstDeclList o)
        {
            WriteTabs();
            sw.Write("CONST ");
            foreach (var item in o.Value)
            {
                item.Accept(this);
            }
            sw.WriteLine(";");

        }

        public void Visit(IConstTypeVarListDecl.TypeDeclList o)
        {
            WriteTabs();
            sw.Write("TYPE ");
            foreach (var item in o.Value)
            {
                item.Accept(this);
            }
            sw.WriteLine(";");
        }

        public void Visit(IConstTypeVarListDecl.VarDeclList o)
        {
            WriteTabs();
            sw.WriteLine("VAR");
            EnterScope();
            this.VisitList(o, () => WriteTabs(), () => { sw.WriteLine(";"); }, true);
            ExitScope();
        }
        public void Visit(ConstDecl o)
        {
            WriteTabs();
            o.IdentDef.Accept(this);
            sw.Write(" = ");
            o.ConstExpr.Accept(this);
        }

        public void Visit(TypeDecl o)
        {
            WriteTabs();
            o.IdentDef.Accept(this);
            sw.Write(" = ");
            o.Type_.Accept(this);
        }

        public void Visit(VarDecl o)
        {
            WriteTabs();
            o.IdentList.Accept(this);
            sw.Write(" : ");
            o.Type_.Accept(this);
        }

        public void Visit(ProcDecl o)
        {
   
        }

        public void Visit(MethAttributes o)
        {
            throw new NotImplementedException();
        }

        public void Visit(ForwardDecl o)
        {
            throw new NotImplementedException();
        }

        public void Visit(FormalPars o)
        {
            throw new NotImplementedException();
        }

        public void Visit(FPSection o)
        {
            throw new NotImplementedException();
        }

        public void Visit(Receiver o)
        {
            throw new NotImplementedException();
        }

        public void Visit(ExprList o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IdentList o)
        {
            VisitList(o.IdentDefs, () => { }, () => sw.Write(", "));
        }

        private void VisitList(AstList lst, Action before, Action after, bool doAfterForLast = false) {
            for (int i = 0; i < lst.Value.Count; i++)
            {
                before();
                lst.Value[i].Accept(this);
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
            throw new NotImplementedException();
        }

        public void Visit(Set o)
        {
            throw new NotImplementedException();
        }

        public void Visit(Element o)
        {
            throw new NotImplementedException();
        }

        public void Visit(AddOp o)
        {
            throw new NotImplementedException();
        }

        public void Visit(MulOp o)
        {
            throw new NotImplementedException();
        }

        public void Visit(Relation o)
        {
            throw new NotImplementedException();
        }

        public void Visit(SimpleExpr o)
        {
            throw new NotImplementedException();
        }

        public void Visit(Term o)
        {
            throw new NotImplementedException();
        }

        public void Visit(Expr o)
        {
            throw new NotImplementedException();
        }

        public void Visit(Case o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IStatement.AssignmentStatement o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IStatement.ProcCallStatement o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IStatement.IfStatement o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IStatement.CaseStatement o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IStatement.WhileStatement o)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void Visit(IFactor.CharacterFactor o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IFactor.DesignatorFactor o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IFactor.ExprFactor o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IFactor.NegFactor o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IFactor.NilFactor o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IFactor.NumberFactor o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IFactor.SetFactor o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IFactor.StringFactor o)
        {
            throw new NotImplementedException();
        }

        public void Visit(Designator o)
        {
            throw new NotImplementedException();
        }

        public void Visit(Designator.IDesignatorSpec.RecordDesignatorSpec o)
        {
            throw new NotImplementedException();
        }

        public void Visit(Designator.IDesignatorSpec.ArrayDesignatorSpec o)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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


    }
}
