using HDLabelMaker.Models;
using HDLabelMaker.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace HDLabelMaker.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ConfigService _configService;
        private readonly LabelDiscoveryService _labelService;
        private PrintService _printService;

        private string _searchText = "";
        private ProductAssociation? _selectedAssociation;
        private LabelTemplate? _selectedLabel;
        private int _printCount = 1;
        private string _statusMessage = "Ready";

        public MainViewModel()
        {
            _configService = new ConfigService();
            _labelService = new LabelDiscoveryService();
            _printService = new PrintService(_configService.LoadConfiguration().PrinterSettings);

            Labels = new ObservableCollection<LabelTemplate>();
            RecentAssociations = new ObservableCollection<ProductAssociation>();

            SearchCommand = new RelayCommand(_ => ExecuteSearch());
            PrintCommand = new RelayCommand(_ => ExecutePrint(), _ => CanPrint());
            ManageAssociationsCommand = new RelayCommand(_ => ExecuteManageAssociations());
            RefreshLabelsCommand = new RelayCommand(_ => LoadLabels());
            SelectPrinterCommand = new RelayCommand(_ => ExecuteSelectPrinter());
            FeedPaperCommand = new RelayCommand(p => ExecuteFeedPaper(int.Parse((string)p!)));

            LoadLabels();
            LoadRecentAssociations();
            InitializePrinter();
        }

        public ObservableCollection<LabelTemplate> Labels { get; }
        public ObservableCollection<ProductAssociation> RecentAssociations { get; }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ExecuteSearch();
            }
        }

        public ProductAssociation? SelectedAssociation
        {
            get => _selectedAssociation;
            set
            {
                _selectedAssociation = value;
                OnPropertyChanged();
                
                if (value != null && !string.IsNullOrEmpty(value.LabelFileName))
                {
                    SelectedLabel = Labels.FirstOrDefault(l => 
                        l.FileName.Equals(value.LabelFileName, StringComparison.OrdinalIgnoreCase));
                    PrintCount = value.DefaultCount;
                }
                
                ((RelayCommand)PrintCommand).RaiseCanExecuteChanged();
            }
        }

        public LabelTemplate? SelectedLabel
        {
            get => _selectedLabel;
            set
            {
                _selectedLabel = value;
                OnPropertyChanged();
                ((RelayCommand)PrintCommand).RaiseCanExecuteChanged();
            }
        }

        public int PrintCount
        {
            get => _printCount;
            set
            {
                _printCount = Math.Max(1, value);
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

        public ICommand SearchCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand ManageAssociationsCommand { get; }
        public ICommand RefreshLabelsCommand { get; }
        public ICommand SelectPrinterCommand { get; }
        public ICommand FeedPaperCommand { get; }

        private void LoadLabels()
        {
            Labels.Clear();
            var labels = _labelService.DiscoverLabels();
            foreach (var label in labels)
            {
                Labels.Add(label);
            }

            StatusMessage = $"Loaded {Labels.Count} label template(s)";
        }

        private void LoadRecentAssociations()
        {
            RecentAssociations.Clear();
            var config = _configService.LoadConfiguration();
            var recent = config.ProductAssociations
                .OrderByDescending(a => a.LastUsed)
                .Take(10);
            
            foreach (var assoc in recent)
            {
                RecentAssociations.Add(assoc);
            }
        }

        private void ExecuteSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                SelectedAssociation = null;
                return;
            }

            var association = _configService.FindAssociation(SearchText.Trim());
            if (association != null)
            {
                SelectedAssociation = association;
                StatusMessage = $"Found: {association.ProductName} ({association.Sku})";
            }
            else
            {
                StatusMessage = $"No association found for '{SearchText}'";
            }
        }

        private bool CanPrint()
        {
            return SelectedLabel != null && SelectedLabel.IsValid;
        }

        private void ExecutePrint()
        {
            if (SelectedLabel == null) return;

            try
            {
                StatusMessage = "Printing...";
                _printService.PrintLabel(SelectedLabel, PrintCount);
                
                if (SelectedAssociation != null)
                {
                    _configService.UpdateLastUsed(SelectedAssociation.Sku);
                    LoadRecentAssociations();
                }

                StatusMessage = $"Printed {PrintCount} label(s) successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Print error: {ex.Message}";
            }
        }

        private void ExecuteManageAssociations()
        {
            var window = new Views.AssociationManagerWindow();
            window.ShowDialog();
            LoadRecentAssociations();
        }

        private void InitializePrinter()
        {
            var config = _configService.LoadConfiguration();
            var savedPort = config.PrinterSettings.Port;

            // Check if the saved port is available and working
            if (!PrinterDetectionService.TestPrinterConnection(savedPort))
            {
                // Try to auto-detect Star TSP printer
                var starPrinter = PrinterDetectionService.FindStarTspPrinter();
                if (starPrinter != null)
                {
                    config.PrinterSettings.Port = starPrinter.PortName;
                    _configService.SaveConfiguration(config);
                    _printService = new PrintService(config.PrinterSettings);
                    StatusMessage = $"Auto-detected Star TSP printer on {starPrinter.PortName}";
                }
                else
                {
                    StatusMessage = $"Printer not found on {savedPort}. Click 'Select Printer' to choose a printer.";
                }
            }
            else
            {
                StatusMessage = $"Printer ready on {savedPort}";
            }
        }

        private void ExecuteFeedPaper(int mm)
        {
            try
            {
                _printService.FeedPaper(mm);
                StatusMessage = $"Fed paper {mm}mm";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Feed error: {ex.Message}";
            }
        }

        private void ExecuteSelectPrinter()
        {
            var dialog = new Views.PrinterSelectionDialog();
            dialog.Owner = System.Windows.Application.Current.MainWindow;

            if (dialog.ShowDialog() == true && dialog.SelectedPortName != null)
            {
                var config = _configService.LoadConfiguration();
                config.PrinterSettings.Port = dialog.SelectedPortName;
                _configService.SaveConfiguration(config);
                _printService = new PrintService(config.PrinterSettings);
                StatusMessage = $"Printer selected: {dialog.SelectedPortName}";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
