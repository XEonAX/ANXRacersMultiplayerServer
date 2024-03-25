using LiteNetLib.Utils;

namespace ANXRacersGalaxy.Serializables
{
    /// <summary>
    /// Packet for receiving/sending Chat messages
    /// </summary>
    [System.Serializable]
    public class PStringMessage : INetSerializable
    {
        public string Message { get; set; }
        public PStringMessage()
        {

        }
        public void Deserialize(NetDataReader reader)
        {
            Message = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Message);
        }
    }
}