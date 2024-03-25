using LiteNetLib.Utils;

namespace ANXRacersGalaxy.Serializables
{
    /// <summary>
    /// Packet sent to a Player or Spectator, informing the current state of all players
    /// </summary>
    [System.Serializable]
    public class PGalaxy: INetSerializable
    {
        public uint MPId { get; set; } 
        public PPlayerInitialState[] Players { get; set; }

        public void Deserialize(NetDataReader reader)
        {
            MPId = reader.GetUInt();
            var playersCount = reader.GetInt();
            Players = new PPlayerInitialState[playersCount];
            for (int i = 0; i < playersCount; i++)
            {
                Players[i] = new PPlayerInitialState();
                Players[i].Deserialize(reader);
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(MPId);
            var playersCount = Players.Length;
            writer.Put(playersCount);
            for (int i = 0; i < playersCount; i++)
            {
                Players[i].Serialize(writer);
            }
        }
    }
}