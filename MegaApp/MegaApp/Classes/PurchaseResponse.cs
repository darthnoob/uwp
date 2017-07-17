using Windows.ApplicationModel.Store;
using MegaApp.Enums;

namespace MegaApp.Classes
{
    public class PurchaseResponse
    {
        public PurchaseResponseType Type { get; set; } = PurchaseResponseType.Unknown;

        public PurchaseResults Result { get; set; }
    }
}
