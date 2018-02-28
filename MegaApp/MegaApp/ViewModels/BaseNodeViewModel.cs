using System;
using System.Collections.ObjectModel;
using System.IO;
using Windows.Storage;
using mega;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    public abstract class BaseNodeViewModel : BaseSdkViewModel, IBaseNode
    {
        protected BaseNodeViewModel(MegaSDK megaSdk) : base(megaSdk)
        {

        }

        #region IBaseNode Interface

        public string Base64Handle { get; set; }

        public ObservableCollection<IBaseNode> ChildCollection { get; set; }

        public string CreationTime { get; set; }

        private string _defaultImagePathData;
        public string DefaultImagePathData
        {
            get { return _defaultImagePathData; }
            set { SetField(ref _defaultImagePathData, value); }
        }

        private NodeDisplayMode _displayMode;
        public NodeDisplayMode DisplayMode
        {
            get { return _displayMode; }
            set { SetField(ref _displayMode, value); }
        }

        public virtual bool IsFolder { get; set; }

        public bool IsImage => ImageService.IsImage(this.Name);

        private bool _isMultiSelected;
        public bool IsMultiSelected
        {
            get { return _isMultiSelected; }
            set { SetField(ref _isMultiSelected, value); }
        }

        public string ModificationTime { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                SetField(ref _name, value);
                if (!this.IsFolder)
                    this.DefaultImagePathData = ImageService.GetDefaultFileTypePathData(this.Name);
            }
        }

        public ObservableCollection<IBaseNode> ParentCollection { get; set; }

        public ulong Size { get; set; }

        private Uri _thumbnailImageUri;
        public Uri ThumbnailImageUri
        {
            get { return _thumbnailImageUri; }
            set { SetField(ref _thumbnailImageUri, value); }
        }

        public string ThumbnailPath
        {
            get
            {
                return Path.Combine(ApplicationData.Current.LocalFolder.Path,
                    ResourceService.AppResources.GetString("AR_ThumbnailsDirectory"), this.Base64Handle);
            }
        }

        #endregion

        #region Properties

        private string _information;
        public string Information
        {
            get { return _information; }
            set { SetField(ref _information, value); }
        }

        private bool _IsDefaultImage;
        public bool IsDefaultImage
        {
            get { return _IsDefaultImage; }
            set { SetField(ref _IsDefaultImage, value); }
        }

        private string _sizeText;
        public string SizeText
        {
            get { return _sizeText; }
            set { SetField(ref _sizeText, value); }
        }

        #endregion
    }
}
