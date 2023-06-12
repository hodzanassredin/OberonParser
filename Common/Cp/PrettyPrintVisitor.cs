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
            sw.Write($"MODULE ");
            o.Ident.AcceptWithComments(this);
            sw.WriteLine($";");
            sw.WriteLine();
            EnterScope();
            if (o.ImportList != null) {
                WriteTabs(); sw.Write("IMPORT ");
                VisitList(o.ImportList, () => { }, () => sw.Write(", "));
                sw.WriteLine(";");
                sw.WriteLine();
            }
            
            o.DeclSeq.AcceptWithComments(this);
            ExitScope();
            if (o.Begin != null) {
                sw.WriteLine("BEGIN");
                EnterScope();
                o.Begin.AcceptWithComments(this);
                ExitScope();
            }

            if (o.Close != null)
            {
                sw.WriteLine("CLOSE");
                EnterScope();
                o.Close.AcceptWithComments(this);
                ExitScope();
            }
            
            sw.WriteLine($"END {o.Ident.Name}.");
        }

        public void Visit(IdentDef o)
        {
            o.Ident.AcceptWithComments(this);

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
            foreach (var item in o.ProcForwardDecls)
            {
                WriteTabs();sw.Write("PROCEDURE ");
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
            if (o.Receiver != null)
            {
                o.Receiver?.AcceptWithComments(this);
                sw.Write(" ");
            }
            
            o.IdentDef.AcceptWithComments(this);
            sw.Write(" ");
            o.FormalPars?.AcceptWithComments(this);
            o.MethAttributes.AcceptWithComments(this);
            if (o.DeclSeq == null && o.StatementSeq == null) {
                return;
            }
            sw.WriteLine(";");
            if (o.DeclSeq != null)
            {
                EnterScope();
                o.DeclSeq.AcceptWithComments(this);
                ExitScope();
            }
            if (o.StatementSeq != null) {
                WriteTabs(); sw.WriteLine("BEGIN");
                EnterScope();
                o.StatementSeq.AcceptWithComments(this);
                ExitScope();
            }
            WriteTabs();sw.Write("END ");o.IdentDef.Ident.AcceptWithComments(this);
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
                o.Receiver?.AcceptWithComments(this);
                sw.Write(" ");
            }

            o.IdentDef.AcceptWithComments(this);
            sw.Write(" ");
            o.FormalPars?.AcceptWithComments(this);
            o.MethAttributes.AcceptWithComments(this);
        }

        public void Visit(FormalPars o)
        {
            sw.Write("(");
            VisitList(o.FPSections, () => { }, ()=> sw.Write("; "), false);
            
            sw.Write(")");
            if (o.Type_ != null) {
                sw.Write(": ");
                o.Type_.AcceptWithComments(this);
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
            o.Type_.AcceptWithComments(this);
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
            o.SelfIdent.AcceptWithComments(this);
            sw.Write(": ");
            o.TypeIdent.AcceptWithComments(this);
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
                stats[i]?.AcceptWithComments(this);
                if (i != (stats.Count - 1) || doAfterForLast) after();
            }
        }

        public void Visit(FieldList o)
        {
            if (o.IdentList != null)
            {
                WriteTabs();
                o.IdentList.AcceptWithComments(this);
                sw.Write(" : ");
                o.Type_.AcceptWithComments(this);
            }
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
            o.Term.AcceptWithComments(this);
            VisitList(o.SimpleExprElements, () => { }, () => { });
        }

        public void Visit(Term o)
        {
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
            VisitList(o.ELSIFs, () => { WriteTabs(); sw.Write("ELSIF "); }, () => { });
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
            for (int i = 0; i < o.Alternatives.Value.Count; i++)
            {
                if (i != 0) {
                    WriteTabs(); sw.Write("|");
                }
                EnterScope();
                o.Alternatives.Value[i].AcceptWithComments(this);
                ExitScope();
            }
            if (o.ElseStatementSeq != null)
            {
                WriteTabs(); sw.WriteLine("ELSE");
                EnterScope();
                o.ElseStatementSeq.AcceptWithComments(this);
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
                o.Qualident.AcceptWithComments(this);
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
            sw.Write("(");
            o.Value.AcceptWithComments(this);
            sw.Write(")");
        }

        public void Visit(Designator.IDesignatorSpec.PointerDesignatorSpec o)
        {
            sw.Write("^");
        }

        public void Visit(Designator.IDesignatorSpec.ProcCallDesignatorSpec o)
        {
            sw.Write("(");
            o.Value?.AcceptWithComments(this);
            sw.Write(")");
        }

        public void Visit(Import o)
        {
            o.Name.AcceptWithComments(this);
            if (o.OriginalName != null)
            {
                sw.Write(" := ");
                o.OriginalName.AcceptWithComments(this);
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
            sw.Write(" "); o.AddOp.AcceptWithComments(this);
            sw.Write(" "); o.Term.AcceptWithComments(this);
        }

        public void Visit(TermElementExpr o)
        {
            sw.Write(" ");o.MulOp.AcceptWithComments(this);sw.Write(" ");o.Factor.AcceptWithComments(this);
        }

        public void Visit(IStatement.WithAlternativeStatement o)
        {
            if (o.Guard != null) {
                o.Guard.AcceptWithComments(this); sw.WriteLine(" DO");
                o.StatementSeq.AcceptWithComments(this);
            }
        }

        public void Visit(Comment comment)
        {
            sw.WriteLine($"(*{comment.Content}*)");
        }
    }
}
