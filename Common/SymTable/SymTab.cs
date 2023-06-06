using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.SymTable
{

	public enum ObjCLass
	{
		NONE,
		VAR,
		TYPE,
		FUNC,
		CONST,
		FIELD,
		PARAM,
		MODULE,
		RECIEVER
	}
	//----------------------------------------------------------------------------------------------
	// Objects
	//----------------------------------------------------------------------------------------------
	public class Obj
	{
		public ObjCLass objClass;    // VAR, TYPE, FUNC
		public string name;
		public TypeDesc type;
		public Scope scope;//
		public string value;
		public Obj(ObjCLass objClass, string name, TypeDesc type, string value) { this.objClass = objClass; this.name = name; this.type = type; this.value = value; }

        public override string ToString()
        {
            switch (objClass)
            {
                case ObjCLass.NONE:
                    break;
                case ObjCLass.VAR:
                    break;
                case ObjCLass.TYPE:
                    break;
                case ObjCLass.FUNC:
                    break;
                case ObjCLass.CONST:
                    break;
                case ObjCLass.FIELD:
                    break;
                case ObjCLass.PARAM:
                    break;
                default:
                    break;
            }
            return $"{objClass} {name}"
				+ (type==null || type == TypeDesc.None ? "" : $" : {type}")
				+ (String.IsNullOrEmpty(value) ? "" : $" = {value}");
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
		public static TypeDesc Predefined(String name, Scope scope)
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
		private Scope predefinedScope;

		public TypeDesc Resolve() {
			return predefinedScope.Find(predefinedName)?.type;
		}

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
					return $"({String.Join(" ,", this.fieldsOrParams)}) : {elemType}";
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
		public static Obj noObj = new Obj(ObjCLass.NONE, "???", TypeDesc.None, "");
		public Scope()
        {
        }
        public List<Obj> locals = new List<Obj>();

		public Scope outer;

		public Obj Find(string name)
		{
			for (Scope s = this; s != null; s = s.outer)
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
	}


	//----------------------------------------------------------------------------------------------
	// Symbol Table
	//----------------------------------------------------------------------------------------------
	public class SymTab
	{
		public Scope curScope;


		public SymTab()
		{
		}
		public void Insert(IEnumerable<Obj> objs)
		{
            foreach (var obj in objs)
            {
				Insert(obj);
			}
		}
		//public void InsertMany(ObjCLass objClass, string[] names, TypeDesc type, String value = "")
		//{
		//	foreach (var name in names)
		//	{
		//		Insert(objClass, name, type, value);
		//	}
		//}
		public Obj Insert(ObjCLass objClass, string name, TypeDesc type, String value = "")
		{
			Obj obj = new Obj(objClass, name, type, value);
			return Insert(obj);
		}

		public Obj Insert(Obj obj)
		{
			foreach (Obj x in curScope.locals)
			{
				if (x.name.Equals(obj.name)) Error("-- " + obj.name + " declared twice");
			}
			curScope.locals.Add(obj);
			return obj;
		}

		public Obj Find(string name)
		{
			return curScope.Find(name);
		}

		public void OpenScope()
		{
			Scope s = curScope;
			curScope = new Scope();
			curScope.outer = s;
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
