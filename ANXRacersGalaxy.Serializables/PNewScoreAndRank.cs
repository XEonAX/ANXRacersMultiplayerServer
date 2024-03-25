namespace ANXRacersGalaxy.Serializables
{
    /// <summary>
    /// Packet received/sent whenever someone finishes a race.
    /// </summary>
    [System.Serializable]
    public class PNewScoreAndRank
    {
        public uint MPId { get; set; }
        public uint ScoreId { get; set; }
        public uint Time { get; set; }
        public uint Rank { get; set; }
        public PNewScoreAndRank()
        {

        }
    }

}