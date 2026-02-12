using HDLabelMaker.Models;
using HDLabelMaker.Services;
using HDLabelMaker.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace HDLabelMaker.Views
{
    public partial class AssociationManagerWindow : Window, INotifyPropertyChanged
    {
        private readonly ConfigService _configService;
        private readonly LabelDiscoveryService _labelService;
        private ObservableCollection<ProductAssociation> _associations;
        private ProductAssociation? _selectedAssociation;
        private ObservableCollection<LabelTemplate> _availableLabels;

        private string _sku = "";
        private string _barcode = "";
        private string _productName = "";
        private LabelTemplate? _selectedLabel;
        private int _defaultCount = 1;

        public AssociationManagerWindow()
        {
            InitializeComponent();
            DataContext = this;

            _configService = new ConfigService();
            _labelService = new LabelDiscoveryService();
            _associations = new ObservableCollection<ProductAssociation>();
            _availableLabels = new ObservableCollection<LabelTemplate>();

            SaveCommand = new RelayCommand(_ => ExecuteSave(), _ => CanSave());
            DeleteCommand = new RelayCommand(_ => ExecuteDelete(), _ => CanDelete());
            ClearCommand = new RelayCommand(_ => ExecuteClear());

            LoadAssociations();
            LoadLabels();
        }

        public ObservableCollection<ProductAssociation> Associations
        {
            get => _associations;
            set
            {
                _associations = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<LabelTemplate> AvailableLabels
        {
            get => _availableLabels;
            set
            {
                _availableLabels = value;
                OnPropertyChanged();
            }
        }

        public ProductAssociation? SelectedAssociation
        {
            get => _selectedAssociation;
            set
            {
                _selectedAssociation = value;
                OnPropertyChanged();
                
                if (value != null)
                {
                    Sku = value.Sku;
                    Barcode = value.Barcode;
                    ProductName = value.ProductName;
                    DefaultCount = value.DefaultCount;
                    SelectedLabel = AvailableLabels.FirstOrDefault(l => 
                        l.FileName.Equals(value.LabelFileName, StringComparison.OrdinalIgnoreCase));
                }
                
                ((RelayCommand)DeleteCommand).RaiseCanExecuteChanged();
            }
        }

        public string Sku
        {
            get => _sku;
            set
            {
                _sku = value;
                OnPropertyChanged();
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public string Barcode
        {
            get => _barcode;
            set
            {
                _barcode = value;
                OnPropertyChanged();
            }
        }

        public string ProductName
        {
            get => _productName;
            set
            {
                _productName = value;
                OnPropertyChanged();
            }
        }

        public LabelTemplate? SelectedLabel
        {
            get => _selectedLabel;
            set
            {
                _selectedLabel = value;
                OnPropertyChanged();
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }

        public int DefaultCount
        {
            get => _defaultCount;
            set
            {
                _defaultCount = Math.Max(1, value);
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }

        private void LoadAssociations()
        {
            Associations.Clear();
            var config = _configService.LoadConfiguration();
            foreach (var assoc in config.ProductAssociations.OrderBy(a => a.ProductName))
            {
                Associations.Add(assoc);
            }
        }

        private void LoadLabels()
        {
            AvailableLabels.Clear();
            var labels = _labelService.DiscoverLabels();
            foreach (var label in labels)
            {
                AvailableLabels.Add(label);
            }
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Sku) && SelectedLabel != null;
        }

        private void ExecuteSave()
        {
            var association = new ProductAssociation
            {
                Sku = Sku.Trim(),
                Barcode = Barcode.Trim(),
                ProductName = ProductName.Trim(),
                LabelFileName = SelectedLabel?.FileName ?? "",
                DefaultCount = DefaultCount,
                LastUsed = DateTime.Now
            };

            _configService.AddOrUpdateAssociation(association);
            LoadAssociations();
            ExecuteClear();
        }

        private bool CanDelete()
        {
            return SelectedAssociation != null;
        }

        private void ExecuteDelete()
        {
            if (SelectedAssociation == null) return;

            var result = MessageBox.Show(
                $"Delete association for {SelectedAssociation.ProductName}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var config = _configService.LoadConfiguration();
                config.ProductAssociations.Remove(SelectedAssociation);
                _configService.SaveConfiguration(config);
                LoadAssociations();
                ExecuteClear();
            }
        }

        private void ExecuteClear()
        {
            Sku = "";
            Barcode = "";
            ProductName = "";
            SelectedLabel = null;
            DefaultCount = 1;
            SelectedAssociation = null;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
