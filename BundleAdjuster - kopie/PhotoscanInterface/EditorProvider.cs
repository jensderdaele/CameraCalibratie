using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BundleAdjuster.PhotoscanInterface;
using BundleAdjuster.PhotoscanInterface.ViewModels;
using BundleAdjuster.Properties;
using Emgu.CV.OCR;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace BundleAdjuster {
    [Export(typeof(IEditorProvider))]
    public class EditorProvider : IEditorProvider {
        private readonly List<string> _extensions = new List<string>
        {
            ".psx"
        };


        public bool Handles(string path) {
            var extension = Path.GetExtension(path);
            return _extensions.Contains(extension);
        }

        public IDocument Create() {
            return new PhotoscanInterfaceViewModel();
        }

        public async Task New(IDocument document, string name) {
            await ((PhotoscanInterfaceViewModel)document).New(name);
        }

        public async Task Open(IDocument document, string path) {
            await ((PhotoscanInterfaceViewModel)document).Load(path);
        }

        public IEnumerable<EditorFileType> FileTypes {
            get { yield return new EditorFileType(Resources.EditorProviderPhotoscanFile, ".psx"); }
        }
    }
}
