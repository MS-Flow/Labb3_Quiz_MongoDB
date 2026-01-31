using Labb3_Quiz.ViewModels;
using System.Windows;

namespace Labb3_Quiz;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var viewModel = new MainWindowViewModel();
        DataContext = viewModel;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.IsFullScreen))
                UpdateFullScreen(viewModel.IsFullScreen);
        };
    }

    private void UpdateFullScreen(bool isFullScreen)
    {
        if (isFullScreen)
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
        }
        else
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Normal;
        }
    }
}

