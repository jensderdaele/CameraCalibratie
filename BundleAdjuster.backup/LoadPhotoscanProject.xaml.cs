using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using BundleAdjuster.Annotations;
using PhotoscanIO;

namespace BundleAdjuster
{
    /// <summary>
    /// Interaction logic for LoadPhotoscanProject.xaml
    /// </summary>
    public partial class LoadPhotoscanProject : Window, INotifyPropertyChanged {
        private PhotoscanIO.Chunk _selectedChunk;
        private ObservableCollection<PhotoscanIO.Chunk> _chunks;

        public ObservableCollection<PhotoscanIO.Chunk> Chunks {
            get => _chunks;
            set {
                _chunks = value;
                OnPropertyChanged();
            }
        }
        public PhotoscanIO.Chunk SelectedChunk {
            get => _selectedChunk;
            set { _selectedChunk = value;
                OnPropertyChanged();
            }
        }

        public LoadPhotoscanProject(PhotoscanIO.AgisoftProject proj) {
            _chunks = new ObservableCollection<Chunk>(proj.Chunks);
            this.DataContext = this;

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = SelectedChunk != null;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var lv = sender as ListView;
            if (lv.SelectedItem is Chunk c) {
                SelectedChunk = c;
            }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Button_Click(null,null);
        }
    }
}
