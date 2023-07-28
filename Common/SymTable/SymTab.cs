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
		BOOL,
		UINT8,
		INT8,
		CHAR8,
		CHAR16,
		UINT16,
		INT16,
		UINT32,
		INT32,
		UINT64,
		INT64,
		FLOAT32,
		FLOAT64,
		NONE,
		
		STRUCT, UNION, ENUM, PTR, ARRAY, FUNC, PREDEFINED
	}
	public class TypeDesc
	{
		public int GetSize
		{
			get
			{
				switch (form)
				{
					case TypeForm.BOOL:
						return 8;
					case TypeForm.UINT8:
						return 8;
					case TypeForm.INT8:
						return 8;
					case TypeForm.UINT16:
						return 16;
					case TypeForm.INT16:
						return 16;
					case TypeForm.UINT32:
						return 32;
					case TypeForm.INT32:
						return 32;
					case TypeForm.UINT64:
						return 64;
					case TypeForm.INT64:
						return 64;
					case TypeForm.FLOAT32:
						return 32;
					case TypeForm.FLOAT64:
						return 64;
					case TypeForm.CHAR8:
						return 8;
					case TypeForm.CHAR16:
						return 16;
					case TypeForm.NONE:
						return 0;
					case TypeForm.STRUCT:
						return 0;
					case TypeForm.UNION:
						return 0;
					case TypeForm.ENUM:
						return 0;
					case TypeForm.PTR:
						return 0;
					case TypeForm.ARRAY:
						return 0;
					case TypeForm.FUNC:
						return 0;
					case TypeForm.PREDEFINED:
						return 0;
					default:
						return 0;
				}
			}
		}
		public bool IsUnsigned => form == TypeForm.UINT8 
								|| form == TypeForm.UINT16
								|| form == TypeForm.UINT32
								|| form == TypeForm.UINT64;
		public bool IsSimple => form <= TypeForm.FLOAT64;

		
		public static TypeDesc BOOL = new TypeDesc(TypeForm.BOOL);
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

		public static TypeDesc CHAR8 = new TypeDesc(TypeForm.CHAR8);
		public static TypeDesc CHAR16 = new TypeDesc(TypeForm.CHAR16);
		public static TypeDesc Predefined(String name, Scope scope)
		{
			return new TypeDesc(TypeForm.PREDEFINED)
			{
				predefinedName = name,
				scope = scope
			};
		}
		public static TypeDesc Function(TypeDesc returnType, Obj[] parameters, Scope scope) {
			return new TypeDesc(TypeForm.FUNC)
			{
				elemType = returnType,
				scope = scope,
				parameters = parameters
			};
		}

		public static TypeDesc Pointer(TypeDesc toType)
		{
			return new TypeDesc(TypeForm.PTR)
			{
				elemType = toType,
			};
		}

		public static TypeDesc Array(TypeDesc elemType, string[] length)
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
		public string[] length = new string[0];//for arrays, can be without size or multidim
		public TypeDesc elemType; // for ARRAY, PTR, FUNC

		public string predefinedName;
		public Scope scope;
		public Obj[] parameters;
		private TypeDesc(TypeForm form) { this.form = form; }

        public override string ToString()
        {
            switch (form)
            {
				case TypeForm.PREDEFINED:
					return predefinedName;
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
			return this.form.ToString();
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

		public Obj InsertFunc(String name, TypeDesc returnType, params TypeDesc[] args) {
			var argsList = args.Select((x, i) => new Obj(ObjCLass.PARAM, $"arg{i}", x, "")).ToArray();
			var fnType = TypeDesc.Function(returnType, argsList, curScope);
			var obj = new Obj(ObjCLass.FUNC, name, fnType, null);
			Insert(obj);
			return obj;
		}

		public Obj Find(string name)
		{
			return curScope.Find(name);
		}

		public Scope OpenScope(bool isSelf = false)
		{
			Scope s = curScope;
			curScope = new Scope(isSelf);
			curScope.outer = s;
			return curScope;
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
