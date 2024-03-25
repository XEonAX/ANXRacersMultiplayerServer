namespace ANXRacersGalaxy.Serializables
{
    /// <summary>
    /// Packet send to all remaining connected clients that a Player has disconnected
    /// </summary>
    [System.Serializable]
    public class PPlayerLeft
    {
        public uint MPId { get; set; }

        public PPlayerLeft()
        {
        }
    }
}