using Avalonia.Controls;
using Practika2.Services;

namespace Practika2.Views
{
    public partial class TestStatisticsView : Window
    {
        public TestStatisticsView(TestStatistics stats)
        {
            InitializeComponent();
            
            StatsPanel.Children.Add(new Avalonia.Controls.TextBlock 
            { 
                Text = $"Всего прохождений: {stats.TotalSubmissions}",
                Margin = new Avalonia.Thickness(0, 0, 0, 8)
            });
            
            StatsPanel.Children.Add(new Avalonia.Controls.TextBlock 
            { 
                Text = $"Средний балл: {stats.AverageScore:F1}%",
                Margin = new Avalonia.Thickness(0, 0, 0, 8)
            });
            
            StatsPanel.Children.Add(new Avalonia.Controls.TextBlock 
            { 
                Text = $"Прошли успешно: {stats.PassedCount}",
                Margin = new Avalonia.Thickness(0, 0, 0, 8)
            });
        }
    }
}



