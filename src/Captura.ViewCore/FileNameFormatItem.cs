namespace Captura.ViewCore
{
    public class FileNameFormatItem
    {
        public FileNameFormatItem(string format, string description)
        {
            Format = format;
            Description = description;
        }

        public string Format { get; }

        public string Description { get; }
    }
}