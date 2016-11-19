using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using System.Diagnostics;
using Neo.IronLua;

namespace LuaTest
{
    class Program
    {
        private static int Mul(int a, int b)
        {
            return a * b;
        }


        static void Main(string[] args)
        {
            MoonSharpTest();
            
            Console.ReadLine();
        }

        private static void DynamicTest()
        {
            dynamic d = new DynObj();

            Console.WriteLine(d.GetTableValue(1, 2));
            Console.WriteLine(d.GetTableValue(-17, 390));
            Console.WriteLine(d.ProcessUsername("PU"));
            Console.WriteLine(d[1]);
            Console.WriteLine(d[2, 3]);

            d.foo = "bar";
            d.baz = "foo";
            Console.WriteLine(d.foo);
            Console.WriteLine(d.baz);
        }

        private static void NeoLuaTest()
        {
            Stopwatch sw = new Stopwatch();

            string scriptCode = @"    
function fact (n)
	if (n == 0) then
		return 1
	else
		return n*fact(n - 1)
	end
end

function testcall ()
    -- return obj.calcHypotenuse(3, 4);
    return obj.calcHypotenuse2();
    -- return mul(3, 4);
    -- return clr.System.Math:Pow(3, 4);
    -- return MyClass.calcHypotenuse(3, 4);
end

function testtable ()
    t.c = t.a + t.b;
end;
    
function testprint ()
    clr.System.Console:WriteLine(""Lorem Ipsum"");
    return 0;
end;

function dyntest ()
    return t.GetTableValue(3, 2);
end;  

function navtest ()
    --return nav.test2(""3"", 2.74);
    --return nav.test0();
    return nav.mul3(10, 20, 30);
end;  
";

            Lua l = new Lua();
            var g = l.CreateEnvironment();
            dynamic dg = g;

            g.RegisterPackage("MyClass", typeof(StaticMyClass));
            g.RegisterPackage("DynClass", typeof(DynObj));

            dg.obj = new MyClass();
            dynamic d = new DynObj();
            dg.dynobj = d;

            dg.mul = new Func<int, int, int>(Mul);
            dg.t = new LuaTable();
            dg.t.a = 2;
            dg.t.b = 4;
            dg.t.GetTableValue = new Func<object, object, string>((a, b) => d.GetTableValue(a, b));

            NAVDispatcher nd = new NAVDispatcher();
            var nav = new LuaTable();
            dg.nav = nav;
            nav["test2"] = new Func<object, object, string>((a, b) => nd.DispatchStringCall("test2", new object[] { a, b }));
            nav["test0"] = new Func<string>(() => nd.DispatchStringCall("test0", new object[] { }));
            nav["mul3"] = new Func<object, object, object, int>((a, b, c) => nd.DispatchIntCall("mul", new object[] { a, b, c }));
            nav["add3"] = new Func<object, object, object, int>((a, b, c) => nd.DispatchIntCall("add", new object[] { a, b, c }));

            g.DoChunk(scriptCode, "test.lua");

            Func<int> testcase =
                () => dg.navtest();
            // () => { dg.testtable(); return dg.t.c; }; 

            sw.Reset();
            sw.Start();
            var res = testcase();
            sw.Stop();

            Console.WriteLine("Result = {0}", res);
            Console.WriteLine("Initial Run Elapsed = {0}", sw.Elapsed);

            long total = 0;

            for (int i = 1; i < 20; i++)
            {
                sw.Reset();
                sw.Start();
                res = testcase();
                sw.Stop();
                total += sw.Elapsed.Ticks;

                Console.WriteLine("Rerun {0}: Result = {1}; Elapsed = {2}", i, res, sw.Elapsed);
            }

            Console.WriteLine("Average Runtime = {0}", total / 20);
        }

        private static void MoonSharpTest()
        {
            Stopwatch sw = new Stopwatch();

            string scriptCode = @"    
function fact (n)
	if (n == 0) then
		return 1
	else
		return n*fact(n - 1)
	end
end

function testcall ()
    -- return obj.calcHypotenuse(3, 4);
    return obj.calcHypotenuse2();
    -- return mul(3, 4);
    -- return clr.System.Math:Pow(3, 4);
    -- return MyClass.calcHypotenuse(3, 4);
end

function testtable ()
    t.c = t.a + t.b;
end;
    
function testprint ()
    clr.System.Console:WriteLine(""Lorem Ipsum"");
    return 0;
end;

function dyntest ()
    return t.GetTableValue(3, 2);
end;  

function navtest ()
    return nav.test2(""3"", 2.74);
    --return nav.test0();
    --return nav:mul3(10, 20, 30);
end;  

function iotest()
    io.input(""e:\\test\\test.txt"");
    return io.read(""*all"");
end;

function loadertest()
    dofile(""e:\\test\\test.lua"");
    return iotest2();
end;

function sandboxtest ()
    return os.date();
    -- return pcall(1);
    -- return iotest();
end;
";

            UserData.RegisterType<MyClass>();

            Script script = new Script(); // CoreModules.Preset_HardSandbox | CoreModules.OS_Time);

            script.Globals["Mul"] = (Func<int, int, int>)Mul;

            DynValue obj = UserData.Create(new MyClass());
            script.Globals.Set("obj", obj);

            NAVDispatcher nd = new NAVDispatcher();
            Table nav = new Table(script);
            script.Globals["nav"] = nav;
            nav["test2"] = new Func<object, object, string>((a, b) => nd.DispatchStringCall("test2", new object[] { a, b }));
            nav["test0"] = new Func<string>(() => nd.DispatchStringCall("test0", new object[] { }));
            nav["mul3"] = new Func<object, object, object, int>((a, b, c) => nd.DispatchIntCall("mul", new object[] { a, b, c }));
            nav["add3"] = new Func<object, object, object, int>((a, b, c) => nd.DispatchIntCall("add", new object[] { a, b, c }));


            script.DoString(scriptCode);

            Func<string> testcase =
                () => script.Call(script.Globals["loadertest"]).String; 

            sw.Reset();
            sw.Start();
            var res = testcase();
            sw.Stop();

            Console.WriteLine("Result = {0}", res);
            Console.WriteLine("Initial Run Elapsed = {0}", sw.Elapsed);

            long total = 0;

            for (int i = 1; i < 20; i++)
            {
                sw.Reset();
                sw.Start();
                res = testcase();
                sw.Stop();
                total += sw.Elapsed.Ticks;

                Console.WriteLine("Rerun {0}: Result = {1}; Elapsed = {2}", i, res, sw.Elapsed);
            }

            Console.WriteLine("Average Runtime = {0}", total / 20);
        }
    }


    class MyClass
    {
        public double calcHypotenuse(double a, double b)
        {
            return Math.Sqrt(a * a + b * b);
        }

        public double calcHypotenuse2()
        {
            double a = 3;
            double b = 4;
            return Math.Sqrt(a * a + b * b);
        }
    }

    static class StaticMyClass
    {
        public static double calcHypotenuse(double a, double b)
        {
            return Math.Sqrt(a * a + b * b);
        }
    }
}
