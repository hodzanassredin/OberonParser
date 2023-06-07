
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
            if (o == null) return null;
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
            var res = new CPParser.Ast.IdentDef { 
                Ident = Map(o.Ident)
                
            };
            if (o.Export.HasValue) {
                res.Export = Map(o.Export.Value);
            }
            return res;
        }

        private CPParser.Ast.IdentDef.IdentExport Map(AOParser.Ast.IdentDef.IdentExport export)
        {
            switch (export)
            {
                case AOParser.Ast.IdentDef.IdentExport.ExportReadonly:
                    return CPParser.Ast.IdentDef.IdentExport.ExportReadonly;
                case AOParser.Ast.IdentDef.IdentExport.Export:
                    return CPParser.Ast.IdentDef.IdentExport.Export;
                default:
                    throw new ArgumentException(export.ToString());
            }
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
                if (r.Item2 != null)
                {
                    foreach (var pd in r.Item2)
                    {
                        res.ProcForwardDecls.Add(pd);
                    }
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
                if (m.Item2 != null)
                {
                    pds.AddRange(m.Item2);
                }
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
                if (m.Item2 != null)
                {
                    pds.AddRange(m.Item2);
                }
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
            return new CPParser.Ast.StatementSeq { 
                Statements = MapLst<AOParser.Ast.IStatement, CPParser.Ast.IStatement>(o.Statements,Map)
            };
        }

        private CPParser.Ast.IStatement Map(AOParser.Ast.IStatement arg)
        {
            switch (arg)
            {
                case AOParser.Ast.IStatement.AssignmentStatement s:
                    return Map(s);
                case AOParser.Ast.IStatement.AwaitStatement s:
                    throw new NotSupportedException("AwaitStatement");
                case AOParser.Ast.IStatement.CaseStatement s:
                    return Map(s);
                case AOParser.Ast.IStatement.ExitStatement s:
                    return Map(s);
                case AOParser.Ast.IStatement.ForStatement s:
                    return Map(s);
                case AOParser.Ast.IStatement.IfStatement s:
                    return Map(s);
                case AOParser.Ast.IStatement.LoopStatement s:
                    return Map(s);
                case AOParser.Ast.IStatement.ProcCallStatement s:
                    return Map(s);
                case AOParser.Ast.IStatement.RepeatStatement s:
                    return Map(s);
                case AOParser.Ast.IStatement.ReturnStatement s:
                    return Map(s);
                case AOParser.Ast.IStatement.WithStatement s:
                    return Map(s);
                case AOParser.Ast.IStatement.WhileStatement s:
                    return Map(s);
                case AOParser.Ast.IStatement.StatBlockStatement s:
                    throw new NotSupportedException("StatBlockStatement");
                default:
                    throw new NotSupportedException();
            }
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
            switch (o.Op)
            {
                case AOParser.Ast.AddOp.AddOps.Add:
                    return new CPParser.Ast.AddOp { Op = CPParser.Ast.AddOp.AddOps.Add };
                case AOParser.Ast.AddOp.AddOps.Sub:
                    return new CPParser.Ast.AddOp { Op = CPParser.Ast.AddOp.AddOps.Sub };
                case AOParser.Ast.AddOp.AddOps.Or:
                    return new CPParser.Ast.AddOp { Op = CPParser.Ast.AddOp.AddOps.Or };
                default:
                    throw new Exception();
            }
        }

        public CPParser.Ast.MulOp Map(AOParser.Ast.MulOp o)
        {
            switch (o.Op)
            {
                case AOParser.Ast.MulOp.MulOps.Mul:
                    return new CPParser.Ast.MulOp { Op = CPParser.Ast.MulOp.MulOps.Mul };
                case AOParser.Ast.MulOp.MulOps.Division:
                    return new CPParser.Ast.MulOp { Op = CPParser.Ast.MulOp.MulOps.Division };
                case AOParser.Ast.MulOp.MulOps.DIV:
                    return new CPParser.Ast.MulOp { Op = CPParser.Ast.MulOp.MulOps.DIV };
                case AOParser.Ast.MulOp.MulOps.MOD:
                    return new CPParser.Ast.MulOp { Op = CPParser.Ast.MulOp.MulOps.MOD };
                case AOParser.Ast.MulOp.MulOps.AND:
                    return new CPParser.Ast.MulOp { Op = CPParser.Ast.MulOp.MulOps.AND };
                default:
                    throw new NotImplementedException();
            }
        }

        public CPParser.Ast.Relation Map(AOParser.Ast.Relation o)
        {
            if (o == null) return null;
            switch (o.Op)
            {
                case AOParser.Ast.Relation.Relations.Eq:
                    return new CPParser.Ast.Relation { Op = CPParser.Ast.Relation.Relations.Eq };
                case AOParser.Ast.Relation.Relations.Neq:
                    return new CPParser.Ast.Relation { Op = CPParser.Ast.Relation.Relations.Neq };
                case AOParser.Ast.Relation.Relations.Lss:
                    return new CPParser.Ast.Relation { Op = CPParser.Ast.Relation.Relations.Lss };
                case AOParser.Ast.Relation.Relations.Leq:
                    return new CPParser.Ast.Relation { Op = CPParser.Ast.Relation.Relations.Leq };
                case AOParser.Ast.Relation.Relations.Gtr:
                    return new CPParser.Ast.Relation { Op = CPParser.Ast.Relation.Relations.Gtr };
                case AOParser.Ast.Relation.Relations.Geq:
                    return new CPParser.Ast.Relation { Op = CPParser.Ast.Relation.Relations.Geq };
                case AOParser.Ast.Relation.Relations.In:
                    return new CPParser.Ast.Relation { Op = CPParser.Ast.Relation.Relations.In };
                case AOParser.Ast.Relation.Relations.Is:
                    return new CPParser.Ast.Relation { Op = CPParser.Ast.Relation.Relations.Is };
                default:
                    throw new NotImplementedException();
            }
        }

        public CPParser.Ast.SimpleExpr Map(AOParser.Ast.SimpleExpr o)
        {
            if (o == null) return null;

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
            switch (factor)
            {
                case AOParser.Ast.IFactor.ExprFactor f:
                    return Map(f);
                case AOParser.Ast.IFactor.NilFactor f:
                    return Map(f);
                case AOParser.Ast.IFactor.SetFactor f:
                    return Map(f);
                case AOParser.Ast.IFactor.CharacterFactor f:
                    return Map(f);
                case AOParser.Ast.IFactor.DesignatorFactor f:
                    return Map(f);
                case AOParser.Ast.IFactor.NegFactor f:
                    return Map(f);
                case AOParser.Ast.IFactor.NumberFactor f:
                    return Map(f);
                case AOParser.Ast.IFactor.StringFactor f:
                    return Map(f);
                default:
                    throw new NotImplementedException();
            }
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
            return new CPParser.Ast.IStatement.AssignmentStatement { 
                Expr = Map(o.Expr),
                Designator = Map(o.Designator)
            };
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
            return new CPParser.Ast.IType.ArrayType { 
                ConstExprs = MapLst<AOParser.Ast.ConstExpr,CPParser.Ast.ConstExpr>(o.ConstExprs, Map)
            };
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
            var lst = o.FieldList.FieldDecl
                        .Cast<AOParser.Ast.FieldDecl>()
                        .Select(x => new CPParser.Ast.FieldList()
                        {
                            IdentList = Map(x.IdentList),
                            Type_ = Map(x.Type_).Item1
                        }).Cast<CPParser.Ast.AstElement>().ToList();

            return new CPParser.Ast.IType.RecordType(null) {
                Qualident = Map(o.Qualident),
                FieldList = new CPParser.Ast.AstList(lst)
            };
        }

        public CPParser.Ast.IType.SynonimType Map(AOParser.Ast.IType.SynonimType o)
        {
            return new CPParser.Ast.IType.SynonimType() { 
                Qualident = Map(o.Qualident)
            };
        }

        public CPParser.Ast.Number Map(AOParser.Ast.Number o)
        {
            return new CPParser.Ast.Number { 
                Value = o.Value
            };
        }

        public CPParser.Ast.IFactor.CharacterFactor Map(AOParser.Ast.IFactor.CharacterFactor o)
        {
            return new CPParser.Ast.IFactor.CharacterFactor { 
                Value = o.Value
            };
        }

        public CPParser.Ast.Comment Map(AOParser.Ast.DefinitionProc o)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IFactor.DesignatorFactor Map(AOParser.Ast.IFactor.DesignatorFactor o)
        {
            return new CPParser.Ast.IFactor.DesignatorFactor {
                Value = Map(o.Value)
            };
        }

        public CPParser.Ast.Comment Map(AOParser.Ast.Definition definition)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IFactor.ExprFactor Map(AOParser.Ast.IFactor.ExprFactor o)
        {
            return new CPParser.Ast.IFactor.ExprFactor
            {
                Value = Map(o.Value)
            };
        }

        public CPParser.Ast.IFactor.NegFactor Map(AOParser.Ast.IFactor.NegFactor o)
        {
            return new CPParser.Ast.IFactor.NegFactor
            {
                Value = Map(o.Value)
            };
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
            return new CPParser.Ast.IFactor.NilFactor
            {
            };
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
            return new CPParser.Ast.IFactor.NumberFactor
            {
                Value = Map(o.Value)
            };
        }

        public CPParser.Ast.IFactor.SetFactor Map(AOParser.Ast.IFactor.SetFactor o)
        {
            return new CPParser.Ast.IFactor.SetFactor
            {
                Value = Map(o.Value)
            };
        }

        public CPParser.Ast.IFactor.StringFactor Map(AOParser.Ast.IFactor.StringFactor o)
        {
            return new CPParser.Ast.IFactor.StringFactor
            {
                Value = o.Value
            };
        }
        public CPParser.Ast.Designator Map(AOParser.Ast.Designator o)
        {
            return new CPParser.Ast.Designator(null) { 
                Qualident = Map(o.Qualident),
                Specs = MapLst< AOParser.Ast.Designator.IDesignatorSpec, CPParser.Ast.Designator.IDesignatorSpec> (o.Specs, Map)
            };
        }

        private CPParser.Ast.Designator.IDesignatorSpec Map(AOParser.Ast.Designator.IDesignatorSpec arg)
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
