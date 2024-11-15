using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exodus.Stride
{
    public class StrideConversionTarget : IConversionTarget
    {
        public StrideConversionTarget()
        {

        }

        public async Task<string> TransformCodeAsync(Document document)
        {
            /*

            TODO - Employ a more sophisticated replacement system using the document editor

            var syntaxRoot = await document.GetSyntaxRootAsync();
            var semanticModel = await document.GetSemanticModelAsync();
            var documentEditor = await DocumentEditor.CreateAsync(document);


            //Insert LogConditionWasTrue() before the Console.WriteLine()
            //documentEditor.InsertBefore(ifStatement.Statement.ChildNodes().Single(), conditionWasTrueInvocation);
            //Insert LogConditionWasFalse() after the Console.WriteLine()
            //documentEditor.InsertAfter(ifStatement.Else.Statement.ChildNodes().Single(), conditionWasFalseInvocation);

            var newDocument = documentEditor.GetChangedDocument();

            var sourceText = await newDocument.GetTextAsync();

            */

            // For now, replace using the Syntax tree

            var sourceText = await document.GetTextAsync();
            var source = sourceText.ToString();

            return SourceFileConverter.TransformCode(source);
        }
    }
}
