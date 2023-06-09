using CPParser.Ast;
using Common.SymTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Cp
{
    public class TypeResolver
    {
        public static bool IsSimple(TypeForm form) {
            return form <= TypeForm.FLOAT64;
        }

        public static bool AreSimple(params TypeForm[] form)
        {
            return form.All(IsSimple);
        }

        public static TypeDesc ResolveAddOp(AddOp.AddOps op, TypeForm form1, TypeForm form2) {
            if (!AreSimple(form1, form2)) return TypeDesc.None;
            switch (op)
            {
                case AddOp.AddOps.Add:
                    if (form1 == TypeForm.INT32 && form2 == TypeForm.INT32)
                        return TypeDesc.INT64;
                    if (form1 <= TypeForm.INT32 && form2 <= TypeForm.INT32)
                        return TypeDesc.INT32;
                    if (form1 <= TypeForm.FLOAT32 && form2 <= TypeForm.FLOAT32)
                        return TypeDesc.FLOAT32;
                    return TypeDesc.FLOAT64;
                    break;
                case AddOp.AddOps.Sub:
                    if (form1 == TypeForm.INT32 && form2 == TypeForm.INT32)
                        return TypeDesc.INT64;
                    if (form1 <= TypeForm.INT32 && form2 <= TypeForm.INT32)
                        return TypeDesc.INT32;
                    if (form1 <= TypeForm.FLOAT32 && form2 <= TypeForm.FLOAT32)
                        return TypeDesc.FLOAT32;
                    return TypeDesc.FLOAT64;
                    break;
                case AddOp.AddOps.Or:
                    if (form1 == TypeForm.BOOL && form2 == TypeForm.BOOL)
                        return TypeDesc.BOOL;
                    break;
                default:
                    break;
            }
            return TypeDesc.None;
        }
        public static TypeDesc ResolveMulOp(MulOp.MulOps op, TypeForm form1, TypeForm form2)
        {
            if (!AreSimple(form1, form2)) return TypeDesc.None;
            switch (op)
            {
                case MulOp.MulOps.Mul:
                    if (form1 == TypeForm.INT32 && form2 == TypeForm.INT32)
                        return TypeDesc.INT64;
                    if (form1 <= TypeForm.INT32 && form2 <= TypeForm.INT32)
                        return TypeDesc.INT32;

                    if (form1 <= TypeForm.FLOAT32 && form2 <= TypeForm.FLOAT32)
                        return TypeDesc.FLOAT32;
                    return TypeDesc.FLOAT64;
                    break;
                case MulOp.MulOps.Division:
                    if (form1 == TypeForm.INT32 && form2 == TypeForm.INT32)
                        return TypeDesc.FLOAT64;
                    if (form1 <= TypeForm.FLOAT32 && form2 <= TypeForm.FLOAT32)
                        return TypeDesc.FLOAT32;
                    return TypeDesc.FLOAT64;
                    break;
                case MulOp.MulOps.DIV:
                    if (form1 < TypeForm.INT32 && form2 < TypeForm.INT32)
                        return TypeDesc.INT32;
                    if (form1 == TypeForm.INT32 && form2 == TypeForm.INT32)
                        return TypeDesc.INT64;
                    break;
                case MulOp.MulOps.MOD:
                    if (form1 < TypeForm.INT32 && form2 < TypeForm.INT32)
                        return TypeDesc.INT32;
                    if (form1 == TypeForm.INT32 && form2 == TypeForm.INT32)
                        return TypeDesc.INT64;
                    break;
                case MulOp.MulOps.AND:
                    if (form1 == TypeForm.BOOL && form2 == TypeForm.BOOL)
                        return TypeDesc.BOOL;
                    break;
                default:
                    break;
            }
            return TypeDesc.None;
        }
        public static TypeDesc ResolveNeg(TypeForm form1, TypeForm form2)
        {

            //if (form1 == TypeForm.BOOL && form2 == TypeForm.BOOL)
            //    return TypeDesc.BOOL;
            return TypeDesc.BOOL;
        }
        public static TypeDesc ResolveRel(Relation.Relations op, TypeForm form1, TypeForm form2)
        {
            return TypeDesc.BOOL;
        }
    }
}
