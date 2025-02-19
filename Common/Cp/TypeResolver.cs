﻿using CPParser.Ast;
using Common.SymTable;

namespace CPParser.Types
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

        public static TypeDesc Resolve(TypeDesc type)
        {
            if (type.predefinedName == null) return TypeDesc.None;
            if (type.predefinedName == "CHR") 
                return TypeDesc.Function(TypeDesc.UINT16, null, type.scope);
            return type.scope.Find(type.predefinedName).type;
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

        public static TypeDesc ResolveNumber(String number) {
            number = number.Replace("'", "");
            if (number.Contains("."))
            {
                Double v;
                if (Double.TryParse(number.Replace('.', ','), System.Globalization.NumberStyles.Float, null, out v))
                {
                    if (v > float.MaxValue || v < float.MinValue) return TypeDesc.FLOAT64;
                    return TypeDesc.FLOAT32;
                }
            }
            else
            {
                Int64 v = 0;
                if (number.StartsWith("0x"))
                {
                    v = Convert.ToInt64(number.Substring(2), 16);
                }
                else if (number.StartsWith("0b"))
                {
                    v = Convert.ToInt64(number.Substring(2), 2);
                }
                else
                {
                    Int64.TryParse(number, out v);
                }
                if (v > Int32.MaxValue || v < Int32.MinValue) return TypeDesc.INT64;
            }
            return TypeDesc.INT32;
            //else if (number.StartsWith("-"))
            //{
            //	Int64 v;
            //	if (Int64.TryParse(number, out v))
            //	{
            //		if (v > Int32.MaxValue || v < Int32.MinValue) return TypeDesc.INT64;
            //		if (v > Int16.MaxValue || v < Int16.MinValue) return TypeDesc.INT32;
            //		if (v > SByte.MaxValue || v < SByte.MinValue) return TypeDesc.INT16;
            //		return TypeDesc.INT8;
            //	}
            //}
            //else
            //{
            //	UInt64 v;
            //	if (UInt64.TryParse(number, out v))
            //	{
            //		if (v > UInt32.MaxValue || v < UInt32.MinValue) return TypeDesc.UINT64;
            //		if (v > UInt16.MaxValue || v < UInt16.MinValue) return TypeDesc.UINT32;
            //		if (v > byte.MaxValue || v < byte.MinValue) return TypeDesc.UINT32;
            //		return TypeDesc.UINT8;
            //	}
            //}
            //return TypeDesc.None;
        }
    }
}
