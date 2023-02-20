using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace GlobalUsingsAnalyzer
{

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class GlobalUsingsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "GlobalUsingsAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Code formatting";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            //var languageVersion = ((CSharpParseOptions)(await context.Document.GetSyntaxTreeAsync(context.CancellationToken))?.Options)?.LanguageVersion ?? LanguageVersion.CSharp6;
            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            //context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
            context.RegisterSyntaxTreeAction(AnalyzeUsings);
        }

        private static void AnalyzeUsings(SyntaxTreeAnalysisContext context)
        {
            var config = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Tree);
            if (config.TryGetValue("dotnet_diagnostic.GlobalUsingsAnalyzer0002.enabled", out var enabled))
            {
                if (enabled?.Equals("false", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    return;
                }
            }

            if (config.TryGetValue("dotnet_diagnostic.GlobalUsingsAnalyzer0001.filename", out var fileName))
            {
                GlobalUsingFileName = fileName;
            }

            Severity = DiagnosticSeverity.Warning;
            if (config.TryGetValue("dotnet_diagnostic.GlobalUsingsAnalyzer0003.severity", out var severityString))
            {

                switch (severityString.ToLower())
                {
                    case "error":
                        Severity = DiagnosticSeverity.Error;
                        break;
                    case "info":
                        Severity = DiagnosticSeverity.Info;
                        break;
                    case "hidden":
                        Severity = DiagnosticSeverity.Hidden;
                        break;
                    default:
                        Severity = DiagnosticSeverity.Warning;
                        break;
                }
            }

            var langVersion = (int)(((CSharpParseOptions)context.Tree.Options)?.SpecifiedLanguageVersion ?? LanguageVersion.CSharp6);
            if (langVersion == 0 || langVersion >= (int)LanguageVersion.CSharp10)
            {
                var root = context.Tree.GetCompilationUnitRoot();
                if (context.Tree.FilePath.Contains(GlobalUsingFileName))
                {
                    return;
                }
                if (root?.Usings.Any() ?? false)
                {
                    var properties = new Dictionary<string, string>
                    {
                        { "GlobalUsingsFileName", GlobalUsingFileName }
                    };
                    foreach (var usng in root.Usings)
                    {
                        var diagnostic = Diagnostic
                            .Create(Rule, usng.GetLocation(), Severity, null, properties.ToImmutableDictionary(), usng.Name, GlobalUsingFileName);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
        public static string GlobalUsingFileName { get; private set; } = "GlobalUsings.cs";
        public static DiagnosticSeverity Severity { get; private set; } = DiagnosticSeverity.Warning;
    }
}