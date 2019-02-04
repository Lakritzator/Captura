namespace Captura.Base.Services
{
    public interface IDialogService
    {
        string PickFolder(string current, string description);

        string PickFile(string initialFolder, string description);
    }
}