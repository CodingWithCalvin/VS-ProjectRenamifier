using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace CodingWithCalvin.ProjectRenamifier.Dialogs
{
    public partial class RenameProgressDialog : Window
    {
        public ObservableCollection<ProgressStep> Steps { get; }

        public RenameProgressDialog(string projectName)
        {
            InitializeComponent();

            HeaderText.Text = $"Renaming '{projectName}'...";

            Steps = new ObservableCollection<ProgressStep>
            {
                new ProgressStep("Analyzing project references"),
                new ProgressStep("Capturing solution structure"),
                new ProgressStep("Removing project from solution"),
                new ProgressStep("Updating project file"),
                new ProgressStep("Updating namespace declarations"),
                new ProgressStep("Renaming project file"),
                new ProgressStep("Renaming project directory"),
                new ProgressStep("Updating project references"),
                new ProgressStep("Re-adding project to solution"),
                new ProgressStep("Updating using statements"),
                new ProgressStep("Updating fully qualified references")
            };

            StepsList.ItemsSource = Steps;
        }

        public void StartStep(int stepIndex)
        {
            if (stepIndex >= 0 && stepIndex < Steps.Count)
            {
                Steps[stepIndex].Status = StepStatus.InProgress;
                UpdateProgress(stepIndex);
            }
        }

        public void CompleteStep(int stepIndex)
        {
            if (stepIndex >= 0 && stepIndex < Steps.Count)
            {
                Steps[stepIndex].Status = StepStatus.Completed;
                UpdateProgress(stepIndex + 1);
            }
        }

        public void FailStep(int stepIndex, string error)
        {
            if (stepIndex >= 0 && stepIndex < Steps.Count)
            {
                Steps[stepIndex].Status = StepStatus.Failed;
                Steps[stepIndex].Description += $" - {error}";
            }
        }

        public void Complete()
        {
            HeaderText.Text = "Rename completed successfully!";
            ProgressBar.Value = 100;
        }

        private void UpdateProgress(int completedSteps)
        {
            ProgressBar.Value = (completedSteps * 100.0) / Steps.Count;
        }
    }

    public enum StepStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed
    }

    public class ProgressStep : INotifyPropertyChanged
    {
        private string _description;
        private StepStatus _status;

        public ProgressStep(string description)
        {
            _description = description;
            _status = StepStatus.Pending;
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public StepStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(StatusIcon));
                OnPropertyChanged(nameof(TextColor));
            }
        }

        public string StatusIcon => _status switch
        {
            StepStatus.Pending => "\u2022",      // Bullet
            StepStatus.InProgress => "\u25B6",   // Play arrow
            StepStatus.Completed => "\u2714",    // Check mark
            StepStatus.Failed => "\u2716",       // X mark
            _ => "\u2022"
        };

        public Brush TextColor => _status switch
        {
            StepStatus.Pending => Brushes.Gray,
            StepStatus.InProgress => Brushes.Black,
            StepStatus.Completed => Brushes.Green,
            StepStatus.Failed => Brushes.Red,
            _ => Brushes.Gray
        };

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
