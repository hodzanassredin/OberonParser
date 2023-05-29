using CPParser.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            throw new NotImplementedException();
        }

        public void Visit(Qualident o)
        {
            throw new NotImplementedException();
        }

        public void Visit(Guard o)
        {
            throw new NotImplementedException();
        }

        public void Visit(Module o)
        {
            sw.WriteLine($"MODULE {o.Ident.Name};");
            EnterScope();
            o.ImportList?.Accept(this);
            ExitScope();
            sw.WriteLine($"END {o.Ident.Name}.");
        }

        public void Visit(ImportList o)
        {
            WriteTabs();sw.Write("IMPORT ");

            sw.WriteLine(";");
        }

        public void Visit(IdentDef o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IConstTypeVarDecl o)
        {
            throw new NotImplementedException();
        }

        public void Visit(IProcForwardDecl o)
        {
            throw new NotImplementedException();
        }

        public void Visit(DeclSeq o)
        {
            throw new NotImplementedException();
        }

        public void Visit(ConstDecl o)
        {
            throw new NotImplementedException();
        }

        public void Visit(TypeDecl o)
        {
            throw new NotImplementedException();
        }

        public void Visit(VarDecl o)
        {
            throw new NotImplementedException();
        }

        public void Visit(ProcDecl o)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}
