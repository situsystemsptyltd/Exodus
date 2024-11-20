using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Exodus.Analysis
{
    internal class UnityAnalyzer
    {
        private CodeAnalysisResults codeAnalysisResults;

        public UnityAnalyzer()
        {
            codeAnalysisResults = new CodeAnalysisResults();
        }

        public CodeAnalysisResults Results => codeAnalysisResults;

        internal async Task AnalyzeDocumentAsync(Document document)
        {
            var semanticModel = await document.GetSemanticModelAsync();

            // Get the syntax root of the document
            var syntaxRoot = await document.GetSyntaxRootAsync();

            if (syntaxRoot == null)
            {
                Console.WriteLine("Unable to get syntax root.");
                return;
            }

            // Iterate over all descendant nodes to find references
            foreach (var node in syntaxRoot.DescendantNodes())
            {
                ISymbol? symbol = null;

                // Handle different kinds of nodes where references might occur
                if (node is IdentifierNameSyntax identifierNode)
                {
                    symbol = semanticModel.GetSymbolInfo(identifierNode).Symbol;
                }
                else if (node is MemberAccessExpressionSyntax memberAccessNode)
                {
                    symbol = semanticModel.GetSymbolInfo(memberAccessNode).Symbol;
                }
                else if (node is TypeSyntax typeNode)
                {
                    symbol = semanticModel.GetSymbolInfo(typeNode).Symbol;
                }

                // Process the symbol if it's valid
                if (symbol != null)
                {
                    // Get the containing type of the symbol
                    var containingType = symbol.ContainingType;

                    // Determine the type of the referenced symbol
                    string referencedType = symbol switch
                    {
                        IPropertySymbol propertySymbol => propertySymbol.Type.Name,
                        IMethodSymbol methodSymbol => methodSymbol.ReturnType.ToString(),
                        IFieldSymbol fieldSymbol => fieldSymbol.Type.ToString(),
                        IEventSymbol eventSymbol => eventSymbol.Type.ToString(),
                        ITypeSymbol typeSymbol => typeSymbol.Name, // For class, struct, interface, etc.
                        _ => "Unknown"
                    } ?? "Unknown";

                    var filePath = document.FilePath ?? "Unknown file";

                    var location = node.GetLocation();
                    var lineSpan = location.GetLineSpan();
                    var lineNumber = lineSpan.StartLinePosition.Line + 1; // 1-based
                    var characterPosition = lineSpan.StartLinePosition.Character + 1; // 1-based

                    if (containingType != null)
                    {
                        var fullName = string.IsNullOrEmpty(containingType.ContainingNamespace.Name)
                            ? $"{containingType.Name}.{symbol.Name}"
                            : $"{containingType.ContainingNamespace.Name}.{containingType.Name}.{symbol.Name}";

                        //Console.WriteLine($"Reference: {symbol.Kind}  {fullName} ()");

                        codeAnalysisResults.AddReference(fullName, filePath, location);
                    }



                    //Console.WriteLine($"  File: {filePath}");
                    //Console.WriteLine($"  Line: {lineNumber}, Position: {characterPosition}");

                }
            }
        }
    }
}