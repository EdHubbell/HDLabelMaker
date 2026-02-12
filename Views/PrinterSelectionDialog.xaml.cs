using HDLabelMaker.Services;
using HDLabelMaker.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace HDLabelMaker.Views
{
    public partial class PrinterSelectionDialog : Window, INotifyPropertyChanged
    {
        private ObservableCollection<PrinterInfo> _printers = new();
        private PrinterInfo? _selectedPrinter;
        private string _statusMessage = "";

        public PrinterSelectionDialog()
        {
            InitializeComponent();
            DataContext = this;
            LoadPrinters();
        }

        public ObservableCollection<PrinterInfo> Printers
        {
            get => _printers;
            set
            {
                _printers = value;
                OnPropertyChanged();
            }
        }

        public PrinterInfo? SelectedPrinter
        {
            get => _selectedPrinter;
            set
            {
                _selectedPrinter = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public string? SelectedPortName => SelectedPrinter?.PortName;

        private void LoadPrinters()
        {
            Printers.Clear();
            var printers = PrinterDetectionService.DetectAvailablePrinters();
            foreach (var printer in printers)
            {
                Printers.Add(printer);
            }

            // Auto-select first Star TSP printer if available
            SelectedPrinter = Printers.FirstOrDefault(p => p.IsStarTsp) ?? Printers.FirstOrDefault();

            if (SelectedPrinter != null)
            {
                StatusMessage = $"Found {Printers.Count} printer(s). " +
                    $"{(SelectedPrinter.IsStarTsp ? "Star TSP printer auto-selected." : "Please select a printer.")}";
            }
            else
            {
                StatusMessage = "No printers found. Please connect a printer and refresh.";
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPrinters();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPrinter == null)
            {
                MessageBox.Show("Please select a printer first.", "No Printer Selected", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            StatusMessage = "Testing connection...";
            bool success = PrinterDetectionService.TestPrinterConnection(SelectedPrinter.PortName);
            
            if (success)
            {
                StatusMessage = $"Connection successful on {SelectedPrinter.PortName}";
                MessageBox.Show($"Successfully connected to printer on port {SelectedPrinter.PortName}",
                    "Connection Test", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = $"Connection failed on {SelectedPrinter.PortName}";
                MessageBox.Show($"Failed to connect to printer on port {SelectedPrinter.PortName}.\n\n" +
                    "Make sure the printer is powered on and connected.",
                    "Connection Test", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPrinter == null)
            {
                MessageBox.Show("Please select a printer.", "No Selection", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
