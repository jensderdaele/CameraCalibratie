using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemini.Framework;
using Microsoft.Win32;
using PhotoscanIO;

namespace BundleAdjuster.PhotoscanInterface.ViewModels
{
    class PhotoscanInterfaceViewModel : PersistedDocument
    {
        protected override Task DoNew() {
            throw new NotImplementedException();
        }

        protected override async Task DoLoad(string filePath) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Agisoft Photoscan files (*.psx)|*.psx|All files (*.*)|*.*";

            var filename = filePath;
            var Project = new AgisoftProject(filename);
            LoadPhotoscanProject chunkdialog = new LoadPhotoscanProject(Project);

            if (chunkdialog.ShowDialog() == true) {
                BP.LoadPhotoscanChunk(chunkdialog.SelectedChunk);
            }
        }

        protected override Task DoSave(string filePath) {
            throw new NotImplementedException();
        }
    }
}
