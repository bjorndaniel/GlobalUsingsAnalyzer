//Adapted from https://gist.github.com/Wolfsblvt/5bc282d1b565238058cc645bcf888db7

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GlobalUsingsAnalyzer
{
    public class CustomCodeAction : CodeAction
    {
        private readonly Func<CancellationToken, bool, Task<Document>> _createChangedSolution;

        public override string EquivalenceKey { get; }
        public override string Title { get; }

        protected CustomCodeAction(string title, Func<CancellationToken, bool, Task<Document>> createChangedDocument, string equivalenceKey = null)
        {
            _createChangedSolution = createChangedDocument;
            Title = title;
            EquivalenceKey = equivalenceKey;
        }

        /// <summary>
        ///     Creates a <see cref="CustomCodeAction" /> for a change to a <see cref="Document" /> within a <see cref="Solution" />.
        ///     Use this factory when the changes could have effects that should only be executed when actually implemented such as file writes.
        /// </summary>
        /// <param name="title">Title of the <see cref="CustomCodeAction" />.</param>
        /// <param name="createChangedDocument">Function to create the <see cref="Document" />.</param>
        /// <param name="equivalenceKey">Optional value used to determine the equivalence of the <see cref="CustomCodeAction" /> with other <see cref="CustomCodeAction" />s. See <see cref="CustomCodeAction.EquivalenceKey" />.</param>
        public static CustomCodeAction Create(string title, Func<CancellationToken, bool, Task<Document>> createChangedDocument, string equivalenceKey = null)
        {
            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (createChangedDocument == null)
            {
                throw new ArgumentNullException(nameof(createChangedDocument));
            }

            return new CustomCodeAction(title, createChangedDocument, equivalenceKey);
        }

        protected override async Task<IEnumerable<CodeActionOperation>> ComputePreviewOperationsAsync(CancellationToken cancellationToken)
        {
            const bool isPreview = true;
            // Content copied from http://source.roslyn.io/#Microsoft.CodeAnalysis.Workspaces/CodeActions/CodeAction.cs,81b0a0866b894b0e,references
            var changedDocument = await GetChangedDocumentWithPreviewAsync(cancellationToken, isPreview).ConfigureAwait(false);
            return new CodeActionOperation[] { new ApplyChangesOperation(changedDocument.Project.Solution) };
        }


        protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
        {
            const bool isPreview = false;
            return GetChangedDocumentWithPreviewAsync(cancellationToken, isPreview);
        }


        protected virtual Task<Document> GetChangedDocumentWithPreviewAsync(CancellationToken cancellationToken, bool isPreview) =>
            _createChangedSolution(cancellationToken, isPreview);
    }
}
