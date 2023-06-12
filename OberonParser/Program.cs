using Common;
using Common.Mappers;

Console.WriteLine("_________________________________");
Console.WriteLine("OberonParser");

if (args.Length < 1)
    Console.WriteLine("Syntax : OberonParser <Oberon source file> { <conditional compilation symbol> }");
else
{
    Console.WriteLine("   Initializing scanner with source file {0}", args[0]);
    Scanner scanner = new Scanner(args[0]);
    Parser parser = new Parser(scanner);
    if (args.Length > 1)
    {
        Console.WriteLine("   Initializing parser with conditional compilation symbols");
        String[] ccs = new String[args.Length - 1];
        System.Array.Copy(args, 1, ccs, 0, ccs.Length);
        //parser.AddConditionalCompilationSymbols(ccs);
    }
    Console.WriteLine("   Parsing source file {0}", args[0]);
    parser.Parse();

    if (parser.errors.count > 0) { return; }

    Console.WriteLine("-- {0} errors dectected", parser.errors.count);
    using (var sw = new StreamWriter(args[0] + ".out"))
    {
        var aoppv = new AOParser.PrettyPrintVisitor(sw);
        parser.module.AcceptWithComments(aoppv);
    }
    if (!StringCompare.CompareFiles(args[0], args[0] + ".out")) {
        return;
    }

    var mapper = new AoToCpMapper();
    var cpModule = mapper.Map(parser.module);

    //using (var sw = new StreamWriter(args[0] + ".out_cp"))
    //{
    //    var ppv = new CPParser.PrettyPrintVisitor(sw);
    //    var str = parser.module.ToString();
    //    cpModule.AcceptWithComments(ppv);
    //}
    var sw2 = new StreamWriter(Console.OpenStandardOutput());
    sw2.AutoFlush = true;
    Console.SetOut(sw2);
    var ppv = new CPParser.PrettyPrintVisitor(sw2);
    var str = parser.module.ToString();
    cpModule.AcceptWithComments(ppv);




}

