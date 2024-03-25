using LiteNetLib.Utils;

namespace ANXRacersGalaxy.Serializables
{

    /// <summary>
    /// This packet contains every players state, It is frequently announced to each client
    /// </summary>
    [System.Serializable]
    public class PPlayerStates : INetSerializable
    {
        public uint serverTime { get; set; }

        public PShipUpdate[] Players { get; set; }
        public PPlayerStates()
        {

        }
        public void Deserialize(NetDataReader reader)
        {
            serverTime = reader.GetUInt();
            var playersCount = reader.GetInt();
            Players = new PShipUpdate[playersCount];
            for (int i = 0; i < playersCount; i++)
            {
                Players[i] = new PShipUpdate();
                Players[i].Deserialize(reader);
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(serverTime);
            var playersCount = Players.Length;
            writer.Put(playersCount);
            for (int i = 0; i < playersCount; i++)
            {
                Players[i].Serialize(writer);
            }
        }
    }
}