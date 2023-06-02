
namespace Common.Mappers
{
    public class AoToCpMapper 
    {
        public readonly CPParser.Ast.Module module = new CPParser.Ast.Module();

        public CPParser.Ast.IStatement.IfStatement.IfThen Map(AOParser.Ast.IStatement.IfStatement.IfThen o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Ident Map(AOParser.Ast.Ident o)
        {
            if (o == null) return null;
            return new CPParser.Ast.Ident { 
                Name = o.Name,
            };
        }

        public CPParser.Ast.Qualident Map(AOParser.Ast.Qualident o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Guard Map(AOParser.Ast.Guard o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Module Map(AOParser.Ast.Module o)
        {
            var res = new CPParser.Ast.Module();
            res.Ident = Map(o.Ident);
            if (o.ImportList != null)
            {
                res.ImportList = new CPParser.Ast.AstList();
                foreach (var item in o.ImportList.Cast<AOParser.Ast.Import>())
                {
                    res.ImportList.Add(Map(item));
                }
            }
            
            res.DeclSeq = Map(o.DeclSeq);
            if (o.Definition != null)
            {
                res.DeclSeq.CommentsBefore.Add(Map(o.Definition));
            }
            res.Begin = Map(o.Body.StatBlock.StatementSeq);
            return res;
            
        }

        public CPParser.Ast.Import Map(AOParser.Ast.Import o)
        {
            return new CPParser.Ast.Import { 
                Name = Map(o.Name),
                OriginalName = Map(o.OriginalName),
            };
        }

        public CPParser.Ast.IdentDef Map(AOParser.Ast.IdentDef o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IConstTypeVarListDecl Map(AOParser.Ast.IConstTypeVarListDecl o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.DeclSeq Map(AOParser.Ast.DeclSeq o)
        {
            var res = new CPParser.Ast.DeclSeq();
            foreach (var item in o.ConstTypeVarDecls.Cast<AOParser.Ast.IConstTypeVarListDecl>())
            {
                res.ConstTypeVarDecls.Add(Map(item));
            }
            foreach (var item in o.ProcDecl.Cast<AOParser.Ast.ProcDecl>())
            {
                res.ProcForwardDecls.Add(Map(item));
            }
            return res;
        }

        public CPParser.Ast.IConstTypeVarListDecl.ConstDeclList Map(AOParser.Ast.IConstTypeVarListDecl.ConstDeclList o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IConstTypeVarListDecl.TypeDeclList Map(AOParser.Ast.IConstTypeVarListDecl.TypeDeclList o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IConstTypeVarListDecl.VarDeclList Map(AOParser.Ast.IConstTypeVarListDecl.VarDeclList o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.ConstDecl Map(AOParser.Ast.ConstDecl o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.TypeDecl Map(AOParser.Ast.TypeDecl o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.VarDecl Map(AOParser.Ast.VarDecl o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.ProcDecl Map(AOParser.Ast.ProcDecl o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.FormalPars Map(AOParser.Ast.FormalPars o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.FPSection Map(AOParser.Ast.FPSection o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.ExprList Map(AOParser.Ast.ExprList o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IdentList Map(AOParser.Ast.IdentList o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.FieldList Map(AOParser.Ast.FieldList o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.ConstExpr Map(AOParser.Ast.ConstExpr o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.CaseLabels Map(AOParser.Ast.CaseLabels o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.StatementSeq Map(AOParser.Ast.StatementSeq o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Set Map(AOParser.Ast.Set o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Element Map(AOParser.Ast.Element o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.AddOp Map(AOParser.Ast.AddOp o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.MulOp Map(AOParser.Ast.MulOp o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Relation Map(AOParser.Ast.Relation o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.SimpleExpr Map(AOParser.Ast.SimpleExpr o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Term Map(AOParser.Ast.Term o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Expr Map(AOParser.Ast.Expr o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Case Map(AOParser.Ast.Case o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IStatement.AssignmentStatement Map(AOParser.Ast.IStatement.AssignmentStatement o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IStatement.ProcCallStatement Map(AOParser.Ast.IStatement.ProcCallStatement o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IStatement.IfStatement Map(AOParser.Ast.IStatement.IfStatement o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IStatement.CaseStatement Map(AOParser.Ast.IStatement.CaseStatement o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IStatement.WhileStatement Map(AOParser.Ast.IStatement.WhileStatement o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IStatement.LoopStatement Map(AOParser.Ast.IStatement.LoopStatement o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IStatement.RepeatStatement Map(AOParser.Ast.IStatement.RepeatStatement o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IStatement.WithStatement Map(AOParser.Ast.IStatement.WithStatement o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IStatement.ExitStatement Map(AOParser.Ast.IStatement.ExitStatement o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IStatement.ReturnStatement Map(AOParser.Ast.IStatement.ReturnStatement o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IStatement.ForStatement Map(AOParser.Ast.IStatement.ForStatement o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IType.ArrayType Map(AOParser.Ast.IType.ArrayType o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IType.PointerType Map(AOParser.Ast.IType.PointerType o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IType.ProcedureType Map(AOParser.Ast.IType.ProcedureType o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IType.RecordType Map(AOParser.Ast.IType.RecordType o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IType.SynonimType Map(AOParser.Ast.IType.SynonimType o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Number Map(AOParser.Ast.Number o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IFactor.CharacterFactor Map(AOParser.Ast.IFactor.CharacterFactor o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Comment Map(AOParser.Ast.DefinitionProc definitionProc)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IFactor.DesignatorFactor Map(AOParser.Ast.IFactor.DesignatorFactor o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Comment Map(AOParser.Ast.Definition definition)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IFactor.ExprFactor Map(AOParser.Ast.IFactor.ExprFactor o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IFactor.NegFactor Map(AOParser.Ast.IFactor.NegFactor o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Comment Map(AOParser.Ast.SysFlag sysFlag)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Comment Map(AOParser.Ast.Body body)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IFactor.NilFactor Map(AOParser.Ast.IFactor.NilFactor o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Comment Map(AOParser.Ast.FieldDecl fieldDecl)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Comment Map(AOParser.Ast.StatBlock statBlock)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Comment Map(AOParser.Ast.ProcHead procHead)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IFactor.NumberFactor Map(AOParser.Ast.IFactor.NumberFactor o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IFactor.SetFactor Map(AOParser.Ast.IFactor.SetFactor o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IFactor.StringFactor Map(AOParser.Ast.IFactor.StringFactor o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Designator Map(AOParser.Ast.Designator o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Designator.IDesignatorSpec.RecordDesignatorSpec Map(AOParser.Ast.Designator.IDesignatorSpec.RecordDesignatorSpec o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Designator.IDesignatorSpec.ArrayDesignatorSpec Map(AOParser.Ast.Designator.IDesignatorSpec.ArrayDesignatorSpec o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Designator.IDesignatorSpec.CastDesignatorSpec Map(AOParser.Ast.Designator.IDesignatorSpec.CastDesignatorSpec o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Designator.IDesignatorSpec.ProcCallDesignatorSpec Map(AOParser.Ast.Designator.IDesignatorSpec.ProcCallDesignatorSpec o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.SimpleElementExpr Map(AOParser.Ast.SimpleElementExpr simpleElementExpr)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.TermElementExpr Map(AOParser.Ast.TermElementExpr termElementExpr)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Comment Map(AOParser.Ast.IType.ObjectType objectType)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Comment Map(AOParser.Ast.IStatement.AwaitStatement awaitStatement)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Comment Map(AOParser.Ast.IStatement.StatBlockStatement statBlockStatement)
        {
            throw new NotImplementedException();
        }
    }
}
