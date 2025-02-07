using System.Net.Http;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using CodeCommenter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Command(PackageIds.MyCommand)]
internal sealed class MyCommand : BaseCommand<MyCommand>
{
    static string summaryPrompt = "Add documentation comments to all classes, methods, properties, and variables in the following code. Use the appropriate syntax for the given programming language (e.g., /// for C#). Keep all existing code untouched, and do not add any additional text outside of the original code and documentation.";
    static string regularPrompt = "Add inline comments to explain the functionality of all classes, methods, properties, and variables in the following code. Use the appropriate comment syntax for the given programming language (e.g., // for C#). Keep all existing code untouched, and do not add any additional text outside of the original code and comments.";
    static string combinedPrompt = "Add documentation comments to all classes, methods, properties, and variables using the appropriate syntax for the given programming language (e.g., /// for C#). Additionally, add inline comments within method bodies to explain the code logic using the correct syntax (e.g., // for C#). Keep all existing code untouched, and do not add any additional text outside of the original code and comments.";

    static string basicPrompt;

    static string ComboBoxValue;
    static string APIKeyValue;

    public static void ExecuteCommand(string textBoxValue)
    {
        APIKeyValue = textBoxValue;
        
    }

    public static void ChangeCommentStyle(string comboBoxValue)
    {
        switch (comboBoxValue)
        {
            case "Function Summaries":
                basicPrompt = summaryPrompt;
                break;
            case "Regular Comments":
                basicPrompt = regularPrompt;
                break;
            case "Comments and Summaries":
                basicPrompt = combinedPrompt;
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
                                    text = basicPrompt + "\n" + selection
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