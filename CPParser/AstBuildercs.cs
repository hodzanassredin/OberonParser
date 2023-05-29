
namespace CPParser
{
    public class AstBuilder
    {
        public Ast.Module Module { get; set; } = new Ast.Module();
        private Stack<Ast.DeclSeq> declSeq = new ();
        public AstBuilder()
        {
            declSeq.Push(Module.DeclSeq);
        }
        public void AddImport(Ast.Ident name, Ast.Ident originalName) {
            if (Module.ImportList == null) Module.ImportList = new Ast.ImportList();
            Module.ImportList.Imports.Add(new Ast.Import { 
                Name = name, 
                OriginalName = originalName
            });
        }
    }
}
