using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

namespace FurinaXML;

internal class Program
{
    static void Main()
    {
        if (!RuntimeFeature.IsDynamicCodeCompiled || !RuntimeFeature.IsDynamicCodeSupported || IsDebug())
            MainExec();
        else
            BenchmarkRunner.Run<Benchmark>();
    }
    public static bool IsDebug()
    {
        Assembly assm = Assembly.GetExecutingAssembly();
        object[] attributes = assm.GetCustomAttributes(typeof(DebuggableAttribute), false);
        if (attributes.Length == 0)
            return false;
        foreach (object attr in attributes)
        {
            if (attr is not DebuggableAttribute d)
                continue;
            return d.IsJITOptimizerDisabled;
        }
        return false;
    }
    static void MainExec()
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("File:");
        UTF8XMLReader reader = new(File.ReadAllBytes(Console.ReadLine().AsSpan().Trim().Trim('"').ToString()));
        while (reader.Read())
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(reader.CurrentToken);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(reader.Position);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(reader.Depth);
            Console.ForegroundColor = ConsoleColor.Yellow;
            switch (reader.CurrentToken)
            {
                case TokenType.None:
                    break;
                case TokenType.XMLDecl:
                    Console.WriteLine(reader.GetXMLDecl());
                    break;
                case TokenType.DocTypeDecl:
                    Console.WriteLine(reader.GetDocTypeDecl());
                    break;
                case TokenType.ProcessingInstruction:
                    break;
                case TokenType.Comment:
                case TokenType.Text:
                case TokenType.STagStart:
                case TokenType.ETag:
                    Console.WriteLine(reader.GetString());
                    break;
                case TokenType.Attribute:
                    Console.WriteLine(reader.GetAttribute(true));
                    break;
                default:
                    break;
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("==========");
        }
    }
}
[ShortRunJob]
[MeanColumn]
[MemoryDiagnoser]
[MarkdownExporter]
[RPlotExporter]
public class Benchmark
{
    // 8 KiB
    public static readonly byte[] DataA = File.ReadAllBytes(@"D:\repos\TrID.NET.Decompiled\TrIDNet.Core\bin\Debug\net8.0-windows\defs\e\exe-vb2-16.trid.xml");
    // 19 KiB
    public static readonly byte[] DataB = File.ReadAllBytes(@"D:\myt\Misc\MS\HalseyExperienceServices\private\src\dev\halsey\services\XpingService\XPingAgentTests\GetTimersAndFilterReminders-CO01XpingTest.xml");
    // 5466 KiB
    public static readonly byte[] DataC = File.ReadAllBytes(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.6.2\zh-Hans\System.Web.xml");
    // 113 MiB
    public static readonly byte[] DataD = File.ReadAllBytes(@"C:\Program Files\dotnet\packs\Microsoft.Android.Ref.34\34.0.113\ref\net8.0\Mono.Android.xml");

    [Benchmark]
    public void DataAFurina()
    {
        UTF8XMLReader reader = new(DataA);
        while (reader.Read())
        {
        }
    }
    [Benchmark]
    public void DataASystem()
    {
        XmlReader reader = XmlReader.Create(new MemoryStream(DataA));
        while (reader.Read())
        {
        }
    }
    [Benchmark]
    [InvocationCount(65536)]
    public void DataBFurina()
    {
        UTF8XMLReader reader = new(DataB);
        while (reader.Read())
        {
        }
    }
    [Benchmark]
    [InvocationCount(65536)]
    public void DataBSystem()
    {
        XmlReader reader = XmlReader.Create(new MemoryStream(DataB));
        while (reader.Read())
        {
        }
    }
    [Benchmark]
    [InvocationCount(128)]
    public void DataCFurina()
    {
        UTF8XMLReader reader = new(DataC);
        while (reader.Read())
        {
        }
    }
    [Benchmark]
    [InvocationCount(128)]
    public void DataCSystem()
    {
        XmlReader reader = XmlReader.Create(new MemoryStream(DataC));
        while (reader.Read())
        {
        }
    }
    [Benchmark]
    [InvocationCount(16)]
    public void DataDFurina()
    {
        UTF8XMLReader reader = new(DataD);
        while (reader.Read())
        {
        }
    }
    [Benchmark]
    [InvocationCount(16)]
    public void DataDSystem()
    {
        XmlReader reader = XmlReader.Create(new MemoryStream(DataD));
        while (reader.Read())
        {
        }
    }
}