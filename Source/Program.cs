
using Exodus;
using Exodus.Stride;

Console.WriteLine("Unity to Stride Converter");

await SolutionConverter.ConvertSolutionAsync("M:\\src\\Git\\SituSystems\\Bimviewer\\Source\\BimViewer\\BimViewer.sln", 
                                       "M:\\test\\UnityToStride",
                                       new StrideConversionTarget());



