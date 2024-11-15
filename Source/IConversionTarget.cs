using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exodus
{
    internal interface IConversionTarget
    {
        // Analyse the roslyn Unity C# source file and return a transformed version
        Task<string> TransformCodeAsync(Document document);

    }
}
