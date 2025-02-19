﻿using CParser;
using System;


Console.WriteLine("_________________________________");
Console.WriteLine("CParser");

if (args.Length < 1)
    Console.WriteLine("Syntax : CParser <c source file> { <conditional compilation symbol> }");
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
    
    Console.WriteLine("-- {0} errors dectected", parser.errors.count);
    var sw = new StreamWriter(Console.OpenStandardOutput());
    sw.AutoFlush = true;
    Console.SetOut(sw);
    //var ppv = new PrettyPrintVisitor(sw);
    //var str = parser.builder.Module.ToString();
    //ppv.Visit(parser.builder.Module);
}


