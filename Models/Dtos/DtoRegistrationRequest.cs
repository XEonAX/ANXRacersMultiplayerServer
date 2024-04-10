public class DtoRegistrationRequest
{
    public int Version { get { return Constants.Version; } }
    public string ServerDisplayName { get; set; }
    public Uri PublicFacingUrl { get; set; }
    public int PortUDP { get; set; }
    public bool IsPublic { get; set; }
}