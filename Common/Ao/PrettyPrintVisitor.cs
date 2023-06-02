using AOParser.Ast;

namespace AOParser
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
            o.Ident1.AcceptWithComments(this);
            if (o.Ident2 != null) {
                sw.Write('.');
                o.Ident2.AcceptWithComments(this);
            }
        }

        public void Visit(Guard o)
        {
            o.VarQualident.AcceptWithComments(this);
            sw.Write(" : ");
            o.TypeQualident.AcceptWithComments(this);
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
            if (o.Definition != null)
            {
                o.Definition.AcceptWithComments(this);
            }
            o.DeclSeq.AcceptWithComments(this);
            ExitScope();
            o.Body.AcceptWithComments(this);
            sw.WriteLine($"{o.Ident.Name}.");
        }

        public void Visit(IdentDef o)
        {
            o.Ident.AcceptWithComments(this);
            if (o.Export != null)
            {
                switch (o.Export)
                {
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
        }

        public void Visit(IConstTypeVarListDecl o)
        {
            o.AcceptWithComments(this);
        }

        public void Visit(IProcForwardDecl o)
        {
            o.AcceptWithComments(this);
            sw.WriteLine(";");
        }

        public void Visit(DeclSeq o)
        {

            foreach (var item in o.ConstTypeVarDecls)
            {
                item.AcceptWithComments(this);
                sw.WriteLine();
            }
            foreach (var item in o.ProcDecl)
            {
                WriteTabs();
                item.AcceptWithComments(this);
                sw.WriteLine(";");
                sw.WriteLine();
            }
        }
        public void Visit(IConstTypeVarListDecl.ConstDeclList o)
        {
            WriteTabs();sw.WriteLine("CONST");
            EnterScope();
            this.VisitList(o.Values, () => { }, () => { sw.WriteLine(";"); }, true);
            ExitScope();
        }

        public void Visit(IConstTypeVarListDecl.TypeDeclList o)
        {
            WriteTabs();sw.WriteLine("TYPE");
            EnterScope();
            this.VisitList(o.Values, () => { }, () => { sw.WriteLine(";"); }, true);
            ExitScope();
        }

        public void Visit(IConstTypeVarListDecl.VarDeclList o)
        {
            WriteTabs();sw.WriteLine("VAR");
            EnterScope();
            this.VisitList(o.Values, () => { }, () => { sw.WriteLine(";"); }, true);
            ExitScope();
        }
        public void Visit(ConstDecl o)
        {
            WriteTabs();o.IdentDef.AcceptWithComments(this);sw.Write(" = ");o.ConstExpr.AcceptWithComments(this);
        }

        public void Visit(TypeDecl o)
        {
            WriteTabs();o.IdentDef.AcceptWithComments(this);sw.Write(" = ");o.Type_.AcceptWithComments(this);
        }

        public void Visit(VarDecl o)
        {
            WriteTabs();o.IdentList.AcceptWithComments(this);sw.Write(": ");o.Type_.AcceptWithComments(this);
        }

        public void Visit(ProcDecl o)
        {
            sw.Write("PROCEDURE ");
            o.ProcHead.AcceptWithComments(this);
            sw.WriteLine(";");
            EnterScope();
            o.DeclSeq.AcceptWithComments(this);
            ExitScope();
            o.Body.AcceptWithComments(this);
            o.Ident.AcceptWithComments(this);
        }

        public void Visit(FormalPars o)
        {
            sw.Write("(");
            VisitList(o.FPSections, () => { }, ()=> sw.Write("; "), false);
            
            sw.Write(")");
            if (o.Qualident != null) {
                sw.Write(": ");
                o.Qualident.AcceptWithComments(this);
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
                    default:
                        break;
                }
                sw.Write(" ");
            }
            
            VisitList(o.Idents, () => { }, () => sw.Write(", "));
            sw.Write(" : ");
            o.Type_.AcceptWithComments(this);
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
                stats[i]?.AcceptWithComments(this);
                if (i != (stats.Count - 1) || doAfterForLast) after();
            }
        }

        public void Visit(FieldList o)
        {
            VisitList(o.FieldDecl, () => { }, () => sw.Write(";"));
        }

        public void Visit(ConstExpr o)
        {
            o.Expr.AcceptWithComments(this);
        }

        public void Visit(CaseLabels o)
        {
            o.ConstExpr1.AcceptWithComments(this);
            if (o.ConstExpr2 != null) {
                sw.Write("..");
                o.ConstExpr2.AcceptWithComments(this);
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
            o.Expr1.AcceptWithComments(this);
            if (o.Expr2 != null) {
                sw.Write("..");
                o.Expr2.AcceptWithComments(this);
            }
        }

        public void Visit(AddOp o)
        {
            switch (o.Op)
            {
                case AddOp.AddOps.Add:
                    sw.Write("+");
                    break;
                case AddOp.AddOps.Sub:
                    sw.Write("-");
                    break;
                case AddOp.AddOps.Or:
                    sw.Write("OR");
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
            o.Term.AcceptWithComments(this);
            VisitList(o.SimpleExprElements, () => { }, () => { });
        }

        public void Visit(Term o)
        {
            if (o.Prefix != null)
            {
                switch (o.Prefix.Value)
                {
                    case Term.TermExprPrefix.Add:
                        sw.Write("+");
                        break;
                    case Term.TermExprPrefix.Sub:
                        sw.Write("-");
                        break;
                    default:
                        break;
                }
            }
            o.Factor.AcceptWithComments(this);
            VisitList(o.TermElements, () => { }, () => { });
        }

        public void Visit(Expr o)
        {
            o.SimpleExpr.AcceptWithComments(this);
            if (o.Relation != null) { 
                sw.Write(' ');
                o.Relation.AcceptWithComments(this);
                sw.Write(' ');
                o.SimpleExpr2.AcceptWithComments(this);
            }
        }

        public void Visit(Case o)
        {
            if (o.CaseLabels.Any()) {
                VisitList(o.CaseLabels, () => { }, () => sw.Write(","));
                sw.Write(" : ");
                o.StatementSeq.AcceptWithComments(this);
            }
        }

        public void Visit(IStatement.AssignmentStatement o)
        {
            WriteTabs();o.Designator.AcceptWithComments(this);
            sw.Write(" := ");
            o.Expr.AcceptWithComments(this);
        }

        public void Visit(IStatement.ProcCallStatement o)
        {
            WriteTabs();o.Designator.AcceptWithComments(this);
            if (o.ExprList != null)
            {
                sw.Write("(");
                o.ExprList.AcceptWithComments(this);
                sw.Write(")");
            }
        }

        public void Visit(IStatement.IfStatement o)
        {
            WriteTabs();sw.Write("IF ");o.If.AcceptWithComments(this);
            VisitList(o.ELSIFs, () => { WriteTabs(); sw.Write("ELSIF"); }, () => { });
            if (o.ElseBody != null) {
                WriteTabs(); sw.WriteLine("ELSE");
                EnterScope();
                o.ElseBody.AcceptWithComments(this);
                ExitScope();
            }
            WriteTabs(); sw.Write("END");
        }

        public void Visit(IStatement.CaseStatement o)
        {
            WriteTabs(); sw.Write("CASE "); o.Expr.AcceptWithComments(this); sw.WriteLine(" OF");
            for (int i = 0; i < o.Cases.Value.Count; i++)
            {
                WriteTabs();
                if (i != 0)
                {
                    sw.Write("|"); 
                }
                sw.Write("\t"); o.Cases.Value[i].AcceptWithComments(this);
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
            WriteTabs();sw.Write("WHILE ");o.Expr.AcceptWithComments(this);sw.WriteLine(" DO");
            EnterScope();
            o.StatementSeq.AcceptWithComments(this);
            ExitScope();
            WriteTabs(); sw.Write("END");
        }

        public void Visit(IStatement.LoopStatement o)
        {
            WriteTabs(); sw.WriteLine("LOOP");
            EnterScope();
            o.StatementSeq.AcceptWithComments(this);
            ExitScope();
            WriteTabs(); sw.Write("END");
        }

        public void Visit(IStatement.RepeatStatement o)
        {
            WriteTabs(); sw.WriteLine("REPEAT");
            EnterScope();
            o.StatementSeq.AcceptWithComments(this);
            ExitScope();
            WriteTabs(); sw.Write("UNTIL "); o.Expr.AcceptWithComments(this);
        }

        public void Visit(IStatement.WithStatement o)
        {
            WriteTabs(); sw.Write("WITH "); 
            o.Qualident1.AcceptWithComments(this); sw.Write(" : "); o.Qualident2.AcceptWithComments(this);
            sw.WriteLine(" DO");
            EnterScope();
            o.StatementSeq.AcceptWithComments(this);
            ExitScope();
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
                o.Expr.AcceptWithComments(this);
            }
            
        }

        public void Visit(IStatement.ForStatement o)
        {
            WriteTabs(); sw.Write("FOR "); o.Ident.AcceptWithComments(this); 
                sw.Write(" := "); o.Expr.AcceptWithComments(this); sw.Write(" TO "); o.ToExpr.AcceptWithComments(this); 
            if (o.ByExpr != null) {
                sw.Write(" BY "); o.ByExpr.AcceptWithComments(this);
            }
            sw.WriteLine(" DO");
            EnterScope();
            o.StatementSeq.AcceptWithComments(this);
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
            o.Type_.AcceptWithComments(this);
        }

        public void Visit(IType.PointerType o)
        {
            sw.Write("POINTER TO ");
            o.Type_.AcceptWithComments(this);
        }

        public void Visit(IType.ProcedureType o)
        {
            sw.Write("PROCEDURE");
            o.FormalPars?.AcceptWithComments(this);
        }

        public void Visit(IType.RecordType o)
        {
            sw.Write("RECORD");
            o.SysFlag?.AcceptWithComments(this);
            if (o.Qualident != null) {
                sw.Write("(");
                o.Qualident.AcceptWithComments(this);
                sw.Write(")");
            }
            if (o.FieldList.FieldDecl.Any())
            {
                sw.WriteLine();
                EnterScope();
                o.FieldList.AcceptWithComments(this);
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
            o.Qualident.AcceptWithComments(this);
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
            o.Value.AcceptWithComments(this);
        }

        public void Visit(IFactor.ExprFactor o)
        {
            sw.Write("("); o.Value.AcceptWithComments(this); sw.Write(")");
        }

        public void Visit(IFactor.NegFactor o)
        {
            sw.Write("~");
            o.Value.AcceptWithComments(this);
        }

        public void Visit(IFactor.NilFactor o)
        {
            sw.Write("NIL");
        }

        public void Visit(IFactor.NumberFactor o)
        {
            o.Value.AcceptWithComments(this);
        }

        public void Visit(IFactor.SetFactor o)
        {
            o.Value.AcceptWithComments(this);
        }

        public async void Visit(IFactor.StringFactor o)
        {
            sw.Write(o.Value);
        }

        public void Visit(Designator o)
        {
            o.Qualident.AcceptWithComments(this);
            VisitList(o.Specs, () => { }, () => { });
            if (o.EndOfLine) {
                sw.Write("$");
            }
        }

        public void Visit(Designator.IDesignatorSpec.RecordDesignatorSpec o)
        {
            sw.Write(".");
            o.Value.AcceptWithComments(this);
        }

        public void Visit(Designator.IDesignatorSpec.ArrayDesignatorSpec o)
        {
            sw.Write("[");
            o.Value.AcceptWithComments(this);
            sw.Write("]");
        }

        public void Visit(Designator.IDesignatorSpec.CastDesignatorSpec o)
        {
            throw new NotImplementedException();
        }

        public void Visit(Designator.IDesignatorSpec.ProcCallDesignatorSpec o)
        {
            sw.Write("(");
            o.Value?.AcceptWithComments(this);
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
            o.Cond.AcceptWithComments(this);
            sw.WriteLine(" THEN");
            EnterScope();
            o.ThenBody.AcceptWithComments(this);
            ExitScope();
        }

        public void Visit(SimpleElementExpr o)
        {
            sw.Write(" "); o.MulOp.AcceptWithComments(this);
            sw.Write(" "); o.Term.AcceptWithComments(this);
        }

        public void Visit(TermElementExpr o)
        {
            sw.Write(" ");o.AddOp.AcceptWithComments(this);sw.Write(" ");o.Factor.AcceptWithComments(this);
        }

        public void Visit(DefinitionProc o)
        {
            sw.Write("PROCEDURE ");
            o.Ident.AcceptWithComments(this);
            if (o.FormalPars != null) {
                sw.Write(" ");
                o.FormalPars.AcceptWithComments(this);
            }
            sw.Write(";");
        }

        public void Visit(Definition o)
        {
            WriteTabs();sw.Write("DEFINITION ");
            o.Ident.AcceptWithComments(this);
            if (o.Qualident != null) {
                sw.Write("REFINES ");
                o.Qualident.AcceptWithComments(this);
            }
            if (o.Procs.Any())
            {
                sw.WriteLine();
                VisitList(o.Procs, () => { }, () => { });
                WriteTabs();
            }
            
            sw.Write("END "); o.Ident.AcceptWithComments(this);
        }

        public void Visit(SysFlag o)
        {
            sw.Write("[");
            o.Ident.AcceptWithComments(this);
            sw.Write("]");
        }

        public void Visit(Body o)
        {
            if (o.StatBlock != null)
            {
                o.StatBlock.AcceptWithComments(this);
            }
            else {
                sw.Write("END ");
            }
        }

        public void Visit(FieldDecl o)
        {
            if (o.IdentList!=null) { 
                o.IdentList.AcceptWithComments(this);
                sw.Write(":");
                o.Type_.AcceptWithComments(this);
            }
        }

        public void Visit(StatBlock o)
        {
            WriteTabs(); sw.Write("BEGIN");
            if (o.IdentLists.Any()) {
                sw.Write(" {");
                VisitList(o.IdentLists, () => { }, () => { });
                sw.Write("}");
            }
            sw.WriteLine();
            if (o.StatementSeq != null)
            {
                EnterScope();
                o.StatementSeq.AcceptWithComments(this);
                ExitScope();
            }
            WriteTabs(); sw.Write("END ");
        }

        public void Visit(ProcHead o)
        {
            if (o.SysFlag != null)
            {
                o.SysFlag.AcceptWithComments(this);
            }
            if (o.Tag.HasValue)
            {
                switch (o.Tag.Value)
                {
                    case ProcHead.Tags.Export:
                        sw.Write("*");
                        break;
                    case ProcHead.Tags.Initializer:
                        sw.Write("&");
                        break;
                    default:
                        break;
                }
            }
            o.IdentDef.AcceptWithComments(this);
            if (o.FormalPars != null)
            {
                o.FormalPars.AcceptWithComments(this);
            }
        }

        public void Visit(IType.ObjectType o)
        {
            sw.WriteLine("OBJECT");
            if (o.SysFlag != null) { 
                o.SysFlag.AcceptWithComments(this); 
            }
            if (o.Qualident != null)
            {
                sw.Write("(");
                o.Qualident.AcceptWithComments(this);
                sw.Write(")");
            }
            if (o.ImplementsQualident != null)
            {
                sw.Write("IMPLEMENTS ");
                o.ImplementsQualident.AcceptWithComments(this);
            }
            EnterScope();
            o.DeclSeq.AcceptWithComments(this);
            ExitScope();
            WriteTabs();
            o.Body.AcceptWithComments(this);
            o.Ident.AcceptWithComments(this);
            
        }

        public void Visit(IStatement.AwaitStatement o)
        {
            sw.Write("AWAIT (");
            o.Expr.AcceptWithComments(this);
            sw.Write(")");
        }

        public void Visit(IStatement.StatBlockStatement o)
        {
            o.StatBlock.AcceptWithComments(this);
        }
    }
}
