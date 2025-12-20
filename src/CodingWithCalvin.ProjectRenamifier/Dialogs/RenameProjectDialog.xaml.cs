using System.IO;
using System.Windows;

namespace CodingWithCalvin.ProjectRenamifier.Dialogs
{
    public partial class RenameProjectDialog : Window
    {
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
        private readonly string _originalName;

        public string NewProjectName => ProjectNameTextBox.Text.Trim();

        public RenameProjectDialog(string currentProjectName)
        {
            InitializeComponent();
            _originalName = currentProjectName;
            ProjectNameTextBox.Text = currentProjectName;
            ProjectNameTextBox.SelectAll();
            ValidateInput();
        }

        private void ProjectNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ValidateInput();
        }

        private void ValidateInput()
        {
            var validationError = GetValidationError();
            if (string.IsNullOrEmpty(validationError))
            {
                ErrorTextBlock.Visibility = Visibility.Collapsed;
                OkButton.IsEnabled = true;
            }
            else
            {
                ErrorTextBlock.Text = validationError;
                ErrorTextBlock.Visibility = Visibility.Visible;
                OkButton.IsEnabled = false;
            }
        }

        private string GetValidationError()
        {
            var name = ProjectNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                return "Project name cannot be empty.";
            }

            if (name.Equals(_originalName, System.StringComparison.OrdinalIgnoreCase))
            {
                return "New name must be different from the current name.";
            }

            foreach (var c in name)
            {
                if (System.Array.IndexOf(InvalidFileNameChars, c) >= 0)
                {
                    return $"Project name cannot contain '{c}' character.";
                }
            }

            return null;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
