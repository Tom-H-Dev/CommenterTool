using System.Windows;
using System.Windows.Controls;

namespace CodeCommenter.Commands
{
    public partial class ToolWindow1Control : UserControl
    {
        public ToolWindow1Control()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {


            // Get selected ComboBox value
            var selectedComboBoxItem = (ComboBoxItem)CommentStyle.SelectedItem;
            string comboBoxValue = selectedComboBoxItem?.Content?.ToString();

            // Get TextBox value
            string textBoxValue = APITextInput.Text;
            //VS.MessageBox.Show("ToolWindow1Control", $"ComboBox Value: {comboBoxValue}, TextBox Value: {textBoxValue}");


            // Call a method in another command class and pass the values
            MyCommand.ExecuteCommand(comboBoxValue, textBoxValue);
        }

        private void CommentStyle_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}
