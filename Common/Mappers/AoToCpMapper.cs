
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
            return new CPParser.Ast.Qualident(null) { 
                Ident1 = Map(o.Ident1),
                Ident2 = Map(o.Ident2)
            };
        }

        public CPParser.Ast.Guard Map(AOParser.Ast.Guard o)
        {
            return new CPParser.Ast.Guard() { 
                VarQualident = Map(o.VarQualident),
                TypeQualident = Map(o.TypeQualident)
            };
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

        public (CPParser.Ast.IConstTypeVarListDecl, List<CPParser.Ast.ProcDecl>) Map(AOParser.Ast.IConstTypeVarListDecl o)
        {
            switch (o)
            {
                case AOParser.Ast.IConstTypeVarListDecl.ConstDeclList t:
                    return (Map(t), null);
                case AOParser.Ast.IConstTypeVarListDecl.VarDeclList t:
                    return Map(t);
                case AOParser.Ast.IConstTypeVarListDecl.TypeDeclList t:
                    return Map(t);
                default:
                    throw new Exception();
            }
        }

        public CPParser.Ast.DeclSeq Map(AOParser.Ast.DeclSeq o)
        {
            var res = new CPParser.Ast.DeclSeq();
            foreach (var item in o.ConstTypeVarDecls.Cast<AOParser.Ast.IConstTypeVarListDecl>())
            {
                var r = Map(item);
                res.ConstTypeVarDecls.Add(r.Item1);
                foreach (var pd in r.Item2)
                {
                    res.ProcForwardDecls.Add(pd);
                }
            }
            foreach (var item in o.ProcDecl.Cast<AOParser.Ast.ProcDecl>())
            {
                res.ProcForwardDecls.Add(Map(item));
            }
            return res;
        }

        public CPParser.Ast.IConstTypeVarListDecl.ConstDeclList Map(AOParser.Ast.IConstTypeVarListDecl.ConstDeclList o)
        {
            var res = new CPParser.Ast.IConstTypeVarListDecl.ConstDeclList();
            foreach (var item in o.Values.Cast<AOParser.Ast.ConstDecl>())
            {
                res.Values.Add(Map(item));
            }
            return res;
        }

        public (CPParser.Ast.IConstTypeVarListDecl.TypeDeclList, List<CPParser.Ast.ProcDecl>) Map(AOParser.Ast.IConstTypeVarListDecl.TypeDeclList o)
        {
            var pds = new List<CPParser.Ast.ProcDecl>();
            var res = new CPParser.Ast.IConstTypeVarListDecl.TypeDeclList();
            foreach (var item in o.Values.Cast<AOParser.Ast.TypeDecl>())
            {
                var m = Map(item);
                res.Values.Add(m.Item1);
                pds.AddRange(m.Item2);
            }
            return (res, pds);
        }

        public (CPParser.Ast.IConstTypeVarListDecl.VarDeclList, List<CPParser.Ast.ProcDecl>) Map(AOParser.Ast.IConstTypeVarListDecl.VarDeclList o)
        {
            var pds = new List<CPParser.Ast.ProcDecl>();
            var res = new CPParser.Ast.IConstTypeVarListDecl.VarDeclList();
            foreach (var item in o.Values.Cast<AOParser.Ast.VarDecl>())
            {
                var m = Map(item);
                res.Values.Add(m.Item1);
                pds.AddRange(m.Item2);
            }
            return (res, pds);
        }

        public CPParser.Ast.ConstDecl Map(AOParser.Ast.ConstDecl o)
        {
            return new CPParser.Ast.ConstDecl {
                ConstExpr = Map(o.ConstExpr),
                IdentDef = Map(o.IdentDef)
            };
        }
        
        public (CPParser.Ast.IType.PointerType, List<CPParser.Ast.ProcDecl>) Map(AOParser.Ast.IType.ObjectType o)
        {
            var vars = o.DeclSeq.ConstTypeVarDecls
                        .Where(x => x is AOParser.Ast.IConstTypeVarListDecl.VarDeclList)
                        .Cast<AOParser.Ast.IConstTypeVarListDecl.VarDeclList>()
                        .SelectMany(x=>x.Values)
                        .Cast<AOParser.Ast.VarDecl>()
                        .Select(x=> new CPParser.Ast.FieldList { 
                            IdentList = Map(x.IdentList),
                            Type_ = Map(x.Type_).Item1//TODO support procs for anonymous objects
                        }).Cast<CPParser.Ast.AstElement>().ToList();

            var procs = o.DeclSeq.ProcDecl
                        .Cast<AOParser.Ast.ProcDecl>()
                        .Select(x => new CPParser.Ast.ProcDecl
                        {
                            IdentDef = new CPParser.Ast.IdentDef { 
                                Ident = Map(x.Ident)
                            },
                            DeclSeq = Map(x.DeclSeq),
                            Receiver = new CPParser.Ast.Receiver(null) { 
                                SelfIdent = new CPParser.Ast.Ident { Name = "self" },
                                ReceiverPrefix = CPParser.Ast.Receiver.Prefix.VAR,
                                TypeIdent = Map(o.Ident)
                            },
                            StatementSeq = Map(x.Body.StatBlock.StatementSeq),
                            FormalPars = Map(x.ProcHead.FormalPars),
                            //MethAttributes = Map(x.ProcHead.Tag.)
                        }).ToList();

            var tp = new CPParser.Ast.IType.PointerType()
            {
                Type_ = new CPParser.Ast.IType.RecordType(o.TypeDescr.scope)
                {
                    Qualident = Map(o.Qualident),
                    FieldList = new CPParser.Ast.AstList(vars)
                }
            };
            return (tp, procs);
        }
        public (CPParser.Ast.IType, List<CPParser.Ast.ProcDecl>) Map(AOParser.Ast.IType o)
        {
            switch (o)
            {
                case AOParser.Ast.IType.ProcedureType t:
                    return (Map(t),null);
                case AOParser.Ast.IType.ArrayType t:
                    return (Map(t), null);
                case AOParser.Ast.IType.ObjectType t:
                    return Map(t);
                case AOParser.Ast.IType.PointerType t:
                    return (Map(t), null);
                case AOParser.Ast.IType.RecordType t:
                    return (Map(t), null);
                case AOParser.Ast.IType.SynonimType t:
                    return (Map(t), null);
                default:
                    throw new Exception();
            }
        }
        public (CPParser.Ast.TypeDecl, List<CPParser.Ast.ProcDecl>) Map(AOParser.Ast.TypeDecl o)
        {
            var m = Map(o.Type_);
            var tp = new CPParser.Ast.TypeDecl
            {
                Type_ = m.Item1,
                IdentDef = Map(o.IdentDef)
            };
            return (tp, m.Item2);
        }

        public (CPParser.Ast.VarDecl, List<CPParser.Ast.ProcDecl>) Map(AOParser.Ast.VarDecl o)
        {
            var m = Map(o.Type_);
            var vr = new CPParser.Ast.VarDecl
            {
                Type_ = m.Item1,
                IdentList = Map(o.IdentList)
            };
            return (vr, m.Item2);
        }

        public CPParser.Ast.ProcDecl Map(AOParser.Ast.ProcDecl o)
        {
            return new CPParser.Ast.ProcDecl {
                DeclSeq = Map(o.DeclSeq),
                FormalPars = Map(o.ProcHead.FormalPars),
                StatementSeq = Map(o.Body.StatBlock.StatementSeq)
            };
        }

        public CPParser.Ast.AstList MapLst<T, TU>(AOParser.Ast.AstList lst, Func<T,TU> map)
            where T : AOParser.Ast.AstElement
            where TU : CPParser.Ast.AstElement
        {
            return new CPParser.Ast.AstList(
                    lst.Cast<T>().Select(map).Cast<CPParser.Ast.AstElement>().ToList()
                );
        }

        public CPParser.Ast.FormalPars Map(AOParser.Ast.FormalPars o)
        {
            return new CPParser.Ast.FormalPars {
                FPSections = MapLst<AOParser.Ast.FPSection, CPParser.Ast.FPSection>(o.FPSections, Map),
                Type_ = new CPParser.Ast.IType.SynonimType() {
                    Qualident = Map(o.Qualident)
                }
            };
        }

        public CPParser.Ast.FPSection Map(AOParser.Ast.FPSection o)
        {
            return new CPParser.Ast.FPSection { 
                Idents = MapLst<AOParser.Ast.Ident, CPParser.Ast.Ident>(o.Idents, Map),
                Type_ = Map(o.Type_).Item1
            };
        }

        public CPParser.Ast.ExprList Map(AOParser.Ast.ExprList o)
        {
            return new CPParser.Ast.ExprList { 
                Exprs = MapLst<AOParser.Ast.Expr, CPParser.Ast.Expr>(o.Exprs, Map),
            };
        }

        public CPParser.Ast.IdentList Map(AOParser.Ast.IdentList o)
        {
            return new CPParser.Ast.IdentList { 
                IdentDefs = MapLst<AOParser.Ast.IdentDef, CPParser.Ast.IdentDef>(o.IdentDefs, Map),
            };
        }

        public CPParser.Ast.FieldList Map(AOParser.Ast.FieldList o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.ConstExpr Map(AOParser.Ast.ConstExpr o)
        {
            return new CPParser.Ast.ConstExpr { 
                Expr = Map(o.Expr)
            };
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
            var t = Map(o.Term);
            if (!o.SimpleExprElements.Any()) {
                return t;
            }
            var lst = o.SimpleExprElements.Cast<AOParser.Ast.SimpleElementExpr>()
                                    .Select(e => new CPParser.Ast.TermElementExpr { 
                                        MulOp = Map(e.MulOp),
                                        Factor = new CPParser.Ast.IFactor.ExprFactor()
                                        {
                                            Value = new CPParser.Ast.Expr()
                                            {
                                                SimpleExpr = Map(e.Term)
                                            }
                                        },

                                    }).Cast<CPParser.Ast.AstElement>().ToList();
            var res = new CPParser.Ast.SimpleExpr { 
                Term = new CPParser.Ast.Term { 
                    Factor = new CPParser.Ast.IFactor.ExprFactor(){ 
                        Value = new CPParser.Ast.Expr() { 
                            SimpleExpr = t
                        }
                    },
                    TermElements = new CPParser.Ast.AstList(lst)
                }
            };

            return res;
        }

        public CPParser.Ast.SimpleExpr Map(AOParser.Ast.Term o)
        {
            var res = new CPParser.Ast.SimpleExpr();
            if (o.Prefix.HasValue)
            {
                res.Prefix = Map(o.Prefix.Value);
                res.Term = new CPParser.Ast.Term
                {
                    Factor = Map(o.Factor)
                };
            };
            
            foreach (var item in o.TermElements.Cast<AOParser.Ast.TermElementExpr>())
            {
                res.SimpleExprElements.Add(new CPParser.Ast.SimpleElementExpr
                {
                    AddOp = Map(item.AddOp),
                    Term = new CPParser.Ast.Term
                    {
                        Factor = Map(item.Factor)
                    }
                });
            }
            return res;
        }

        private CPParser.Ast.IFactor Map(AOParser.Ast.IFactor factor)
        {
            throw new NotImplementedException();
        }

        private CPParser.Ast.SimpleExpr.SimpleExprPrefix Map(AOParser.Ast.Term.TermExprPrefix value)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.Expr Map(AOParser.Ast.Expr o)
        {
            return new CPParser.Ast.Expr { 
                SimpleExpr = Map(o.SimpleExpr),
                Relation = Map(o.Relation),
                SimpleExpr2 = Map(o.SimpleExpr2),
            };
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
