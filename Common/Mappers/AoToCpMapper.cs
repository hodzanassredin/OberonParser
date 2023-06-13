
using CPParser.Ast;
using System.Text;

namespace Common.Mappers
{
    public class AoToCpMapper
    {
        public const string CompatModuleName = "CyptoCompat";
        public static CPParser.Ast.Qualident GetQualident(string name) {
            var res =  new CPParser.Ast.Qualident(null) { Ident1 = new CPParser.Ast.Ident { Name = name } };
            return res;
        }
        public static CPParser.Ast.Qualident GetQualidentFrom(string from, string name)
        {
            var res = GetQualident(name);
            res.Ident1.CommentsBefore.Add(new CPParser.Ast.Comment() { Content = $"CONV FROM {from}" });
            return res;
        }
        public static CPParser.Ast.Qualident GetQualident(string moduleName, string name)
        {
            return new CPParser.Ast.Qualident(null) { Ident1 = new CPParser.Ast.Ident { Name = moduleName },
                Ident2 = new CPParser.Ast.Ident { Name = name }
            };

        }
        Dictionary<string, CPParser.Ast.Ident> moduleMap = new Dictionary<string, CPParser.Ast.Ident>
        {
            ["Streams"] = new CPParser.Ast.Ident { Name = "CommStreams"},
            ["KernelLog"] = new CPParser.Ast.Ident { Name = "StdLog" },
        };

        Dictionary<string, CPParser.Ast.Qualident> simpleTypeMap = new Dictionary<string, CPParser.Ast.Qualident>
        {
            ["BOOLEAN"] = GetQualident("BOOLEAN"),
            ["CHAR"] = GetQualident("SHORTCHAR"),
            ["SIGNED8"] = GetQualident("BYTE"),
            ["SIGNED16"] = GetQualident("SHORTINT"),
            ["SIGNED32"] = GetQualident("INTEGER"),
            ["SIGNED64"] = GetQualident("LONGINT"),
            ["UNSIGNED8"] = GetQualident("SHORTCHAR"),
            ["UNSIGNED16"] = GetQualident("CHAR"),
            ["UNSIGNED32"] = GetQualident(CompatModuleName, "UNSIGNED32"),
            ["UNSIGNED64"] = GetQualident(CompatModuleName, "UNSIGNED64"),
            ["FLOAT32"] = GetQualident("SHORTREAL"),
            ["FLOAT64"] = GetQualident("REAL"),
            ["SET8"] = GetQualident("SET"),
            ["SET16"] = GetQualident("SET"),
            ["SET32"] = GetQualident("SET"),
            ["SET64"] = GetQualident(CompatModuleName, "SET64"),

            ["REAL"] = GetQualident("REAL"),
            ["INTEGER"] = GetQualident("INTEGER"),
            ["ADDRESS"] = GetQualident(CompatModuleName, "ADDRESS"),
            ["SIZE"] = GetQualident(CompatModuleName, "SIZE"),
            ["SET"] = GetQualident("SET")
        };
        Dictionary<string, CPParser.Ast.Qualident> simpleFuncMap = new Dictionary<string, CPParser.Ast.Qualident>
        {
            ["ADDRESSOF"] = GetQualident("SYSTEM", "ADR"),
            ["SIZEOF"] = GetQualident("SIZE"),
            ["LSH"] = GetQualident("SYSTEM", "LSH"),
            ["ROT"] = GetQualident("SYSTEM", "ROT"),

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
                Cond = Map(o.Cond, Common.SymTable.TypeDesc.BOOL),
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
            if (simpleTypeMap.ContainsKey(o.ToString()))
            {
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
            res.ImportList = new CPParser.Ast.AstList();
            if (o.ImportList != null)
            {
                foreach (var item in o.ImportList.Cast<AOParser.Ast.Import>())
                {
                    res.ImportList.Add(Map(item));
                }
                
            }
            res.ImportList.Add(new CPParser.Ast.Import
            {
                Name = new CPParser.Ast.Ident { Name = CompatModuleName }
            });
            res.DeclSeq = Map(o.DeclSeq);

            AddForward(res.DeclSeq.ProcForwardDecls);

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

        private void AddForward(AstList procForwardDecls)
        {
            //var deps = new Dictionary<string, List<String>>();
            //foreach (var item in procForwardDecls)
            //{
            //    if (item is CPParser.Ast.ProcDecl pdecl) {
            //        var p = pdecl.IdentDef.Ident.Name;
            //        if (pdecl.StatementSeq!=null)
            //        foreach (var stmt in pdecl.StatementSeq.Statements)
            //        {

            //        }
            //    }
            //}
        }

        private CPParser.Ast.Comment GetConvComment(string from) { 
            return new CPParser.Ast.Comment() { Content = $"CONV FROM {from}" };
        }
        private CPParser.Ast.Comment GetRemoveComment(string from)
        {
            return new CPParser.Ast.Comment() { Content = $"CONV REMOVE {from}" };
        }
        public CPParser.Ast.Import Map(AOParser.Ast.Import o)
        {
            CPParser.Ast.Import res = null;
            if (o.OriginalName == null && moduleMap.ContainsKey(o.Name.Name)) {
                res = new CPParser.Ast.Import
                {
                    Name = moduleMap[o.Name.Name],
                };
                res.Name.CommentsBefore.Add(GetConvComment(o.Name.Name));
            }
            else if (o.OriginalName != null && moduleMap.ContainsKey(o.OriginalName.Name))
            {
                res = new CPParser.Ast.Import
                {
                    Name = Map(o.Name),
                    OriginalName = moduleMap[o.OriginalName.Name],
                };
                res.OriginalName.CommentsBefore.Add(GetConvComment(o.OriginalName.Name));
            }
            else {
                res = new CPParser.Ast.Import {
                        Name = Map(o.Name),
                        OriginalName = Map(o.OriginalName)
                       };
            }
            return res;
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

        public (CPParser.Ast.IConstTypeVarListDecl, List<CPParser.Ast.ProcDecl>, List<CPParser.Ast.ConstDecl>) Map(AOParser.Ast.IConstTypeVarListDecl o)
        {
            switch (o)
            {
                case AOParser.Ast.IConstTypeVarListDecl.ConstDeclList t:
                    return (Map(t), null, null);
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
                if (r.Item3 != null)
                {
                    var cds = new CPParser.Ast.IConstTypeVarListDecl.ConstDeclList();
                    foreach (var cd in r.Item3)
                    {
                        cds.Values.Add(cd);
                        
                    }
                    res.ConstTypeVarDecls.Add(cds);
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

        public (CPParser.Ast.IConstTypeVarListDecl.TypeDeclList, List<CPParser.Ast.ProcDecl>, List<CPParser.Ast.ConstDecl>) Map(AOParser.Ast.IConstTypeVarListDecl.TypeDeclList o)
        {
            var pds = new List<CPParser.Ast.ProcDecl>();
            var cds = new List<CPParser.Ast.ConstDecl>();
            var res = new CPParser.Ast.IConstTypeVarListDecl.TypeDeclList();
            foreach (var item in o.Values.Cast<AOParser.Ast.TypeDecl>())
            {
                var m = Map(item);
                res.Values.Add(m.Item1);
                if (m.Item2 != null)
                {
                    pds.AddRange(m.Item2);
                }
                if (m.Item3 != null)
                {
                    cds.AddRange(m.Item3);
                }
            }
            return (res, pds, cds);
        }

        public (CPParser.Ast.IConstTypeVarListDecl.VarDeclList, List<CPParser.Ast.ProcDecl>, List<CPParser.Ast.ConstDecl>) Map(AOParser.Ast.IConstTypeVarListDecl.VarDeclList o)
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
            return (res, pds, null);
        }

        public CPParser.Ast.ConstDecl Map(AOParser.Ast.ConstDecl o)
        {
            return new CPParser.Ast.ConstDecl {
                ConstExpr = Map(o.ConstExpr, null),
                IdentDef = Map(o.IdentDef)
            };
        }
        public (CPParser.Ast.IType.PointerType, List<CPParser.Ast.ProcDecl>, List<CPParser.Ast.ConstDecl>) Map(AOParser.Ast.IType.ObjectType o)
        {
            var vars = o.DeclSeq.ConstTypeVarDecls
                        .Where(x => x is AOParser.Ast.IConstTypeVarListDecl.VarDeclList)
                        .Cast<AOParser.Ast.IConstTypeVarListDecl.VarDeclList>()
                        .SelectMany(x => x.Values)
                        .Cast<AOParser.Ast.VarDecl>()
                        .Select(x => new CPParser.Ast.FieldList {
                            IdentList = Map(x.IdentList),
                            Type_ = Map(x.Type_, null).Item1//TODO support procs for anonymous objects
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
                            StatementSeq = Map(x.Body.StatBlock, x.ProcHead.FormalPars?.Qualident?.TypeDescr ?? Common.SymTable.TypeDesc.None),
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
                    FieldList = new CPParser.Ast.AstList(vars),
                    RecordMeta = CPParser.Ast.IType.RecordType.Meta.EXTENSIBLE
                }
            };
            return (tp, procs,null);
        }

        public (CPParser.Ast.IType, List<CPParser.Ast.ProcDecl>, List<CPParser.Ast.ConstDecl>) Map(AOParser.Ast.IType o, AOParser.Ast.IdentDef identDef)
        {
            switch (o)
            {
                case AOParser.Ast.IType.ProcedureType t:
                    return (Map(t), null,null);
                case AOParser.Ast.IType.ArrayType t:
                    return (Map(t), null, null);
                case AOParser.Ast.IType.ObjectType t:
                    return Map(t);
                case AOParser.Ast.IType.PointerType t:
                    return (Map(t), null, null);
                case AOParser.Ast.IType.RecordType t:
                    return (Map(t), null, null);
                case AOParser.Ast.IType.SynonimType t:
                    return (Map(t), null, null);
                case AOParser.Ast.IType.EnumType t:
                    return Map(t, identDef);
                default:
                    throw new Exception();
            }
        }
        public (CPParser.Ast.IType, List<CPParser.Ast.ProcDecl>, List<CPParser.Ast.ConstDecl>) Map(AOParser.Ast.IType.EnumType o, AOParser.Ast.IdentDef identDef)
        {
            var t = new CPParser.Ast.IType.SynonimType() {Qualident = new CPParser.Ast.Qualident(null) { Ident1 = new CPParser.Ast.Ident() { Name = "INTEGER"} } };
            var consts = new List<CPParser.Ast.ConstDecl>();
            var i = 0;
            foreach (var item in o.Enums.Cast<AOParser.Ast.EnumItem>())
            {
                var ident = Map(item.IdentDef);
                if (ident != null) {
                    ident.Ident.Name = $"{identDef?.Ident.Name}_{ident.Ident.Name}";

                }
                var expr = Map(item.Expr, null);
                if (expr == null) {
                    expr = new CPParser.Ast.Expr
                    {
                        SimpleExpr = new CPParser.Ast.SimpleExpr
                        {
                            Term = new CPParser.Ast.Term
                            {
                                Factor = new CPParser.Ast.IFactor.NumberFactor
                                {
                                    Value = new CPParser.Ast.Number
                                    {
                                        Value = i.ToString()
                                    }
                                }
                            }
                        }
                    };
                    i++;
                }
                consts.Add(new CPParser.Ast.ConstDecl { 
                    ConstExpr = new CPParser.Ast.ConstExpr { Expr = expr },
                    IdentDef = ident,
                });
            }


            return (t, null, consts);
        }
        public (CPParser.Ast.TypeDecl, List<CPParser.Ast.ProcDecl>, List<CPParser.Ast.ConstDecl>) Map(AOParser.Ast.TypeDecl o)
        {
            
            var m = Map(o.Type_, o.IdentDef);
            var tp = new CPParser.Ast.TypeDecl
            {
                Type_ = m.Item1,
                IdentDef = Map(o.IdentDef)
            };
            return (tp, m.Item2, m.Item3);
        }

        public (CPParser.Ast.VarDecl, List<CPParser.Ast.ProcDecl>) Map(AOParser.Ast.VarDecl o)
        {
            var m = Map(o.Type_,null);
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
                IdentDef = Map(o.ProcHead.IdentDef),
                DeclSeq = Map(o.DeclSeq),
                FormalPars = Map(o.ProcHead.FormalPars),
                StatementSeq = Map(o.Body.StatBlock, o.ProcHead.FormalPars?.Qualident?.TypeDescr ?? Common.SymTable.TypeDesc.None),
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
                Type_ = Map(o.Type_, null).Item1
            };
        }

        public CPParser.Ast.ExprList Map(AOParser.Ast.ExprList o, Common.SymTable.TypeDesc[] expectedTypes)
        {
            if (o == null) return null;

            var res = new CPParser.Ast.ExprList
            {

            };
            var i = 0;
            foreach (var e in o.Exprs.Cast<AOParser.Ast.Expr>())
            {
                res.Exprs.Add(Map(e, expectedTypes==null?null: expectedTypes[i]));
                i++;
            }
            return res;
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

        public CPParser.Ast.ConstExpr Map(AOParser.Ast.ConstExpr o, Common.SymTable.TypeDesc expecteType)
        {
            if (o == null) return null;
            return new CPParser.Ast.ConstExpr {
                Expr = Map(o.Expr, expecteType)
            };
        }

        public CPParser.Ast.CaseLabels Map(AOParser.Ast.CaseLabels o)
        {
            return new CPParser.Ast.CaseLabels {
                ConstExpr1 = Map(o.ConstExpr1, null),
                ConstExpr2 = Map(o.ConstExpr2, null)
            };
        }
        public CPParser.Ast.StatementSeq Map(AOParser.Ast.StatBlock o, Common.SymTable.TypeDesc expecteType)
        {
            if (o == null) return null;
            var res = Map(o.StatementSeq, expecteType);
            if (o.Flags!=null) {
                if (o.Flags.Values.Any())
                {
                    res.CommentsBefore.Add(GetNotSupportedWarning(o.Flags.ToString()));
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
                case AOParser.Ast.IStatement.IgnoreStatement s:
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
            return new CPParser.Ast.Element { 
                Expr1 = Map(o.Expr1, Common.SymTable.TypeDesc.INT32),
                Expr2 = Map(o.Expr2, Common.SymTable.TypeDesc.INT32)
            };
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
                                Factor = Map(t.Factor, null)
                            }
                        }
                    }
                };
            } else {
                yield return Map(t.Factor, null);
            }
            foreach (var te in t.TermElements.Cast<AOParser.Ast.TermElementExpr>())
            {
                yield return Map(te.AddOp);
                yield return Map(te.Factor, null);
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



        private CPParser.Ast.IFactor Map(AOParser.Ast.IFactor factor, Common.SymTable.TypeDesc expectedType)
        {
            switch (factor)
            {
                case AOParser.Ast.IFactor.ExprFactor f:
                    return Map(f, expectedType);
                case AOParser.Ast.IFactor.NilFactor f:
                    return Map(f);
                case AOParser.Ast.IFactor.SetFactor f:
                    return Map(f);
                case AOParser.Ast.IFactor.CharacterFactor f:
                    return Map(f);
                case AOParser.Ast.IFactor.DesignatorFactor f:
                    return Map(f);
                case AOParser.Ast.IFactor.NegFactor f:
                    return Map(f, expectedType);
                case AOParser.Ast.IFactor.NumberFactor f:
                    return Map(f);
                case AOParser.Ast.IFactor.StringFactor f:
                    return Map(f);
                case AOParser.Ast.IFactor.SizeOfFactor f:
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

        public CPParser.Ast.Expr Map(AOParser.Ast.Expr o, Common.SymTable.TypeDesc toType)
        {
            if (o == null) return null;
            o = DownOrUpCast(o, toType);
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

            if (toType == null) return e;
            if (!toType.IsSimple || !e.TypeDescr.IsSimple) return e;
            if (toType.form == e.TypeDescr.form) return e;
            var downcast = toType.form < e.TypeDescr.form;

            var fn = "(*WARN CONV CAST*)" + (downcast ? "SHORT" : "LONG");
            
            return new AOParser.Ast.Expr(e.scope) { 
                SimpleExpr = new AOParser.Ast.SimpleExpr { 
                    Term = new AOParser.Ast.Term { 
                        Factor = new AOParser.Ast.IFactor.DesignatorFactor() { 
                            Value = new AOParser.Ast.Designator(e.scope) { 
                                Qualident = new AOParser.Ast.Qualident(e.scope) {Ident1 = new AOParser.Ast.Ident() { Name = fn} },
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
                Expr = Map(o.Expr, o.Designator.TypeDescr),
            };
            return res;
        }

        public CPParser.Ast.IStatement.ProcCallStatement Map(AOParser.Ast.IStatement.ProcCallStatement o)
        {
            var fType = o.Designator.TypeDescr;
            var tps = fType.parameters?.Select(x => x.type)?.ToArray();
            return new CPParser.Ast.IStatement.ProcCallStatement { 
                Designator = Map(o.Designator),
                ExprList = Map(o.ExprList, tps)
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
                Expr = Map(o.Expr, null),
                ElseBody = Map(o.ElseBody, expecteType),
                Cases = MapLst<AOParser.Ast.Case, CPParser.Ast.Case>(o.Cases, x=>Map(x, expecteType))
            };
        }

        public CPParser.Ast.IStatement.WhileStatement Map(AOParser.Ast.IStatement.WhileStatement o, Common.SymTable.TypeDesc expecteType)
        {
            return new CPParser.Ast.IStatement.WhileStatement
            {
                Expr = Map(o.Expr, Common.SymTable.TypeDesc.BOOL),
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
                Expr = Map(o.Expr, Common.SymTable.TypeDesc.BOOL),
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
        public CPParser.Ast.IStatement.AssignmentStatement Map(AOParser.Ast.IStatement.IgnoreStatement o, Common.SymTable.TypeDesc expectedType)
        {
            return new CPParser.Ast.IStatement.AssignmentStatement
            {
                Expr = Map(o.Expr, expectedType),
                Designator = new CPParser.Ast.Designator(null) { 
                    Qualident = GetQualident("IGNORE")
                }
            };
        }
        public CPParser.Ast.IStatement.ReturnStatement Map(AOParser.Ast.IStatement.ReturnStatement o, Common.SymTable.TypeDesc expectedType)
        {
            return new CPParser.Ast.IStatement.ReturnStatement {
                Expr = Map(o.Expr, expectedType)
            };
        }


        public CPParser.Ast.IStatement.ForStatement Map(AOParser.Ast.IStatement.ForStatement o, Common.SymTable.TypeDesc expectedType)
        {
            return new CPParser.Ast.IStatement.ForStatement { 
                ByExpr = Map(o.ByExpr, null),
                Expr = Map(o.Expr, null),
                ToExpr = Map(o.ToExpr, null),
                Ident = Map(o.Ident),
                StatementSeq = Map(o.StatementSeq, expectedType)
            };
        }

        public CPParser.Ast.IType.ArrayType Map(AOParser.Ast.IType.ArrayType o)
        {
            return new CPParser.Ast.IType.ArrayType { 
                ConstExprs = MapLst<AOParser.Ast.ConstExpr, CPParser.Ast.ConstExpr>(o.ConstExprs, x => Map(x, null)),
                Type_ = Map(o.Type_, null).Item1
            };
        }
        private CPParser.Ast.Comment GetNotSupportedWarning(string str) {
            if (str == null) return null;
            return new CPParser.Ast.Comment() { Content = "WARN CONV NOT SUPPORTED: " + str };
        }
        public CPParser.Ast.IType.PointerType Map(AOParser.Ast.IType.PointerType o)
        {
            var res = new CPParser.Ast.IType.PointerType
            {
                Type_ = Map(o.Type_, null).Item1
            };

            res.CommentsBefore.Add(GetNotSupportedWarning(o.Flags?.ToString()));

            return res;
        }

        public CPParser.Ast.IType.ProcedureType Map(AOParser.Ast.IType.ProcedureType o)
        {
            return new CPParser.Ast.IType.ProcedureType(null) { 
                FormalPars = Map(o.FormalPars)
            };
        }

        public CPParser.Ast.IType.RecordType Map(AOParser.Ast.IType.RecordType o)
        {
            var lst = o.FieldList.FieldDecl
                        .Cast<AOParser.Ast.FieldDecl>()
                        .Select(x => new CPParser.Ast.FieldList()
                        {
                            IdentList = Map(x.IdentList),
                            Type_ = Map(x.Type_, null).Item1
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

        HashSet<string> skipIfArgTypeEqReturnType = new HashSet<string> { "CHR", "SET32" };

        public CPParser.Ast.IFactor Map(AOParser.Ast.IFactor.DesignatorFactor o)
        {
            var res = new CPParser.Ast.IFactor.DesignatorFactor
            {
                Value = Map(o.Value)
            };

            if (skipIfArgTypeEqReturnType.Contains(o.Value.Qualident.ToString()) && o.Value.Specs.Count() == 1) {
                var call = o.Value.Specs.Value.FirstOrDefault() as AOParser.Ast.Designator.IDesignatorSpec.ProcCallDesignatorSpec;
                if (call != null)
                {
                    var arg = call.Value.Exprs.FirstOrDefault() as AOParser.Ast.Expr;
                    var argType = AOParser.Types.TypeResolver.Resolve(arg.TypeDescr);
                    var returnedType = AOParser.Types.TypeResolver.Resolve(o.Value.Qualident.TypeDescr, true).elemType;
                    if (argType.form == returnedType.form && argType.predefinedName == returnedType.predefinedName) {
                        res.Value.Qualident = GetQualident("");
                        res.Value.Qualident.CommentsBefore.Add(GetRemoveComment(o.Value.Qualident.Ident1.Name));
                    }
                }

            }

            return res;
        }

        public CPParser.Ast.Comment Map(AOParser.Ast.Definition definition)
        {
            throw new NotImplementedException();
        }

        public CPParser.Ast.IFactor.ExprFactor Map(AOParser.Ast.IFactor.ExprFactor o, Common.SymTable.TypeDesc expectedType)
        {
            return new CPParser.Ast.IFactor.ExprFactor
            {
                Value = Map(o.Value, expectedType)
            };
        }

        public CPParser.Ast.IFactor.NegFactor Map(AOParser.Ast.IFactor.NegFactor o, Common.SymTable.TypeDesc expectedType)
        {
            return new CPParser.Ast.IFactor.NegFactor
            {
                Value = Map(o.Value, expectedType)
            };
        }

        public CPParser.Ast.Comment Map(AOParser.Ast.Flags sysFlag)
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
        //replace SIZE OF XXX by SIZE(XXX)
        public CPParser.Ast.IFactor.DesignatorFactor Map(AOParser.Ast.IFactor.SizeOfFactor o)
        {
            var args = new CPParser.Ast.ExprList();
            args.Exprs.Add(new CPParser.Ast.Expr() { 
                SimpleExpr = new CPParser.Ast.SimpleExpr() { 
                    Term = new CPParser.Ast.Term { 
                        Factor = Map(o.Value, null)
                    }
                }
            });
            var d = new CPParser.Ast.Designator(null)
            {
                Qualident = GetQualident("SIZE"),
            };
            d.Specs.Add(new CPParser.Ast.Designator.IDesignatorSpec.ProcCallDesignatorSpec() { 
                Value = args
            });
            return new CPParser.Ast.IFactor.DesignatorFactor
            {
                Value = d
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

            if (AOParser.Types.TypeResolver.Resolve(o.Qualident.TypeDescr).form == SymTable.TypeForm.ENUM && o.Specs.Value.Any()) {
                var selector = ((AOParser.Ast.Designator.IDesignatorSpec.RecordDesignatorSpec)(o.Specs.Value[0])).Value;
                return new CPParser.Ast.Designator(null) {
                    Qualident = GetQualident($"{o.Qualident.ToString()}_{selector}")
                };
            }

            
            if (simpleFuncMap.ContainsKey(o.Qualident.ToString()) && o.Specs.Any() && o.Specs.Value[0] is AOParser.Ast.Designator.IDesignatorSpec.ProcCallDesignatorSpec)
            {
                q = simpleFuncMap[o.Qualident.ToString()];
                
            }

            var tp = AOParser.Types.TypeResolver.Resolve(o.Qualident.TypeDescr);
            //if (tp.form == SymTable.TypeForm.FUNC)

            var res = new CPParser.Ast.Designator(null) { 
                Qualident = q
            };
            foreach (var item in o.Specs.Cast<AOParser.Ast.Designator.IDesignatorSpec>())
            {
                
                res.Specs.Add(Map(item, tp.form == SymTable.TypeForm.FUNC ? tp.parameters?.Select(x=>x.type)?.ToArray():null));
                tp = item.Specify(tp);
            }

            return res;
        }

        private CPParser.Ast.Designator.IDesignatorSpec Map(AOParser.Ast.Designator.IDesignatorSpec arg, Common.SymTable.TypeDesc[] expectedTypes)
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
                    return Map(s, expectedTypes);
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
                Value = Map(o.Value, null)
            };
        }

        public CPParser.Ast.Designator.IDesignatorSpec.CastDesignatorSpec Map(AOParser.Ast.Designator.IDesignatorSpec.CastDesignatorSpec o)
        {
            return new CPParser.Ast.Designator.IDesignatorSpec.CastDesignatorSpec(null)
            {
                Value = Map(o.Value)
            };
        }

        public CPParser.Ast.Designator.IDesignatorSpec.ProcCallDesignatorSpec Map(AOParser.Ast.Designator.IDesignatorSpec.ProcCallDesignatorSpec o, Common.SymTable.TypeDesc[] expectedTypes)
        {
            return new CPParser.Ast.Designator.IDesignatorSpec.ProcCallDesignatorSpec
            {
                Value = Map(o.Value, expectedTypes)
            };
        }

    }
}
