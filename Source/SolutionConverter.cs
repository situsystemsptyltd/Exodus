using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Exodus.Stride;
using Exodus.Analysis;

namespace Exodus
{
    public static class SolutionConverter
    {

        internal static async Task ConvertSolutionAsync(string solutionFilePath, string outputBaseDirectory, IConversionTarget conversionTarget)
        {
            if (!File.Exists(solutionFilePath))
            {
                Console.WriteLine("Solution file not found");
                return;
            }

            if (string.IsNullOrEmpty(outputBaseDirectory)) 
            {
                Console.WriteLine("Output directory not specified");
                return;
            }

            // Create the MSBuildWorkSpace
            var ws = MSBuildWorkspace.Create();
            var solution = ws.OpenSolutionAsync(solutionFilePath).Result;

            // Select projects which reference the Unity Engine DLL
            var unityProjects = solution.Projects.Where(p => IsUnityProject(p)).ToList();

            // Get a list of all CS files in those unity projects
            var unityDocuments = unityProjects.SelectMany(p => p.Documents.Where(d => d.Name.ToLowerInvariant().EndsWith(".cs"))).ToList();

            var analyzer = new UnityAnalyzer();

            // we process each document one at a time because for a complicated conversion it may rely on semantics present in other documents
            foreach (var document in unityDocuments)
            {
                await analyzer.AnalyzeDocumentAsync(document);
                await ProcessFileAsync(document, outputBaseDirectory, conversionTarget); 
            }

            analyzer.Results.Print();
        }

        // Process each C# file (read, transform, and write back to output directory)
        private static async Task ProcessFileAsync(Document document, string outputBaseDirectory, IConversionTarget conversionTarget)
        {
            // Calculate output file path for each C# file
            string outputFilePath = GetOutputFilePath(document.FilePath, document.Project.FilePath, outputBaseDirectory);

            // Read the content of the file asynchronously
            string content = await File.ReadAllTextAsync(document.FilePath);

            // Transform the code
            string transformedContent = await conversionTarget.TransformCodeAsync(document);

            // Ensure the output directory exists
            string outputDirectory = Path.GetDirectoryName(outputFilePath);
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Write the transformed content asynchronously
            await File.WriteAllTextAsync(outputFilePath, transformedContent);

            // Log to console
            Console.WriteLine(outputFilePath);
        }

        // Check if the project references Unity (look for Unity references in the .csproj file)
        private static bool IsUnityProject(Project project)
        {
            if (project == null)
            {
                return false;
            }

            // check for references of the unity engine or editor
            var dllReferences = project.MetadataReferences
                .Where(r => r is PortableExecutableReference)
                .Select(r => r as PortableExecutableReference)
                .ToList();

            return dllReferences.Any(r => !string.IsNullOrEmpty(r.FilePath) && r.FilePath.Contains("UnityEngine"));
        }

        // Calculate the output file path for the transformed C# file
        private static string GetOutputFilePath(string inputFilePath, string projectFilePath, string outputBaseDirectory)
        {
            // Get the project directory (where the .csproj file is located)
            var projectDirectory = Path.GetDirectoryName(projectFilePath);

            // Compute the relative path of the input file from the project directory
            var relativePath = Path.GetRelativePath(projectDirectory, inputFilePath);

            // Compute the output directory by combining the output base directory with the relative file path
            string outputFilePath = Path.Combine(outputBaseDirectory, relativePath);

            // Ensure the directory structure exists
            string outputDirectory = Path.GetDirectoryName(outputFilePath);
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            return outputFilePath;
        }
    }
}
