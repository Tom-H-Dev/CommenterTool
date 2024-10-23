using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CodeCommenter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Command(PackageIds.MyCommand)]
internal sealed class MyCommand : BaseCommand<MyCommand>
{
    static string summaryPrompt = "Can you write summaries (/// in c#) above functions and variables and classes and dont change the code, keep it the same";
    static string regularPrompt = "Can you write comments for my code and dont change the code, keep it the same";
    
    static string basicPrompt;
    string ownApiKey = "AIzaSyAnG15-I6130dQ3Xzxz5bENyKMfCKbMccg";

    static string ComboBoxValue;
    static string APIKeyValue;

    public static void ExecuteCommand(string comboBoxValue, string textBoxValue)
    {
        // Process the ComboBox and TextBox values
        // Example:
        //System.Diagnostics.Debug.WriteLine($"ComboBox Value: {comboBoxValue}, TextBox Value: {textBoxValue}");

        APIKeyValue = textBoxValue;

        switch (comboBoxValue)
        {
            case "":
                basicPrompt = summaryPrompt;
                break;
            case "2":
                basicPrompt = regularPrompt;
                break;
        }
        ComboBoxValue = comboBoxValue;

        // Add your logic to handle the data here
    }

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        VS.MessageBox.Show("Debug controll input value screen", $"ComboBox Value: {ComboBoxValue}, TextBox Value: {APIKeyValue}");

        // When the event is called
        var docView = await VS.Documents.GetActiveDocumentViewAsync();
        var selection = docView?.TextBuffer.CurrentSnapshot.GetText();

        if (selection == null) return;

        var fullspan = new Microsoft.VisualStudio.Text.Span(0, selection.Length);

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
                                text = summaryPrompt + "\n" + selection
                            }
                        }
                    }
                }
            };

            var jsonContent = JsonConvert.SerializeObject(jsonData);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={ownApiKey}", httpContent);

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
                    docView.TextBuffer.Replace(fullspan, text);
                }
                else
                {
                    docView.TextBuffer.Replace(fullspan, "Text not found in the response.");
                }
            }
            else
            {
                docView.TextBuffer.Replace(fullspan, "Error " + response.StatusCode);
            }
        }
    }
}

