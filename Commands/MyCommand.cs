//using System.Linq;

//namespace CodeCommenter
//{
//    [Command(PackageIds.MyCommand)]
//    internal sealed class MyCommand : BaseCommand<MyCommand>
//    {
//        string AIPrompt = "Can you comment all the code listed below, comment the fucntions and " +
//        "variables with summaries and write some normal comments in functions where the code is a bit more difficult to understand.";

//        string ownApiKey = "AIzaSyAnG15-I6130dQ3Xzxz5bENyKMfCKbMccg";
//        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
//        {

//            //When the event is called
//            var docView = await VS.Documents.GetActiveDocumentViewAsync();
//            var selection = docView?.TextBuffer.CurrentSnapshot.GetText();

//            var guid = Guid.NewGuid().ToString();

//            var fullspan = new Microsoft.VisualStudio.Text.Span(0, selection.Length);
//            docView.TextBuffer.Replace(fullspan, guid + "\n" + selection);
//        }
//    }
//}

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CodeCommenter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Command(PackageIds.MyCommand)]
internal sealed class MyCommand : BaseCommand<MyCommand>
{
    string AIPrompt = "Can you write comments (///) above functions, variables and classes";

    string ownApiKey = "AIzaSyAnG15-I6130dQ3Xzxz5bENyKMfCKbMccg";

    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
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
                                text = AIPrompt + "\n" + selection
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

