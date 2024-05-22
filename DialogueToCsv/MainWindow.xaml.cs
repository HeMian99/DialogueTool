using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CsvHelper;
using Microsoft.Win32;

namespace DialogueToCsv;
public partial class MainWindow : Window
{
    private TestDialogue dialogueWindow;

    public MainWindow()
    {
        InitializeComponent();
        var source = new ObservableCollection<DialogueDataClass>
        {
            new DialogueDataClass
                { ID = 0, DialogueText = "Edit your first dialogue text here.", ToID = 1, HasChoices = 0 }
        };
        DialogueGrid.ItemsSource = source;
    }
    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }

    private void UpdateIds()
    {
        ObservableCollection<DialogueDataClass> dataCollection =
            (ObservableCollection<DialogueDataClass>)DialogueGrid.ItemsSource;

        for (int i = 0; i < dataCollection.Count; i++)
        {
            dataCollection[i].ID = i;
        }
    }
    private void AddRow(object sender, RoutedEventArgs e)
    {
        ObservableCollection<DialogueDataClass> dataCollection =
            (ObservableCollection<DialogueDataClass>)DialogueGrid.ItemsSource;

        DialogueDataClass newRow = new DialogueDataClass();
        int index = DialogueGrid.SelectedIndex;
        if (index != -1)
        {
            newRow.ID = index + 1;
            newRow.ToID = newRow.ID + 1;
            dataCollection.Insert(index + 1, newRow);

            DialogueGrid.SelectedItem = newRow; 
        }
        else
        {
            newRow.ID = dataCollection.Count;
            newRow.ToID = newRow.ID + 1;
            dataCollection.Add(newRow);
        }

        UpdateIds(); 
    }

    private void DeleteRow(object sender, RoutedEventArgs e)
    {
        if (DialogueGrid.SelectedItems.Count > 0)
        {
            ObservableCollection<DialogueDataClass> dataCollection =
                (ObservableCollection<DialogueDataClass>)DialogueGrid.ItemsSource;

            for (int i = DialogueGrid.SelectedItems.Count - 1; i >= 0; i--)
            {
                DialogueDataClass selectedRow = (DialogueDataClass)DialogueGrid.SelectedItems[i];
                if (selectedRow != null) dataCollection.Remove(selectedRow);
            }
        }
        else
        {
            // Display an error
        }

        UpdateIds();
    }

    private void BtnExportCsv_Click(object sender, RoutedEventArgs e)
    {
        ObservableCollection<DialogueDataClass> dialogues =
            DialogueGrid.ItemsSource as ObservableCollection<DialogueDataClass>;

        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "CSV file (*.csv)|*.csv";
        if (saveFileDialog.ShowDialog() == true)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to save the file as UTF-8?", "Save As UTF-8",
                MessageBoxButton.YesNo);
            Encoding encoding = result == MessageBoxResult.Yes ? Encoding.UTF8 : Encoding.Default;
            using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName, false, encoding))
            {
                ConvertDataToCsv<DialogueDataClass>(dialogues, writer);
            }
        }
    }

    public void ConvertDataToCsv<T>(IEnumerable<T> dialogues, StreamWriter writer)
    {
        foreach (var dialogue in dialogues)
        {
            var id = typeof(T).GetProperty("ID")?.GetValue(dialogue, null);
            var name = typeof(T).GetProperty("Name")?.GetValue(dialogue, null);
            var dialogueText = typeof(T).GetProperty("DialogueText")?.GetValue(dialogue, null);
            var toId = typeof(T).GetProperty("ToID")?.GetValue(dialogue, null);
            var hasChoices = typeof(T).GetProperty("HasChoices")?.GetValue(dialogue, null);
            var choicesText = typeof(T).GetProperty("ChoicesText")?.GetValue(dialogue, null);

            string line = $"{id}&{name}&{dialogueText}&{toId}&{hasChoices}&{choicesText}";
            writer.WriteLine(line);
        }
    }

    private void BtnImportCsv_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "CSV file (*.csv)|*.csv";
        if (openFileDialog.ShowDialog() == true)
        {
            var dialogues = new ObservableCollection<DialogueDataClass>();
            try
            {
                using (StreamReader reader = new StreamReader(openFileDialog.FileName, new UTF8Encoding(false, true)))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var values = line.Split('&');

                        var dialogue = new DialogueDataClass
                        {
                            ID = Int32.Parse(values[0]),
                            Name = values[1],
                            DialogueText = values[2],
                            ToID = Int32.Parse(values[3]),
                            HasChoices = Int32.Parse(values[4]),
                            ChoicesText = values[5]
                        };

                        dialogues.Add(dialogue);
                    }
                }

                DialogueGrid.ItemsSource = dialogues;
            }
            catch (DecoderFallbackException ex)
            {
                MessageBox.Show("The file is not in UTF-8 format", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void DialogueGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.Column.Header.ToString() == "Choice?")
        {
            TextBox txtBox = e.EditingElement as TextBox;
            var dialogueData = e.Row.Item as DialogueDataClass;

            if (txtBox != null && dialogueData != null)
            {
                int number;
                bool parsed = Int32.TryParse(txtBox.Text, out number);
                if (!parsed || number < 0 || number > 7)
                {
                    MessageBox.Show("The value input should be 0-7.");
                    e.Cancel = true;
                }
            }
        }

        if (e.Column.Header.ToString() == "Choices Text")
        {
            TextBox txtBox = e.EditingElement as TextBox;
            var dialogueData = e.Row.Item as DialogueDataClass;

            if (txtBox == null && dialogueData == null) return;
            if (dialogueData.HasChoices == 0) return;
            if (dialogueData.HasChoices != txtBox.Text.Split('/').Length)
            {
                MessageBox.Show($"Number of Choices error, current choices text is {txtBox.Text.Split('/').Length}");
                e.Cancel = true;
            }
        }
    }

    private void TestDialogue(object sender, RoutedEventArgs e)
    {
        if (dialogueWindow != null && dialogueWindow.IsVisible)
        {
            return;
        }

        ObservableCollection<DialogueDataClass> dialogues =
            (ObservableCollection<DialogueDataClass>)DialogueGrid.ItemsSource;

        dialogueWindow = new TestDialogue(dialogues);
        var mainWindow = Application.Current.MainWindow;
        double mainWindowRight = mainWindow.Left + mainWindow.Width;
        dialogueWindow.Left = mainWindowRight;
        dialogueWindow.Top = mainWindow.Top;

        dialogueWindow.Show();
    }
}