using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using MegaApp.Enums;
using MegaApp.ViewModels;

namespace MegaApp.Classes
{
    public class TransferQueue: ObservableCollection<TransferObjectModel>
    {
        public TransferQueue()
        {
            QueuePaused = true;

            Uploads = new ObservableCollection<TransferObjectModel>();
            Downloads = new ObservableCollection<TransferObjectModel>();

            Uploads.CollectionChanged += UploadsOnCollectionChanged;
            Downloads.CollectionChanged += DownloadsOnCollectionChanged;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);

            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        var transferObject = (TransferObjectModel)item;
                        switch (transferObject.Type)
                        {
                            case TransferType.Download:
                                DownloadSort(transferObject);
                                break;

                            case TransferType.Upload:
                                UploadSort(transferObject);
                                break;
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        var transferObject = (TransferObjectModel)item;
                        switch (transferObject.Type)
                        {
                            case TransferType.Download:
                                Downloads.Remove(transferObject);
                                break;

                            case TransferType.Upload:
                                Uploads.Remove(transferObject);
                                break;
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private void DownloadsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                        ((TransferObjectModel)item).PropertyChanged += DownloadsOnPropertyChanged;
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                        ((TransferObjectModel)item).PropertyChanged -= DownloadsOnPropertyChanged;
                    break;

                default:
                    break;
            }
        }

        private void UploadsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                        ((TransferObjectModel)item).PropertyChanged += UploadsOnPropertyChanged;
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                        ((TransferObjectModel)item).PropertyChanged -= UploadsOnPropertyChanged;
                    break;

                default:
                    break;
            }
        }       

        private void UploadsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Equals("Status")) return;

            UploadSort((TransferObjectModel) sender);
        }

        private void UploadSort(TransferObjectModel transferObject)
        {
            if (Uploads.Contains(transferObject))
                if (!Uploads.Remove(transferObject)) return;

            var inserted = false;

            for (var i = 0; i <= Uploads.Count - 1; i++)
            {
                if ((int)transferObject.Status <= (int)Uploads[i].Status)
                {
                    Uploads.Insert(i, transferObject);
                    inserted = true;
                    break;
                }
            }

            if (!inserted)
                Uploads.Add(transferObject);
        }

        private void DownloadSort(TransferObjectModel transferObject)
        {
            if (Downloads.Contains(transferObject))
                if (!Downloads.Remove(transferObject)) return;

            var inserted = false;

            for (var i = 0; i <= Downloads.Count - 1; i++)
            {
                if ((int)transferObject.Status <= (int)Downloads[i].Status)
                {
                    Downloads.Insert(i, transferObject);
                    inserted = true;
                    break;
                }
            }

            if (!inserted)
                Downloads.Add(transferObject);
        }

        private void DownloadsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!e.PropertyName.Equals("Status")) return;

            DownloadSort((TransferObjectModel)sender);
        }

        public ObservableCollection<TransferObjectModel> Uploads { get; private set; }

        public ObservableCollection<TransferObjectModel> Downloads { get; private set; }

        public bool QueuePaused { get; set; }
    }
}
