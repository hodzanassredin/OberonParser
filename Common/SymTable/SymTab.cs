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
		NONE = 0, UINT8, INT8, UINT16, INT16, UINT32, INT32, UINT64, INT64, FLOAT32, FLOAT64, STRUCT = 10, UNION = 11, ENUM = 12, PTR = 13, ARRAY = 14, FUNC = 15, PREDEFINED = 16
	}

	public class TypeDesc
	{
		public static TypeDesc UINT8 = new TypeDesc(TypeForm.UINT8);
		public static TypeDesc INT8 = new TypeDesc(TypeForm.INT8);
		public static TypeDesc UINT16 = new TypeDesc(TypeForm.UINT16);
		public static TypeDesc INT16 = new TypeDesc(TypeForm.INT16);
		public static TypeDesc INT32 = new TypeDesc(TypeForm.INT32);
		public static TypeDesc INT64 = new TypeDesc(TypeForm.INT64);
		public static TypeDesc UINT32 = new TypeDesc(TypeForm.UINT32);
		public static TypeDesc UINT64 = new TypeDesc(TypeForm.UINT64);
		public static TypeDesc FLOAT32 = new TypeDesc(TypeForm.FLOAT32);
		public static TypeDesc FLOAT64 = new TypeDesc(TypeForm.FLOAT64);

		public static TypeDesc Predefined(String name, Scope scope)
		{
			return new TypeDesc(TypeForm.PREDEFINED)
			{
				predefinedName = name,
				scope = scope
			};
		}
		public static TypeDesc Function(TypeDesc returnType, Scope scope) {
			return new TypeDesc(TypeForm.FUNC)
			{
				elemType = returnType,
				scope = scope
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

		public static TypeDesc Struct(TypeDesc baseType, Scope scope)
		{
			return new TypeDesc(TypeForm.STRUCT)
			{
				elemType = baseType,
				scope = scope
			};
		}

		public static TypeDesc Enum(TypeDesc baseType, Scope scope)
		{
			return new TypeDesc(TypeForm.ENUM)
			{
				elemType = baseType,
				scope = scope
			};
		}

		public static TypeDesc // predeclared primitive types
			None = new TypeDesc(TypeForm.NONE);

		public TypeForm form;     // NONE, CHAR, SHORT, ...
								  //public Obj obj;      // to defining object if such an object exists. base type
		public int[] length = new int[0];//for arrays, can be without size or multidim
		public TypeDesc elemType; // for ARRAY, PTR, FUNC

		public string predefinedName;
		public Scope scope;

		public TypeDesc Resolve() {
			return scope.Find(predefinedName)?.type;
		}

		private TypeDesc(TypeForm form) { this.form = form; }

        public override string ToString()
        {
            switch (form)
            {
                case TypeForm.NONE:
					return "NONE";
				case TypeForm.STRUCT:
					return $"RECORD ({elemType}) BEGIN {scope} END;";
                case TypeForm.UNION:
					return $"UNION TODO";
				case TypeForm.ENUM:
					return $"ENUM ({elemType}) BEGIN {scope} END;";
                case TypeForm.PTR:
					return $"POINTER TO {elemType}";
                case TypeForm.ARRAY:
					var size = length.Length == 0 ? "" : "[" + String.Join(", ", length.Select(x=>x.ToString())) + "]";
					return $"ARRAY {size} OF {elemType}";
				case TypeForm.FUNC:
					return $"({scope}) : {elemType}";
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
		bool isSelf = false;
		public static Obj noObj = new Obj(ObjCLass.NONE, "???", TypeDesc.None, "");
		public Scope(bool isSelf)
        {
            this.isSelf = isSelf;	
        }
        public List<Obj> locals = new List<Obj>();

		public Scope outer;

		public bool IsSelf(string name)
		{
			for (Scope s = this; s != null; s = s.outer)
			{
				foreach (Obj x in s.locals)
				{
					if (x.name.Equals(name)) return s.isSelf;
				}
			}
			// when all declarations are processed correctly this error should be reported
			// Error("-- " + name + " undeclared");
			return false;
		}

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
        public override string ToString()
        {
			return String.Join(Environment.NewLine, locals);
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

		public void OpenScope(bool isSelf = false)
		{
			Scope s = curScope;
			curScope = new Scope(isSelf);
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
