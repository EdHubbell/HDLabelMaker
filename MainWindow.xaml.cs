using HDLabelMaker.ViewModels;
using System.Windows;

namespace HDLabelMaker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void IncrementCount(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.PrintCount++;
            }
        }

        private void DecrementCount(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.PrintCount > 1)
            {
                vm.PrintCount--;
            }
        }
    }
}