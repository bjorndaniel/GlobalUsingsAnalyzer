using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = GlobalUsingsAnalyzer.Test.CSharpCodeFixVerifier<
    GlobalUsingsAnalyzer.GlobalUsingsAnalyzer,
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
            var test = @"using System;
    using System.Collections.Generic;
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
            var analyzerFix = new CSharpCodeFixTest<GlobalUsingsAnalyzer,
                GlobalUsingsAnalyzerCodeFixProvider, MSTestVerifier>
            {
                TestState =
                {
                    Sources = { test }
                },
                FixedState =
                {
                    Sources = { fixtest }
                }
            };
            analyzerFix.TestState.ExpectedDiagnostics.Add(
                new DiagnosticResult(
                    "GlobalUsingsAnalyzer",
                    Microsoft.CodeAnalysis.DiagnosticSeverity.Warning
                    ).WithLocation(1, 1));
            analyzerFix.TestState.ExpectedDiagnostics.Add(
    new DiagnosticResult(
        "GlobalUsingsAnalyzer",
        Microsoft.CodeAnalysis.DiagnosticSeverity.Warning
        ).WithLocation(2, 5));
            await analyzerFix.RunAsync();
        }


        [TestMethod]
        public async Task TestMethod3()
        {
            var test = @"
//DEBUG
    using System;
//END
    namespace ConsoleApplication1
    {
        class Class1
        {   
        }
    }";
            var fixtest = @"
//DEBUG
    
//END
    namespace ConsoleApplication1
    {
        class Class1
        {   
        }
    }";

            var analyzerFix = new CSharpCodeFixTest<GlobalUsingsAnalyzer,GlobalUsingsAnalyzerCodeFixProvider, MSTestVerifier>
            {
                TestState =
                {
                    Sources = { test }
                },
                FixedState =
                {
                    Sources = { fixtest }
                }
            };
            analyzerFix.TestState.ExpectedDiagnostics.Add(
                new DiagnosticResult(
                    "GlobalUsingsAnalyzer",
                    Microsoft.CodeAnalysis.DiagnosticSeverity.Warning
                    ).WithLocation(3, 5));
            await analyzerFix.RunAsync();
        }
    }
}
