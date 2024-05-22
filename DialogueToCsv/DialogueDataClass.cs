using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DialogueToCsv;

public class DialogueDataClass : INotifyPropertyChanged
{
    private int _id;

    public int ID
    {
        get => _id;
        set => SetField(ref _id, value);
    }

    private string _name;

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }
    private string _dialogueText;

    public string DialogueText
    {
        get => _dialogueText;
        set => SetField(ref _dialogueText, value);
    }

    private int _toID;

    public int ToID
    {
        get => _toID;
        set => SetField(ref _toID, value);
    }

    private int _hasChoices;

    public int HasChoices
    {
        get => _hasChoices;
        set => SetField(ref _hasChoices, value);
    }

    private string _choicesText;

    public string ChoicesText
    {
        get => _choicesText;
        set => SetField(ref _choicesText, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}