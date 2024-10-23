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
using CodeCommenter.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Command(PackageIds.CodeCommenterSettings)]
internal sealed class CodeCommenterSettings : BaseCommand<CodeCommenterSettings>
{
    protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
        return ToolWindow1.ShowAsync();
    }
}

