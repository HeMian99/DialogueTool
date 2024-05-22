using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace DialogueToCsv;

public partial class TestDialogue : Window
{
    public ObservableCollection<DialogueDataClass> Dialogues { get; set; }
    public int CurrentDialogIndex { get; set; } = 0;

    private Dictionary<Button, int> buttonToIDDic;
    public TestDialogue(ObservableCollection<DialogueDataClass> dialogues)
    {
        Dialogues = dialogues;
        DataContext = this;
        InitializeComponent();
        ShowDialogue(Dialogues[CurrentDialogIndex]);
    }

    private void ShowDialogue(DialogueDataClass dialogue)
    {
        buttonToIDDic = new Dictionary<Button, int>();
        NameTextBox.Text = dialogue.Name;
        DialogueTextBox.Text = dialogue.DialogueText;
        Button[] choiseTextBoxes = new Button[] { Choice1, Choice2, Choice3, Choice4, Choice5, Choice6, Choice7 };
        foreach(var textBox in choiseTextBoxes)
        {
            textBox.Visibility = Visibility.Collapsed;
        }
        if(dialogue.HasChoices > 0 && dialogue.ChoicesText != null)
        {
            string[] choices = dialogue.ChoicesText.Split('/');
            for (int i = 0; i < dialogue.HasChoices && i < choices.Length && i < choiseTextBoxes.Length; i++)
            {
                var cTemp = choices[i].Split("@");
                string choiceTextTemp = cTemp[0];
                string toIdTemp = cTemp[1];
                buttonToIDDic[choiseTextBoxes[i]] = int.Parse(toIdTemp);
                choiseTextBoxes[i].Content = choiceTextTemp + " To " + toIdTemp;
                choiseTextBoxes[i].Visibility = Visibility.Visible;
            }
        }
    }
    private void NextDialogue(object sender, RoutedEventArgs e)
    {
        if (Dialogues[CurrentDialogIndex].HasChoices > 0)
        {
            MessageBox.Show("There are choices need to be selected.");
            return;
        }

        CurrentDialogIndex = Dialogues[CurrentDialogIndex].ToID;
        if(CurrentDialogIndex < Dialogues.Count)
        {
            ShowDialogue(Dialogues[CurrentDialogIndex]);
        }
        else
        {
            this.Visibility = Visibility.Collapsed;
        }
    }

    private void GoSpecificDialogue(object sender, RoutedEventArgs e)
    {
        if (buttonToIDDic.TryGetValue((Button)sender, out int toId))
        {
            CurrentDialogIndex = toId;
            if (CurrentDialogIndex < Dialogues.Count)
            {
                ShowDialogue(Dialogues[CurrentDialogIndex]);
            }
            else
            {
                MessageBox.Show("Error with dialogue index.");
            }
        }
        else
        {
            MessageBox.Show("Can't read the index from current dialogue.");
        }
    }
}