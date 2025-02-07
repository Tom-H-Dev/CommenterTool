using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CodeCommenter.Commands
{
    public partial class ToolWindow1Control : UserControl
    {
        string filePath = Directory.GetCurrentDirectory() + "data.mydata";
        public ToolWindow1Control()
        {
            InitializeComponent();
            if (File.Exists(filePath))
            {
                // Read the content from the file and set it to the TextBox
                string fileContent = File.ReadAllText(filePath);
                MyCommand.ExecuteCommand(fileContent);
                ApiKeyTextBox.Text = fileContent;
                MyCommand.ChangeCommentStyle(CommentStyle.Text);
            }
            else
            {
                // Optionally, you can leave it empty or set a placeholder
                ApiKeyTextBox.Clear();
            }
        }

        private void ComboBox_SelectionChanged(object sender, EventArgs e)
        {
            MyCommand.ChangeCommentStyle(CommentStyle.Text);
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {


            string textBoxValue = ApiKeyTextBox.Text;
            // Get selected ComboBox value
            var selectedComboBoxItem = (ComboBoxItem)CommentStyle.SelectedItem;
            string comboBoxValue = selectedComboBoxItem?.Content?.ToString();

            // Create the HTTP client
            using (var httpClient = new HttpClient())
            {
                var jsonData = new
                {
                    contents = new[]
                    {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = "Can you write me back hello?"
                            }
                        }
                    }
                }
                };

                var jsonContent = JsonConvert.SerializeObject(jsonData);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={textBoxValue}", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();

                    // Parse the JSON response
                    var jsonResponse = JObject.Parse(responseBody);

                    // Extract the "text" part
                    var text = jsonResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                    // Replace the document content with the extracted text
                    if (text != null)
                    {
                        VS.MessageBox.Show("API key is correct and set!");
                        File.WriteAllText(filePath, textBoxValue);
                        MyCommand.ExecuteCommand(textBoxValue);
                    }
                    else
                    {
                        VS.MessageBox.ShowError("No text return found!");
                    }
                }
                else
                {
                    VS.MessageBox.ShowError("API key does not work or is incorrect!");
                }
            }



            // Get TextBox value
            //VS.MessageBox.Show("ToolWindow1Control", $"ComboBox Value: {comboBoxValue}, TextBox Value: {textBoxValue}");
            //VS.MessageBox.Show(textBoxValue);

            // Call a method in another command class and pass the values
        }

        private void CommentStyle_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
        private void ApiKeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ApiKeyTextBox.Text))
            {
                PlaceholderText.Visibility = Visibility.Visible;
            }
            else
            {
                PlaceholderText.Visibility = Visibility.Collapsed;
            }
        }

        private void ClearApiData(object sender, RoutedEventArgs e)
        {

            if (File.Exists(filePath))
            {
                VS.MessageBox.Show("Successfully deleted data.");
                File.Delete(filePath);
            }
            else
            {
                VS.MessageBox.ShowError("You have no saved data.");
            }
        }

        private void CommentStyle_SelectionChanged_1(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
