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
    /// Incoming shares short options
    /// </summary>
    public enum IncomingSharesSortOrderType
    {
        ORDER_NAME              = 0,
        ORDER_MODIFICATION      = 1,
        ORDER_ACCESS            = 2,
        ORDER_OWNER             = 3
    }

    /// <summary>
    /// Outgoing shares short options
    /// </summary>
    public enum OutgoingSharesSortOrderType
    {
        ORDER_NAME  = 0
    }
}
