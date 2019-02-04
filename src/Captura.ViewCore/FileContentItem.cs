using System.IO;

namespace Captura.ViewCore
{
    public class FileContentItem
    {
        public string FileName { get; }

        public FileContentItem(string fileName)
        {
            FileName = fileName;

            Name = Path.GetFileNameWithoutExtension(fileName);
        }

        public string Name { get; }

        public string Content => File.Exists(FileName) ? File.ReadAllText(FileName) : null;
    }
}