using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;

namespace PomodoroApp
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        
        public class Task
        {
            public string Name { get; set; }
            public string Project { get; set; }
            public int EstimatedPomodoros { get; set; }
            public int CompletedPomodoros { get; set; }
            public bool IsCompleted { get; set; }
            public TimeSpan TimeSpent { get; set; } 
        }

       
        public class Project
        {
            public string Name { get; set; }
            public TimeSpan TotalTime { get; set; }
            public decimal HourlyRate { get; set; }

            public decimal TotalEarnings => (decimal)TotalTime.TotalHours * HourlyRate;
        }

       
        public ObservableCollection<Task> Tasks { get; set; }
        public ObservableCollection<Project> Projects { get; set; }

        
        private Timer _timer;
        private int _remainingSeconds;
        private bool _isTimerRunning;
        private Task _activeTask; 

        public event PropertyChangedEventHandler PropertyChanged;

        public string TimerText => $"{_remainingSeconds / 60:D2}:{_remainingSeconds % 60:D2}";

        public MainWindow()
        {
            InitializeComponent();

            // 
            Tasks = new ObservableCollection<Task>
            {
                new Task { Name = "Zadanie 1", Project = "Projekt 1", EstimatedPomodoros = 4, CompletedPomodoros = 1, TimeSpent = TimeSpan.Zero },
                new Task { Name = "Zadanie 2", Project = "Projekt 2", EstimatedPomodoros = 3, CompletedPomodoros = 0, TimeSpent = TimeSpan.Zero }
            };

            Projects = new ObservableCollection<Project>
            {
                new Project { Name = "Projekt 1", TotalTime = TimeSpan.Zero, HourlyRate = 50 },
                new Project { Name = "Projekt 2", TotalTime = TimeSpan.Zero, HourlyRate = 75 }
            };

            TaskListBox.ItemsSource = Tasks;

            
            _remainingSeconds = 25 * 60; 
            _timer = new Timer(1000);
            _timer.Elapsed += Timer_Elapsed;

            
            TimerTextBlock.Text = TimerText;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_remainingSeconds > 0)
            {
                _remainingSeconds--;
                Dispatcher.Invoke(() =>
                {
                    OnPropertyChanged(nameof(TimerText));
                    TimerTextBlock.Text = TimerText;
                });

                
                if (_activeTask != null)
                {
                    _activeTask.TimeSpent += TimeSpan.FromSeconds(1);
                }
            }
            else
            {
                _timer.Stop();
                _isTimerRunning = false;
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Pomodoro sesja ukończona!");
                });
            }
        }

        private void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            if (!_isTimerRunning && TaskListBox.SelectedItem is Task selectedTask)
            {
                _isTimerRunning = true;
                _timer.Start();
                _activeTask = selectedTask;
            }
        }

        private void PauseTimer_Click(object sender, RoutedEventArgs e)
        {
            if (_isTimerRunning)
            {
                _isTimerRunning = false;
                _timer.Stop();
            }
        }

        private void ResetTimer_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _isTimerRunning = false;
            _remainingSeconds = 25 * 60;
            TimerTextBlock.Text = TimerText;
            _activeTask = null;
        }

        private void TaskListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (TaskListBox.SelectedItem is Task selectedTask)
            {
                MessageBox.Show($"Wybrane zadanie: {selectedTask.Name}");
            }
        }

        
        private void ExportToCsv_Click(object sender, RoutedEventArgs e)
        {
            
            foreach (var project in Projects)
            {
                project.TotalTime = Tasks
                    .Where(t => t.Project == project.Name)
                    .Aggregate(TimeSpan.Zero, (sum, task) => sum + task.TimeSpent);
            }

            var csvContent = new StringBuilder();
            csvContent.AppendLine("PProjekt, Całkowity czas (godziny), Stawka godzinowa, Całkowite zarobki");

            foreach (var project in Projects)
            {
                csvContent.AppendLine($"{project.Name},{project.TotalTime.TotalHours:F2},{project.HourlyRate},{project.TotalEarnings:F2}");
            }

            string filePath = "ProjektRaport.csv";
            File.WriteAllText(filePath, csvContent.ToString());

            MessageBox.Show($"Raport wyeksportowany do {filePath}");
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
