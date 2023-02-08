using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalUsingsAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(GlobalUsingsAnalyzerCodeFixProvider)), Shared]
    public class GlobalUsingsAnalyzerCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get => ImmutableArray.Create(GlobalUsingsAnalyzer.DiagnosticId);
        }

        public sealed override FixAllProvider GetFixAllProvider() =>
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var fileName = diagnostic.Properties.ContainsKey("GlobalUsingsFileName") ? diagnostic.Properties["GlobalUsingsFileName"] : "GlobalUsings.cs";
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var syntax = root.FindToken(diagnosticSpan.Start).Parent;
            context.RegisterCodeFix(
                CustomCodeAction.Create(
                title: CodeFixResources.CodeFixTitle,
                createChangedDocument: (c, isPreview) => Fix(context.Document, syntax, isPreview, fileName, c),
                equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                diagnostic
            );
        }

        async Task<Document> Fix(Document document, SyntaxNode syntax, bool isPreview, string filename, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return document;
            }
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (!isPreview)
            {
                var count = 0;
                while (count < 3)
                {
                    try
                    {
                        var csproj = document.Project.FilePath;
                        var file = new FileInfo(csproj ?? Assembly.GetExecutingAssembly().Location);
                        var directory = file.Directory;
                        var text = syntax.GetText().ToString().Trim('\r', '\n');
                        var globalUsings = new FileInfo($"{directory.FullName}{Path.DirectorySeparatorChar}{filename}");
                        if (!(globalUsings.Exists && File.ReadAllText(globalUsings.FullName).Contains(text)))
                        {
                            using (var writer = globalUsings.AppendText())
                            {
                                await writer.WriteLineAsync($"global {text}").ConfigureAwait(false);
                            }
                        }
                    }
                    catch (Exception) { }
                    finally
                    {
                        count++;
                    }
                }
            }
            var newDocument = document
                .WithSyntaxRoot(root.RemoveNodes(new List<SyntaxNode> { syntax },
                SyntaxRemoveOptions.KeepExteriorTrivia));
            var newRoot = await newDocument.GetSyntaxRootAsync();
            if (newRoot.HasLeadingTrivia)
            {
                var trivia = newRoot.GetLeadingTrivia();
                var newTrivia =  trivia.Where(_ => !_.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.EmptyStatement));
                return newDocument.WithSyntaxRoot(newRoot.WithLeadingTrivia(newTrivia));
            }
            return newDocument;
        }
    }
}
