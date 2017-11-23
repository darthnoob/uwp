namespace MegaApp.Enums
{
    /// <summary>
    /// Possible sort directions
    /// </summary>
    public enum SortOrderDirection
    {
        ORDER_ASCENDING     = 0,
        ORDER_DESCENDING    = 1
    }

    /// <summary>
    /// Nodes sort options
    /// </summary>
    public enum NodesSortOrderType
    {
        ORDER_NAME          = 0,
        ORDER_SIZE          = 1,
        ORDER_CREATION      = 2,
        ORDER_MODIFICATION  = 3,
        ORDER_TYPE          = 4,
    }

    /// <summary>
    /// Contacts sort options
    /// </summary>
    public enum ContactsSortOrderType
    {
        ORDER_NAME  = 0,
        ORDER_EMAIL = 1
    }

    /// <summary>
    /// Contact requests sort options
    /// </summary>
    public enum ContactRerquestsSortOrderType
    {
        ORDER_NAME = 0
    }

    /// <summary>
    /// Incoming shares sort options
    /// </summary>
    public enum IncomingSharesSortOrderType
    {
        ORDER_NAME              = 0,
        ORDER_MODIFICATION      = 1,
        ORDER_ACCESS            = 2,
        ORDER_OWNER             = 3
    }

    /// <summary>
    /// Outgoing shares sort options
    /// </summary>
    public enum OutgoingSharesSortOrderType
    {
        ORDER_NAME  = 0
    }
}
