namespace ANXRacersGalaxy.Serializables
{   
    /// <summary>
    /// Packet sent by Spectator to us when they join
    /// </summary>
    [System.Serializable]
    public class PSpectatorJoin
    {
        public uint MPId { get; set; }
        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        public PSpectatorJoin()
        {

        }
    }

}