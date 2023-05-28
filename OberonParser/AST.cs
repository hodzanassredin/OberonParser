namespace Ast
{
    enum OType
    {
        UNDEF,
        INT,
        BOOL,
    }
    enum Operator
    {
        EQU,
        LSS,
        GTR,
        ADD,
        SUB,
        MUL,
        DIV,
    }
    abstract class Node
    {

        //  node of the AST
        public abstract void Dump();
    }
    // ----------- Declarations ----------------------------
    class Obj : Node
    {

        //  any declared object that has a name
        public String name;

        //  name of this object
        public OType type;

        //  type of this object (UNDEF for procedures)
        public Obj(String s, OType t)
        {
            this.name = s;
            this.type = t;
        }

        public override void Dump()
        {

        }
    }
    class Var : Obj
    {

        //  variable
        public int adr;

        //  address in memory
        public Var(String name, OType type) :
                base(name, type)
        {

        }
    }
    class Proc : Obj
    {

        //  procedure (also used for the main program)
        public List<Obj> locals;

        //  local objects declared in this procedures
        public Block block;

        //  block of this procedure (null for the main program)
        public int nextAdr;

        //  next free address in this procedure
        public Proc program;

        //  link to the Proc node of the main program or null
        public Parser parser;

        //  for error messages
        public Proc(String name, Proc program, Parser parser) :
                base(name, OType.UNDEF)
        {

            this.locals = new List<Obj>();
            this.program = program;
            this.parser = parser;
        }

        public void add(Obj obj)
        {
            foreach (Obj o in this.locals)
            {
                if (o.name.Equals(obj.name))
                {
                    this.parser.SemErr((obj.name + " declared twice"));
                }

            }

            this.locals.Add(obj);
            if ((obj is Var))
            {
                this.nextAdr++;
            }

            if (obj is Var) ((Var)obj).adr = nextAdr++;
        }

        public Obj find(String name)
        {
            foreach (Obj o in this.locals)
            {
                if (o.name.Equals(name))
                {
                    return o;
                }

            }

            if ((this.program != null))
            {
                foreach (Obj o in this.program.locals)
                {
                    if (o.name.Equals(name))
                    {
                        return o;
                    }

                }

            }

            this.parser.SemErr((name + " undeclared"));
            return new Obj("_undef", OType.INT);
            //  dummy
        }

        public override void Dump()
        {
            Console.WriteLine(("Proc " + name));
            this.block.Dump();
            Console.WriteLine();
        }
    }
    // ----------- Expressions ------------------------------
    abstract class Expr : Node
    {
    }
    class BinExpr : Expr
    {

        public Operator op;

        Expr left;

        Expr right;

        public BinExpr(Expr e1, Operator o, Expr e2)
        {
            this.op = o;
            this.left = e1;
            this.right = e2;
        }

        public override void Dump()
        {
            this.left.Dump();
            Console.Write((" "
                        + (this.op.ToString() + " ")));
            this.right.Dump();
        }
    }
    class UnaryExpr : Expr
    {

        Operator op;

        Expr e;

        public UnaryExpr(Operator x, Expr y)
        {
            this.op = x;
            this.e = y;
        }

        public override void Dump()
        {
            Console.Write((this.op.ToString() + " "));
            this.e.Dump();
        }
    }
    class Ident : Expr
    {

        Obj obj;

        public Ident(Obj o)
        {
            this.obj = o;
        }

        public override void Dump()
        {
            Console.Write(this.obj.name);
        }
    }
    class IntCon : Expr
    {

        int val;

        public IntCon(int x)
        {
            this.val = x;
        }

        public override void Dump()
        {
            Console.Write(this.val);
        }
    }
    class BoolCon : Expr
    {

        bool val;

        public BoolCon(bool x)
        {
            this.val = x;
        }

        public override void Dump()
        {
            Console.Write(this.val);
        }
    }
    // ------------- Statements -----------------------------
    class Stat : Node
    {

        public static int indent = 0;

        public override void Dump()
        {
            for (int i = 0; (i < indent); i++)
            {
                Console.Write("  ");
            }

        }
    }
    class Assignment : Stat
    {

        Obj left;

        Expr right;

        public Assignment(Obj o, Expr e)
        {
            this.left = o;
            this.right = e;
        }

        public override void Dump()
        {
            base.Dump();
            Console.Write((this.left.name + " = "));
            this.right.Dump();
            Console.WriteLine();
        }
    }
    class Call : Stat
    {

        Obj proc;

        public Call(Obj o)
        {
            this.proc = o;
        }

        public override void Dump()
        {
            base.Dump();
            Console.WriteLine(("call " + this.proc.name));
        }
    }
    class If : Stat
    {

        Expr cond;

        Stat stat;

        public If(Expr e, Stat s)
        {
            this.cond = e;
            this.stat = s;
        }

        public override void Dump()
        {
            base.Dump();
            Console.Write("if ");
            this.cond.Dump();
            Console.WriteLine();
            Stat.indent++;
            this.stat.Dump();
            Stat.indent--;
        }
    }
    class IfElse : Stat
    {

        Stat ifPart;

        Stat elsePart;

        public IfElse(Stat i, Stat e)
        {
            this.ifPart = i;
            this.elsePart = e;
        }

        public override void Dump()
        {
            this.ifPart.Dump();
            base.Dump();
            Console.WriteLine("else ");
            Stat.indent++;
            this.elsePart.Dump();
            Stat.indent--;
        }
    }
    class While : Stat
    {

        Expr cond;

        Stat stat;

        public While(Expr e, Stat s)
        {
            this.cond = e;
            this.stat = s;
        }

        public override void Dump()
        {
            base.Dump();
            Console.Write("while ");
            this.cond.Dump();
            Console.WriteLine();
            Stat.indent++;
            this.stat.Dump();
            Stat.indent--;
        }
    }
    class Read : Stat
    {

        Obj obj;

        public Read(Obj o)
        {
            this.obj = o;
        }

        public override void Dump()
        {
            base.Dump();
            Console.WriteLine(("read " + this.obj.name));
        }
    }
    class Write : Stat
    {

        Expr e;

        public Write(Expr x)
        {
            this.e = x;
        }

        public override void Dump()
        {
            base.Dump();
            Console.Write("write ");
            this.e.Dump();
            Console.WriteLine();
        }
    }
    class Block : Stat
    {

        List<Stat> stats = new List<Stat>();

        public void add(Stat s)
        {
            this.stats.Add(s);
        }

        public override void Dump()
        {
            base.Dump();
            Console.WriteLine("Block(");
            Stat.indent++;
            foreach (Stat s in this.stats)
            {
                s.Dump();
            }

            Stat.indent--;
            base.Dump();
            Console.WriteLine(")");
        }
    }
}