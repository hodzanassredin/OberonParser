
using System.Text;

namespace Common.Mappers
{
    public class AoToCpMapper
    {
        public static CPParser.Ast.Qualident GetQualident(string name) {
            return new CPParser.Ast.Qualident(null) { Ident1 = new CPParser.Ast.Ident { Name = name } };
        }
        public static CPParser.Ast.Qualident GetQualident(string moduleName, string name)
        {
            return new CPParser.Ast.Qualident(null) { Ident1 = new CPParser.Ast.Ident { Name = moduleName },
                Ident2 = new CPParser.Ast.Ident { Name = name }
            };
        }
        Dictionary<string, CPParser.Ast.Qualident> simpleTypeMap = new Dictionary<string, CPParser.Ast.Qualident>
        {
            ["BOOLEAN"] = GetQualident("BOOLEAN"),
            ["CHAR"] = GetQualident("SHORTCHAR"),
            ["SIGNED8"] = GetQualident("BYTE"),
            ["SIGNED16"] = GetQualident("SHORTINT"),
            ["SIGNED32"] = GetQualident("INTEGER"),
            ["SIGNED64"] = GetQualident("LONGINT"),
            ["UNSIGNED8"] = GetQualident("CHAR"),
            ["UNSIGNED16"] = GetQualident("AOCompat", "UNSIGNED16"),
            ["UNSIGNED32"] = GetQualident("AOCompat", "UNSIGNED32"),
            ["UNSIGNED64"] = GetQualident("AOCompat", "UNSIGNED64"),
            ["FLOAT32"] = GetQualident("SHORTREAL"),
            ["FLOAT64"] = GetQualident("REAL"),
            ["SET8"] = GetQualident("SET"),
            ["SET16"] = GetQualident("SET"),
            ["SET32"] = GetQualident("SET"),
            ["SET64"] = GetQualident("AOCompat", "SET64"),

            ["REAL"] = GetQualident("REAL"),
            ["INTEGER"] = GetQualident("INTEGER"),
            ["ADDRESS"] = GetQualident("AOCompat", "ADDRESS"),
            ["SIZE"] = GetQualident("AOCompat", "SIZE"),
            ["SET"] = GetQualident("SET")
        };
        Dictionary<string, CPParser.Ast.Qualident> simpleFuncMap = new Dictionary<string, CPParser.Ast.Qualident>
        {
            ["SET8"] = GetQualident("BITS"),
            ["SET16"] = GetQualident("BITS"),
            ["SET32"] = GetQualident("BITS"),
        };
        public static string BinaryStringToHexString(string binary)
        {
            if (string.IsNullOrEmpty(binary))
                return binary;

            StringBuilder result = new StringBuilder(binary.Length / 8 + 1);

            // TODO: check all 1's or 0's... throw otherwise

            int mod4Len = binary.Length % 8;
            if (mod4Len != 0)
            {
                // pad to length multiple of 8
                binary = binary.PadLeft(((binary.Length / 8) + 1) * 8, '0');
            }

            for (int i = 0; i < binary.Length; i += 8)
            {
                string eightBits = binary.Substring(i, 8);
                result.AppendFormat("{0:X2}", Convert.ToByte(eightBits, 2));
            }

            return result.ToString();
        }
        public string MapNumber(string number) {
            number = number.Replace("\'", "");
            if (number.StartsWith("0b")) {
                var binary = number.Substring(2);
                var suffix = binary.Length > 32 ? 'L' : 'H';
                return BinaryStringToHexString(number.Substring(2)) + suffix;
            }
            if (number.StartsWith("0x"))
            {
                var hex = number.Substring(2);
                if (!Char.IsDigit(hex[0])) hex = "0" + hex;
                var suffix = hex.Length > 8 ? 'L' : 'H';
                return hex + suffix;
            }

            return number;
        }

        public readonly CPParser.Ast.Module module = new CPParser.Ast.Module();

        public CPParser.Ast.IStatement.IfStatement.IfThen Map(AOParser.Ast.IStatement.IfStatement.IfThen o, Common.SymTable.TypeDesc expecteType)
        {
            return new CPParser.Ast.IStatement.IfStatement.IfThen {
                Cond = Map(o.Cond),
                ThenBody = Map(o.ThenBody, expecteType)
            };
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
            if (simpleTypeMap.ContainsKey(o.ToString())) {
                return simpleTypeMap[o.ToString()];
            }
            if (o.IsSelf) {
                return new CPParser.Ast.Qualident(null)
                {
                    Ident1 = new CPParser.Ast.Ident() { Name = "SELF" },
                    Ident2 = Map(o.Ident1)
                };
            }

            if (o.Ident1.Name == "Math" && o.Ident2 != null) {

                var name = o.Ident2.Name;
                name = char.ToUpper(name[0]) + name.Substring(1);
                return new CPParser.Ast.Qualident(null)
                {
                    Ident1 = Map(o.Ident1),
                    Ident2 = new CPParser.Ast.Ident { Name = name }
                };
            }

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
            if (o.Body.StatBlock != null)
            {
                res.Begin = Map(o.Body.StatBlock, Common.SymTable.TypeDesc.None);


            }
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
                        .SelectMany(x => x.Values)
                        .Cast<AOParser.Ast.VarDecl>()
                        .Select(x => new CPParser.Ast.FieldList {
                            IdentList = Map(x.IdentList),
                            Type_ = Map(x.Type_).Item1//TODO support procs for anonymous objects
                        }).Cast<CPParser.Ast.AstElement>().ToList();


            var procs = o.DeclSeq.ProcDecl
                        .Cast<AOParser.Ast.ProcDecl>()
                        .Select(x => new CPParser.Ast.ProcDecl
                        {
                            IdentDef = Map(x.ProcHead.IdentDef),
                            DeclSeq = Map(x.DeclSeq),
                            Receiver = new CPParser.Ast.Receiver(null) {
                                SelfIdent = new CPParser.Ast.Ident { Name = "SELF" },
                                //ReceiverPrefix = CPParser.Ast.Receiver.Prefix., //dont need for pointer record
                                TypeIdent = Map(o.Ident)
                            },
                            StatementSeq = Map(x.Body.StatBlock, x.ProcHead.FormalPars?.Qualident?.FindType()??Common.SymTable.TypeDesc.None),
                            FormalPars = Map(x.ProcHead.FormalPars),
                            MethAttributes = new CPParser.Ast.MethAttributes() {
                                IsNew = true,
                            }

                            //Map(x.ProcHead.Tag.)
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
                    return (Map(t), null);
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
                StatementSeq = Map(o.Body.StatBlock, o.ProcHead.FormalPars.Qualident.FindType()),
                MethAttributes = new CPParser.Ast.MethAttributes()
            };
        }

        public CPParser.Ast.AstList MapLst<T, TU>(AOParser.Ast.AstList lst, Func<T, TU> map)
            where T : AOParser.Ast.AstElement
            where TU : CPParser.Ast.AstElement
        {
            return new CPParser.Ast.AstList(
                    lst.Cast<T>().Select(map).Cast<CPParser.Ast.AstElement>().ToList()
                );
        }

        public CPParser.Ast.FormalPars Map(AOParser.Ast.FormalPars o)
        {
            if (o == null) return null;
            return new CPParser.Ast.FormalPars {
                FPSections = MapLst<AOParser.Ast.FPSection, CPParser.Ast.FPSection>(o.FPSections, Map),
                Type_ = o.Qualident == null ? null : new CPParser.Ast.IType.SynonimType() {
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
            if (o == null) return null;
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
            if (o == null) return null;
            return new CPParser.Ast.ConstExpr {
                Expr = Map(o.Expr)
            };
        }

        public CPParser.Ast.CaseLabels Map(AOParser.Ast.CaseLabels o)
        {
            return new CPParser.Ast.CaseLabels {
                ConstExpr1 = Map(o.ConstExpr1),
                ConstExpr2 = Map(o.ConstExpr2)
            };
        }
        public CPParser.Ast.StatementSeq Map(AOParser.Ast.StatBlock o, Common.SymTable.TypeDesc expecteType)
        {
            if (o == null) return null;
            var res = Map(o.StatementSeq, expecteType);
            if (o.IdentLists.Any()) {
                if (o.IdentLists.Any())
                {
                    var comment = new CPParser.Ast.Comment() { Content = "NOT SUPPORTED CONV: { " + o.IdentLists.ToString() + " }" };
                    res.CommentsBefore.Add(comment);
                }
            }
            return res;
        }
        public CPParser.Ast.StatementSeq Map(AOParser.Ast.StatementSeq o, Common.SymTable.TypeDesc expectedType)
        {
            if (o == null) return null;
            return new CPParser.Ast.StatementSeq {
                Statements = MapLst<AOParser.Ast.IStatement, CPParser.Ast.AstElement>(o.Statements, x=>Map(x, expectedType))
            };
        }

        private CPParser.Ast.AstElement Map(AOParser.Ast.IStatement arg, Common.SymTable.TypeDesc expectedType)
        {
            switch (arg)
            {
                case AOParser.Ast.IStatement.AssignmentStatement s:
                    return Map(s);
                case AOParser.Ast.IStatement.AwaitStatement s:
                    return new CPParser.Ast.Comment() { Content = "NOT SUPPORTED CONV: " + s.ToString() };
                case AOParser.Ast.IStatement.CaseStatement s:
                    return Map(s, expectedType);
                case AOParser.Ast.IStatement.ExitStatement s:
                    return Map(s);
                case AOParser.Ast.IStatement.ForStatement s:
                    return Map(s, expectedType);
                case AOParser.Ast.IStatement.IfStatement s:
                    return Map(s, expectedType);
                case AOParser.Ast.IStatement.LoopStatement s:
                    return Map(s, expectedType);
                case AOParser.Ast.IStatement.ProcCallStatement s:
                    return Map(s);
                case AOParser.Ast.IStatement.RepeatStatement s:
                    return Map(s, expectedType);
                case AOParser.Ast.IStatement.ReturnStatement s:
                    return Map(s, expectedType);
                case AOParser.Ast.IStatement.WithStatement s:
                    return Map(s, expectedType);
                case AOParser.Ast.IStatement.WhileStatement s:
                    return Map(s, expectedType);
                case AOParser.Ast.IStatement.StatBlockStatement s:
                    return new CPParser.Ast.Comment() { Content = "NOT SUPPORTED CONV: " + s.ToString() };
                default:
                    throw new NotSupportedException();
            }
        }

        public CPParser.Ast.Set Map(AOParser.Ast.Set o)
        {
            return new CPParser.Ast.Set {
                Elements = MapLst<AOParser.Ast.Element, CPParser.Ast.Element>(o.Elements, Map)
            };
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
        private IEnumerable<CPParser.Ast.AstElement> ToSequence(AOParser.Ast.Term t)
        {
            if (t.Prefix.HasValue) {
                yield return new CPParser.Ast.IFactor.ExprFactor() {
                    Value = new CPParser.Ast.Expr() {
                        SimpleExpr = new CPParser.Ast.SimpleExpr {
                            Prefix = Map(t.Prefix.Value),
                            Term = new CPParser.Ast.Term {
                                Factor = Map(t.Factor)
                            }
                        }
                    }
                };
            } else {
                yield return Map(t.Factor);
            }
            foreach (var te in t.TermElements.Cast<AOParser.Ast.TermElementExpr>())
            {
                yield return Map(te.AddOp);
                yield return Map(te.Factor);
            }
        }
        private IEnumerable<CPParser.Ast.AstElement> ToSequence(AOParser.Ast.SimpleExpr o) {
            foreach (var item in ToSequence(o.Term))
            {
                yield return item;
            }
            foreach (var te in o.SimpleExprElements.Cast<AOParser.Ast.SimpleElementExpr>())
            {
                yield return Map(te.MulOp);
                foreach (var item in ToSequence(te.Term))
                {
                    yield return item;
                }
            }
        }

        public CPParser.Ast.SimpleExpr Map(AOParser.Ast.SimpleExpr o)
        {
            if (o == null) return null;

            var s = ToSequence(o).ToList();

            var res = new CPParser.Ast.SimpleExpr();
            var t = new CPParser.Ast.Term();
            CPParser.Ast.TermElementExpr te = null;
            res.Term = t;
            foreach (var item in s) {
                switch (item)
                {
                    case CPParser.Ast.IFactor f:
                        if (te != null)
                        {
                            te.Factor = f;
                        }
                        else
                        {
                            t.Factor = f;
                        }
                        break;

                    case CPParser.Ast.AddOp f:
                        t = new CPParser.Ast.Term();
                        te = null;
                        res.SimpleExprElements.Add(new CPParser.Ast.SimpleElementExpr {
                            AddOp = f,
                            Term = t
                        });
                        break;
                    case CPParser.Ast.MulOp f:
                        te = new CPParser.Ast.TermElementExpr()
                        {
                            MulOp = f,
                        };
                        t.TermElements.Add(te);
                        break;

                    default:
                        break;
                }
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
            switch (value)
            {
                case AOParser.Ast.Term.TermExprPrefix.Add:
                    return CPParser.Ast.SimpleExpr.SimpleExprPrefix.Add;
                case AOParser.Ast.Term.TermExprPrefix.Sub:
                    return CPParser.Ast.SimpleExpr.SimpleExprPrefix.Sub;
                default:
                    throw new Exception();
            }
        }

        public CPParser.Ast.Expr Map(AOParser.Ast.Expr o)
        {
            return new CPParser.Ast.Expr {
                SimpleExpr = Map(o.SimpleExpr),
                Relation = Map(o.Relation),
                SimpleExpr2 = Map(o.SimpleExpr2),
            };
        }

        public CPParser.Ast.Case Map(AOParser.Ast.Case o, Common.SymTable.TypeDesc expecteType)
        {
            return new CPParser.Ast.Case {
                CaseLabels = MapLst<AOParser.Ast.CaseLabels, CPParser.Ast.CaseLabels>(o.CaseLabels, Map),
                StatementSeq = Map(o.StatementSeq, expecteType)
            };
        }

        public AOParser.Ast.Expr DownOrUpCast(AOParser.Ast.Expr e, Common.SymTable.TypeDesc toType) {

            return e;
            if (!toType.IsSimple || !e.TypeDescr.IsSimple) return e;
            if (toType.form == e.TypeDescr.form) return e;
            var downcast = toType.form < e.TypeDescr.form;

            var fn = downcast ? "SHORT" : "LONG";
            
            return new AOParser.Ast.Expr { 
                SimpleExpr = new AOParser.Ast.SimpleExpr { 
                    Term = new AOParser.Ast.Term { 
                        Factor = new AOParser.Ast.IFactor.DesignatorFactor() { 
                            Value = new AOParser.Ast.Designator(null) { 
                                Qualident = new AOParser.Ast.Qualident(null) {Ident1 = new AOParser.Ast.Ident() { Name = fn} },
                                Specs = new AOParser.Ast.AstList { new AOParser.Ast.Designator.IDesignatorSpec.ProcCallDesignatorSpec() { 
                                    Value = new AOParser.Ast.ExprList(){ 
                                        Exprs = new AOParser.Ast.AstList(){ 
                                            e
                                        }
                                    }
                                } }
                            }
                        }
                    }
                }
            };
        }

        public CPParser.Ast.IStatement.AssignmentStatement Map(AOParser.Ast.IStatement.AssignmentStatement o)
        {
            var res = new CPParser.Ast.IStatement.AssignmentStatement {
                Designator = Map(o.Designator),
                Expr = Map(DownOrUpCast(o.Expr, o.Designator.TypeDescr)),
            };
            return res;
        }

        public CPParser.Ast.IStatement.ProcCallStatement Map(AOParser.Ast.IStatement.ProcCallStatement o)
        {
            return new CPParser.Ast.IStatement.ProcCallStatement { 
                Designator = Map(o.Designator),
                ExprList = Map(o.ExprList)
            };
        }

        public CPParser.Ast.IStatement.IfStatement Map(AOParser.Ast.IStatement.IfStatement o, Common.SymTable.TypeDesc expecteType)
        {
            return new CPParser.Ast.IStatement.IfStatement
            {
                If = Map(o.If, expecteType),
                ElseBody = Map(o.ElseBody, expecteType),
                ELSIFs = MapLst<AOParser.Ast.IStatement.IfStatement.IfThen, CPParser.Ast.IStatement.IfStatement.IfThen>(o.ELSIFs, x=>Map(x, expecteType))
            };
        }
        public CPParser.Ast.IStatement.CaseStatement Map(AOParser.Ast.IStatement.CaseStatement o, Common.SymTable.TypeDesc expecteType)
        {
            return new CPParser.Ast.IStatement.CaseStatement
            {
                Expr = Map(o.Expr),
                ElseBody = Map(o.ElseBody, expecteType),
                Cases = MapLst<AOParser.Ast.Case, CPParser.Ast.Case>(o.Cases, x=>Map(x, expecteType))
            };
        }

        public CPParser.Ast.IStatement.WhileStatement Map(AOParser.Ast.IStatement.WhileStatement o, Common.SymTable.TypeDesc expecteType)
        {
            return new CPParser.Ast.IStatement.WhileStatement
            {
                Expr = Map(o.Expr),
                StatementSeq = Map(o.StatementSeq, expecteType)
            };
        }

        public CPParser.Ast.IStatement.LoopStatement Map(AOParser.Ast.IStatement.LoopStatement o, Common.SymTable.TypeDesc expecteType)
        {
            return new CPParser.Ast.IStatement.LoopStatement
            {
                StatementSeq = Map(o.StatementSeq, expecteType)
            };
        }

        public CPParser.Ast.IStatement.RepeatStatement Map(AOParser.Ast.IStatement.RepeatStatement o, Common.SymTable.TypeDesc expecteType)
        {
            return new CPParser.Ast.IStatement.RepeatStatement { 
                Expr = Map(o.Expr),
                StatementSeq = Map(o.StatementSeq, expecteType)
            };
        }

        public CPParser.Ast.IStatement.WithStatement Map(AOParser.Ast.IStatement.WithStatement o, Common.SymTable.TypeDesc expecteType)
        {
            var res = new CPParser.Ast.IStatement.WithStatement {
            };
            res.Alternatives.Add(new CPParser.Ast.IStatement.WithAlternativeStatement() { 
                StatementSeq = Map(o.StatementSeq, expecteType),
                Guard = new CPParser.Ast.Guard { 
                    VarQualident = Map(o.Qualident1),
                    TypeQualident = Map(o.Qualident2)
                }
            });
            return res;
        }

        public CPParser.Ast.IStatement.ExitStatement Map(AOParser.Ast.IStatement.ExitStatement o)
        {
            return new CPParser.Ast.IStatement.ExitStatement();
        }

        public CPParser.Ast.IStatement.ReturnStatement Map(AOParser.Ast.IStatement.ReturnStatement o, Common.SymTable.TypeDesc expectedType)
        {
            return new CPParser.Ast.IStatement.ReturnStatement {
                Expr = Map(DownOrUpCast(o.Expr, expectedType))
            };
        }


        public CPParser.Ast.IStatement.ForStatement Map(AOParser.Ast.IStatement.ForStatement o, Common.SymTable.TypeDesc expectedType)
        {
            return new CPParser.Ast.IStatement.ForStatement { 
                ByExpr = Map(o.ByExpr),
                Expr = Map(o.Expr),
                ToExpr = Map(o.ToExpr),
                Ident = Map(o.Ident),
                StatementSeq = Map(o.StatementSeq, expectedType)
            };
        }

        public CPParser.Ast.IType.ArrayType Map(AOParser.Ast.IType.ArrayType o)
        {
            return new CPParser.Ast.IType.ArrayType { 
                ConstExprs = MapLst<AOParser.Ast.ConstExpr,CPParser.Ast.ConstExpr>(o.ConstExprs, Map),
                Type_ = Map(o.Type_).Item1
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
                FieldList = new CPParser.Ast.AstList(lst),
                RecordMeta = CPParser.Ast.IType.RecordType.Meta.EXTENSIBLE
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
                Value = MapNumber(o.Value)
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

        //public CPParser.Ast.Comment Map(AOParser.Ast.Body body)
        //{
        //    throw new NotImplementedException();
        //}

        public CPParser.Ast.IFactor.NilFactor Map(AOParser.Ast.IFactor.NilFactor o)
        {
            return new CPParser.Ast.IFactor.NilFactor
            {
            };
        }

        //public CPParser.Ast.Comment Map(AOParser.Ast.FieldDecl fieldDecl)
        //{
        //    throw new NotImplementedException();
        //}

        //public CPParser.Ast.Comment Map(AOParser.Ast.StatBlock statBlock)
        //{
        //    throw new NotImplementedException();
        //}

        //public CPParser.Ast.Comment Map(AOParser.Ast.ProcHead procHead)
        //{
        //    throw new NotImplementedException();
        //}

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
            var q = Map(o.Qualident);
            if (simpleFuncMap.ContainsKey(o.Qualident.ToString()) && o.Specs.Any() && o.Specs.Value[0] is AOParser.Ast.Designator.IDesignatorSpec.ProcCallDesignatorSpec)
            {
                q = simpleFuncMap[o.Qualident.ToString()];
            }
            return new CPParser.Ast.Designator(null) { 
                Qualident = q,
                Specs = MapLst< AOParser.Ast.Designator.IDesignatorSpec, CPParser.Ast.Designator.IDesignatorSpec> (o.Specs, Map)
            };
        }

        private CPParser.Ast.Designator.IDesignatorSpec Map(AOParser.Ast.Designator.IDesignatorSpec arg)
        {
            switch (arg)
            {
                case AOParser.Ast.Designator.IDesignatorSpec.RecordDesignatorSpec s:
                    return Map(s);
                case AOParser.Ast.Designator.IDesignatorSpec.ArrayDesignatorSpec s:
                    return Map(s);
                case AOParser.Ast.Designator.IDesignatorSpec.CastDesignatorSpec s:
                    return Map(s);
                case AOParser.Ast.Designator.IDesignatorSpec.ProcCallDesignatorSpec s:
                    return Map(s);
                case AOParser.Ast.Designator.IDesignatorSpec.PointerDesignatorSpec s:
                    return Map(s);
                default:
                    throw new ArgumentException();
            }
        }
        public CPParser.Ast.Designator.IDesignatorSpec.PointerDesignatorSpec Map(AOParser.Ast.Designator.IDesignatorSpec.PointerDesignatorSpec o)
        {
            return new CPParser.Ast.Designator.IDesignatorSpec.PointerDesignatorSpec
            {
                
            };
        }
        public CPParser.Ast.Designator.IDesignatorSpec.RecordDesignatorSpec Map(AOParser.Ast.Designator.IDesignatorSpec.RecordDesignatorSpec o)
        {
            return new CPParser.Ast.Designator.IDesignatorSpec.RecordDesignatorSpec { 
                Value = Map(o.Value)
            };
        }

        public CPParser.Ast.Designator.IDesignatorSpec.ArrayDesignatorSpec Map(AOParser.Ast.Designator.IDesignatorSpec.ArrayDesignatorSpec o)
        {
            return new CPParser.Ast.Designator.IDesignatorSpec.ArrayDesignatorSpec
            {
                Value = Map(o.Value)
            };
        }

        public CPParser.Ast.Designator.IDesignatorSpec.CastDesignatorSpec Map(AOParser.Ast.Designator.IDesignatorSpec.CastDesignatorSpec o)
        {
            return new CPParser.Ast.Designator.IDesignatorSpec.CastDesignatorSpec(null)
            {
                Value = Map(o.Value)
            };
        }

        public CPParser.Ast.Designator.IDesignatorSpec.ProcCallDesignatorSpec Map(AOParser.Ast.Designator.IDesignatorSpec.ProcCallDesignatorSpec o)
        {
            return new CPParser.Ast.Designator.IDesignatorSpec.ProcCallDesignatorSpec
            {
                Value = Map(o.Value)
            };
        }

    }
}
