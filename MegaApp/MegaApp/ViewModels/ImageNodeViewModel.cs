using System;
using System.Collections.ObjectModel;
using System.IO;
using Windows.Storage;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Interfaces;
using MegaApp.Services;

namespace MegaApp.ViewModels
{
    class ImageNodeViewModel: FileNodeViewModel
    {
        public ImageNodeViewModel(MegaSDK megaSdk, AppInformation appInformation, MNode megaNode, ContainerType parentContainerType,
            ObservableCollection<IMegaNode> parentCollection = null, ObservableCollection<IMegaNode> childCollection = null)
            : base(megaSdk, appInformation, megaNode, parentContainerType, parentCollection, childCollection)
        {
            // Image node downloads to the image path of the full original image
            Transfer = new TransferObjectModel(this, TransferType.Download, LocalDownloadPath);

            DefaultImagePathData = ImageService.GetDefaultFileTypePathData(Name);

            // Default false for preview slide
            InViewingRange = false;
        }

        #region Override Methods

        public override async void Open()
        {
            await FileService.OpenFile(LocalDownloadPath);
        }

        #endregion

        #region Public Methods

        public bool HasPreviewInCache()
        {
            return FileService.FileExists(PreviewPath);
        }

        public void CancelPreviewRequest()
        {
            MegaSdk.cancelGetPreview(OriginalMNode);
            IsBusy = false;
        }

        public void SetPreviewImage()
        {
            if (IsBusy) return;
            if (!OriginalMNode.hasPreview()) return;

            if (OriginalMNode.hasPreview())
                GetPreview();
            else
                GetImage(true);
        }

        public void SetImage()
        {
            if (IsBusy) return;

            GetImage(false);
        }

        #endregion

        #region Private Methods

        private void GetPreview()
        {
            //if (FileService.FileExists(PreviewPath))
            //{
            //    PreviewImageUri = new Uri(PreviewPath);
            //}
            //else
            //{
            //    MegaSdk.getPreview(this.OriginalMNode, PreviewPath, new GetPreviewRequestListener(this));
            //}
        }

        private void GetImage(bool isForPreview)
        {
            if (FileService.FileExists(LocalDownloadPath))
            {
                ImageUri = new Uri(LocalDownloadPath);

                if (!isForPreview) return;

                PreviewImageUri = new Uri(PreviewPath);

            }
            else
            {
                if (isForPreview)
                    IsBusy = true;
                Transfer.AutoLoadImageOnFinish = true;
                Transfer.StartTransfer();
            }
        }

        #endregion

        #region Properties

        public bool InViewingRange { get; set; }

        private Uri _previewImageUri;
        public Uri PreviewImageUri
        {
            get
            {
                if (_previewImageUri == null && InViewingRange)
                    SetPreviewImage();
                return _previewImageUri;
            }
            set { SetField(ref _previewImageUri, value); }
        }

        private Uri _imageUri;
        public Uri ImageUri
        {
            get { return _imageUri; }
            set { SetField(ref _imageUri, value); }
        }

        public string PreviewPath => Path.Combine(ApplicationData.Current.LocalFolder.Path,
            ResourceService.AppResources.GetString("AR_PreviewsDirectory"), 
            OriginalMNode.getBase64Handle());

        #endregion
    }
}
