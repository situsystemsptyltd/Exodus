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
    public class StrideConversionTarget : IConversionTarget
    {
        public async Task<string> TransformCodeAsync(Document document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var codeConverter = new StrideCodeConverter(document);

            var translatedCode = await codeConverter.TransformCodeAsync();

            return translatedCode;
        }

    }
}
