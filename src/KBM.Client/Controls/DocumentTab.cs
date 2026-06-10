using System.Windows.Input;

namespace KBM.Client.Controls;

public sealed class DocumentTab
{
    public DocumentTab(string key, string title, object content, ICommand closeCommand)
    {
        Key = key;
        Title = title;
        Content = content;
        CloseCommand = closeCommand;
    }

    public string Key { get; }
    public string Title { get; }
    public object Content { get; }
    public ICommand CloseCommand { get; }
}
