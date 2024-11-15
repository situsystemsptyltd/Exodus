using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Exodus.Stride
{
    public class StrideCodeConverter 
    {
        public StrideCodeConverter(Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            this.document = document;
        }

        SyntaxNode? root;
        SemanticModel?  semanticModel;
        Document document;
        DocumentEditor? editor;


        public async Task<string> TransformCodeAsync()
        {
            root = await document.GetSyntaxRootAsync();
            semanticModel = await document.GetSemanticModelAsync();
            editor = await DocumentEditor.CreateAsync(document);

            if (root == null || semanticModel == null || editor == null)
            {
                return string.Empty;
            }

            ReplaceUsingDirectives();

            //ConvertMonoBehavioursToScriptComponents();

           

            var newDocument = editor.GetChangedDocument();

            root = await newDocument.GetSyntaxRootAsync();

            return root.NormalizeWhitespace().ToFullString();


            //// For now, replace using the Syntax tree

            //var sourceText = await document.GetTextAsync();
            //var source = sourceText.ToString();

            //return SourceFileConverter.TransformCode(source);
        }

        public void ReplaceUsingDirectives()
        {
            var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToList();
            if (usingDirectives.Any()) 
            {

//                var expression = SyntaxFactory.ParseExpression("using Stride.Entities;\nusing Stride.Collections;\nusing Stride.Core.Mathematics;\n");

                // Add Stride using directives
                editor.InsertAfter(usingDirectives.Last(),
                                    editor.Generator.NamespaceImportDeclaration("Stride.Entities")
                                    .WithTrailingTrivia(SyntaxFactory.SyntaxTrivia(SyntaxKind.EndOfLineTrivia, "\n")));
                editor.InsertAfter(usingDirectives.Last(),
                                    editor.Generator.NamespaceImportDeclaration("Stride.Collections")
                                    .WithTrailingTrivia(SyntaxFactory.SyntaxTrivia(SyntaxKind.EndOfLineTrivia, "\n")));
                editor.InsertAfter(usingDirectives.Last(),
                                    editor.Generator.NamespaceImportDeclaration("Stride.Core.Mathematics")
                                    .WithTrailingTrivia(SyntaxFactory.SyntaxTrivia(SyntaxKind.EndOfLineTrivia, "\n")));

                // Remove Unity Using directives
                foreach (var usingDirective in usingDirectives)
                {
                    if (usingDirective.Name is not null && usingDirective.Name.ToString().Contains("Unity"))
                    {
                        editor.RemoveNode(usingDirective);
                    }
                }
            }
        }


        private void ConvertMonoBehavioursToScriptComponents()
        {
            throw new NotImplementedException();
        }
    }
}
