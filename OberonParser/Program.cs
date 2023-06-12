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

    //if (parser.errors.count > 0) { return; }

    Console.WriteLine("-- {0} errors dectected", parser.errors.count);
    using (var sw = new StreamWriter(args[0] + ".out"))
    {
        var aoppv = new AOParser.PrettyPrintVisitor(sw);
        parser.module.AcceptWithComments(aoppv);
    }


    var mapper = new AoToCpMapper();
    var cpModule = mapper.Map(parser.module);

    using (var sw = new StreamWriter(args[0] + ".out_cp"))
    {
        var ppv = new CPParser.PrettyPrintVisitor(sw);
        var str = parser.module.ToString();
        cpModule.AcceptWithComments(ppv);
    }

    using (var sr = new StreamReader(args[0]))
    {
        using (var sr_out = new StreamReader(args[0] + ".out"))
        {
            var txt1 = sr.ReadToEnd();
            var txt2 = sr_out.ReadToEnd();

            var i = 0; var j = 0;
            while (true)
            {
                var firstIsFinished = i == txt1.Length;
                var secondIsFinished = j == txt2.Length;
                if (firstIsFinished && secondIsFinished) break;
                else if (!firstIsFinished && (txt1[i] == '(' && txt1[i + 1] == '*')) i = SkipComment(txt1, i);
                else if (!secondIsFinished && (txt2[j] == '(' && txt2[j + 1] == '*')) j = SkipComment(txt2, j);
                else if (!firstIsFinished && Char.IsWhiteSpace(txt1[i]))
                {
                    i++;
                }
                else if (!secondIsFinished && Char.IsWhiteSpace(txt2[j]))
                {
                    j++;
                }
                else if (txt1[i] == txt2[j])
                {
                    i++;
                    j++;
                }
                else throw new Exception();
            }
        }

    }

    static int SkipComment(string str, int i)
    {
        while (true)
        {
            i++;
            if (i == str.Length) return i;

            if (str[i] == '(' && str[i + 1] == '*')
            {
                SkipComment(str, i);
            }
            if (str[i] == '*' && str[i + 1] == ')') {
                return i+2;       
            }
        }
    }


}

