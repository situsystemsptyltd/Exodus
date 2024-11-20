using Microsoft.CodeAnalysis;

namespace Exodus.Analysis
{
    class CodeReference
    {
        public CodeReference(string filePath, Location location)
        {
            FilePath = filePath;
            Location = location;
        }

        public string FilePath { get; }
        public Location Location { get; }
    }
}
