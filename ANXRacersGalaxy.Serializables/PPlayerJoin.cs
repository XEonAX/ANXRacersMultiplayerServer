namespace ANXRacersGalaxy.Serializables
{
    /// <summary>
    /// Packet sent by Player to us when they join
    /// </summary>
    [System.Serializable]
    public class PPlayerJoin
    {
        public uint MPId { get; set; }
        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        public string SkinId { get; set; }
        public PPlayerJoin()
        {

        }
    }
}