using System.Net.Http;
using System.Text;
using System.Windows.Input;
using CodeCommenter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Command(PackageIds.MyCommand)]
internal sealed class MyCommand : BaseCommand<MyCommand>
{
    static string summaryPrompt = "Can you write summaries (/// in c#) above functions and variables and classes and dont change the code, keep it the same";
    static string regularPrompt = "Can you write comments for my code and dont change the code, keep it the same";

    static string basicPrompt;

    static string ComboBoxValue;
    static string APIKeyValue;

    public static void ExecuteCommand(string comboBoxValue, string textBoxValue)
    {
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
    }

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        if (APIKeyValue is null)
        {
            VS.MessageBox.ShowError("No API Key found");
            return;
        }

        // Set loading cursor
        Mouse.OverrideCursor = Cursors.Wait;

        try
        {
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

                var response = await httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent?key={APIKeyValue}", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JObject.Parse(responseBody);
                    var text = jsonResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                    if (text != null)
                    {
                        text = text.Trim();

                        if (text.StartsWith("```csharp"))
                        {
                            text = text.Substring(9).Trim();
                        }

                        if (text.EndsWith("```"))
                        {
                            text = text.Substring(0, text.Length - 3).Trim();
                        }
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
        catch (Exception ex)
        {
            VS.MessageBox.ShowError($"An error occurred: {ex.Message}");
        }
        finally
        {
            // Reset cursor back to normal
            Mouse.OverrideCursor = null;
        }
    }
}