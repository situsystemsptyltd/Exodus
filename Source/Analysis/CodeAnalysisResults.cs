using Microsoft.CodeAnalysis;

namespace Exodus.Analysis
{
    class CodeAnalysisResults
    {
        private Dictionary<string, List<CodeReference>> referencesByType = new Dictionary<string, List<CodeReference>>();

        public void Print()
        {
            var keys = referencesByType.Keys.Where(k => k.StartsWith("Unity")).OrderBy(k => k).ToArray();
            foreach (var key in keys)
            {
                var value = referencesByType[key];
                Console.WriteLine($"{key},{value.Count}");
            }
        }

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
    }
}
