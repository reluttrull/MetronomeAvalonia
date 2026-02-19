using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using MetronomeMVVM.ViewModels;
using System;

namespace MetronomeMVVM.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void OnBpmClicked(object sender, PointerPressedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.TempBpm = string.Empty;
                viewModel.IsEditingBpm = true;
                this.FindControl<TextBox>("EditBpmBox")?.Focus();
            }
        }

        private void OnBpmLostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                if (!int.TryParse(viewModel.TempBpm, out int newBpm))
                {
                    viewModel.TempBpm = string.Empty;
                    viewModel.IsEditingBpm = false;
                    return;
                }
                viewModel.SetBpm(newBpm);
                viewModel.IsEditingBpm = false;
            }
        }
    }
}