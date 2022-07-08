using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = GlobalUsingsAnalyzer.Test.CSharpCodeFixVerifier<
    GlobalUsingsAnalyzer.GlobalUsingsAnalyzerAnalyzer,
    GlobalUsingsAnalyzer.GlobalUsingsAnalyzerCodeFixProvider>;

namespace GlobalUsingsAnalyzer.Test
{
    [TestClass]
    public class GlobalUsingsAnalyzerUnitTest
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task TestMethod1()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task TestMethod2()
        {
            var test = @"
    {|#0:using System;|}
    namespace ConsoleApplication1
    {
        class Class1
        {   
        }
    }";

            var fixtest = @"
    namespace ConsoleApplication1
    {
        class Class1
        {   
        }
    }";

            var expected = VerifyCS.Diagnostic("GlobalUsingsAnalyzer").WithLocation(0);
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
        }
    }
}
