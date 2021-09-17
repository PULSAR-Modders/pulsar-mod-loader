namespace PulsarModLoader
{
    public enum MPFunction
    {
        /// <summary>
        /// No MP Requirements
        /// </summary>
        None,          //0 No mp requirements
        /// <summary>
        /// Only the host is required to have it installed
        /// </summary>
        HostOnly,      //1 Only the host is required to have it installed
        /// <summary>
        /// Host must have installed for clients to use
        /// </summary>
        HostRequired,  //2 Host must have installed for clients to use
        /// <summary>
        /// All clients must have installed
        /// </summary>
        All            //3 All clients must have installed
    }
}
