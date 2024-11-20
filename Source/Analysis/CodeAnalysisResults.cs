using Microsoft.CodeAnalysis;

namespace Exodus.Analysis
{
    class CodeAnalysisResults
    {
        private Dictionary<string, List<CodeReference>> referencesByType = new Dictionary<string, List<CodeReference>>();

        internal void AddReference(string fullName, string filePath, Location location)
        {
            if (fullName == null) return;

            if (!referencesByType.TryGetValue(fullName, out var references))
            {
                references = new List<CodeReference>();
                referencesByType.Add(fullName, references);
            }

            references.Add(new CodeReference(filePath, location));

        }

        internal void SaveResultsToFolder(string outputBaseDirectory)
        {
            SaveReferenceSummaryCsv(Path.Combine(outputBaseDirectory, "analysis-summary.csv"));
            SaveReferencesCsv(Path.Combine(outputBaseDirectory, "analysis-references.csv"));
        }

        private void SaveReferenceSummaryCsv(string filePath)
        {
            // Write to the file
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("item,reference_count");
                var keys = referencesByType.Keys.Where(k => k.StartsWith("Unity")).OrderBy(k => k).ToArray();
                foreach (var key in keys)
                {
                    var value = referencesByType[key];
                    writer.WriteLine($"{key},{value.Count}");
                }
            }
        }

        private void SaveReferencesCsv(string filePath)
        {
            // Write to the file
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("item,filepath,line,col");
                var keys = referencesByType.Keys.Where(k => k.StartsWith("Unity")).OrderBy(k => k).ToArray();
                foreach (var key in keys)
                {
                    var references = referencesByType[key];

                    foreach (var reference in references)
                    {
                        var location = reference.Location;
                        var lineSpan = location.GetLineSpan();
                        var lineNumber = lineSpan.StartLinePosition.Line + 1; // 1-based
                        var characterPosition = lineSpan.StartLinePosition.Character + 1; // 1-based

                        writer.WriteLine($"{key},{reference.FilePath},{lineNumber},{characterPosition}");
                    }
                }
            }
        }
    }
}
