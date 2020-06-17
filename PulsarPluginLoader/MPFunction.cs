namespace PulsarPluginLoader
{
    public enum MPFunction
    {
        None,          //0 No mp requirements
        HostOnly,      //1 Only the host is required to have it installed
        HostApproved,  //2 Host must have installed for clients to use
        All            //3 All clients must have installed
    }
}
