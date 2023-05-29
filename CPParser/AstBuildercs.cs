
namespace CPParser
{
    public class AstBuilder
    {
        public Ast.Module Module { get; set; }

        public void SetModuleName(String name) {
            Module = new Ast.Module() {Ident = new Ast.Ident { Name = name } };
        }

        public void AddImport(String name, String originalName) {
            if (name == originalName) name = String.Empty;
            if (Module.ImportList == null) Module.ImportList = new Ast.ImportList();
            Module.ImportList.Imports.Add(new Ast.Import { Name = new Ast.Ident { Name = name }, OriginalName = new Ast.Ident { Name = originalName } });
        }
    }
}
