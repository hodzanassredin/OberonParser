using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.SymTable
{

	public enum ObjCLass
	{
		VAR,
		TYPE,
		FUNC,
		CONST,
		FIELD,
		PARAM
	}
	//----------------------------------------------------------------------------------------------
	// Objects
	//----------------------------------------------------------------------------------------------
	public class Obj
	{
		public ObjCLass objClass;    // VAR, TYPE, FUNC
		public string name;
		public TypeDesc type;

		public string value;
		public Obj(ObjCLass objClass, string name, TypeDesc type, string value) { this.objClass = objClass; this.name = name; this.type = type; this.value = value; }

        public override string ToString()
        {
            switch (objClass)
            {
                case ObjCLass.VAR:
				case ObjCLass.PARAM:
				case ObjCLass.FIELD:
					return $"{name} : {type}" + (String.IsNullOrEmpty(value) ? "" : $" = {value}");
				case ObjCLass.TYPE:
					return $"{name} = {type};";
				case ObjCLass.FUNC:
					return $"PROCEDURE {name} {type} = {value};";
				case ObjCLass.CONST:
					return $"{name} = {value};";
				default:
					return $"";
			}

        }
    }


	//----------------------------------------------------------------------------------------------
	// Types
	//----------------------------------------------------------------------------------------------
	public enum TypeForm {
		NONE = 0, STRUCT = 10, UNION = 11, ENUM = 12, PTR = 13, ARRAY = 14, FUNC = 15, PREDEFINED = 16
	}

	public class TypeDesc
	{
		public static TypeDesc Predefined(String name)
		{
			return new TypeDesc(TypeForm.PREDEFINED)
			{
				predefinedName = name
			};
		}
		public static TypeDesc Function(TypeDesc returnType, params Obj[] parameters) {
			return new TypeDesc(TypeForm.FUNC)
			{
				elemType = returnType,
				fieldsOrParams = parameters.ToList()
			};
		}

		public static TypeDesc Pointer(TypeDesc toType)
		{
			return new TypeDesc(TypeForm.PTR)
			{
				elemType = toType,
			};
		}

		public static TypeDesc Array(TypeDesc elemType, int[] length)
		{
			return new TypeDesc(TypeForm.ARRAY)
			{
				elemType = elemType,
				length = length
			};
		}

		public static TypeDesc Struct(TypeDesc baseType, params Obj[] fields)
		{
			return new TypeDesc(TypeForm.STRUCT)
			{
				fieldsOrParams = fields.ToList(),
				elemType = baseType,
			};
		}

		public static TypeDesc Enum(TypeDesc baseType, params Obj[] vars)
		{
			return new TypeDesc(TypeForm.ENUM)
			{
				fieldsOrParams = vars.ToList(),
				elemType = baseType,
			};
		}

		public static TypeDesc // predeclared primitive types
			None = new TypeDesc(TypeForm.NONE);

		public TypeForm form;     // NONE, CHAR, SHORT, ...
								  //public Obj obj;      // to defining object if such an object exists. base type
		public int[] length = new int[0];//for arrays, can be without size or multidim
		public TypeDesc elemType; // for ARRAY, PTR, FUNC
		public List<Obj> fieldsOrParams;   // for STRUCT, UNION, ENUM
        private string predefinedName;

        private TypeDesc(TypeForm form) { this.form = form; }

        public override string ToString()
        {
            switch (form)
            {
                case TypeForm.NONE:
					return "NONE";
				case TypeForm.STRUCT:
					return $"RECORD ({elemType}) BEGIN {String.Join(";", this.fieldsOrParams)} END;";
                case TypeForm.UNION:
					return $"UNION TODO";
				case TypeForm.ENUM:
					return $"ENUM ({elemType}) BEGIN {String.Join(";", this.fieldsOrParams)} END;";
                case TypeForm.PTR:
					return $"POINTER TO {elemType}";
                case TypeForm.ARRAY:
					var size = length.Length == 0 ? "" : "[" + String.Join(", ", length.Select(x=>x.ToString())) + "]";
					return $"ARRAY OF {elemType}";
				case TypeForm.FUNC:
					return $"PROCEDURE ({String.Join(" ,", this.fieldsOrParams)}) : {elemType}";
                default:
                    break;
            }
			return predefinedName;
        }
    }


	//----------------------------------------------------------------------------------------------
	// Scopes
	//----------------------------------------------------------------------------------------------
	public class Scope
	{
        public Scope(string name)
        {
            this.name = name;
        }
        public List<Obj> locals = new List<Obj>();

		public List<Scope> subScopes = new List<Scope>();
		public Scope outer;
        public readonly string name;

        public override string ToString()
        {
			if (outer==null) return name;
			return $"{outer}.{name}";
        }
    }


	//----------------------------------------------------------------------------------------------
	// Symbol Table
	//----------------------------------------------------------------------------------------------
	public class SymTab
	{
		public Scope curScope;
		public Obj noObj;

		public SymTab()
		{
			noObj = new Obj(ObjCLass.VAR, "???", TypeDesc.None, "");
		}
		public void InsertMany(ObjCLass objClass, string[] names, TypeDesc type, String value = "")
		{
            foreach (var name in names)
            {
				Insert(objClass, name, type, value);
			}
		}
		public Obj Insert(ObjCLass objClass, string name, TypeDesc type, String value = "")
		{
			Obj obj = new Obj(objClass, name, type, value);
			foreach (Obj x in curScope.locals)
			{
				if (x.name.Equals(name)) Error("-- " + name + " declared twice");
			}
			curScope.locals.Add(obj);
			return obj;
		}

		public Obj Find(string name)
		{
			for (Scope s = curScope; s != null; s = s.outer)
			{
				foreach (Obj x in s.locals)
				{
					if (x.name.Equals(name)) return x;
				}
			}
			// when all declarations are processed correctly this error should be reported
			// Error("-- " + name + " undeclared");
			return noObj;
		}

		public void OpenScope(String name)
		{
			Scope s = curScope;
			curScope = new Scope(name);
			curScope.outer = s;
			s?.subScopes?.Add(curScope);
		}

		public void CloseScope()
		{
			curScope = curScope.outer;
		}

		private void Error(string s)
		{
			Console.WriteLine(s);
		}

	}
}
