namespace Captura.ViewCore
{
    public class FileNameFormatGroup
    {
        public FileNameFormatGroup(string name, FileNameFormatItem[] formats)
        {
            Name = name;
            Formats = formats;
        }

        public string Name { get; }

        public FileNameFormatItem[] Formats { get; }
    }
}