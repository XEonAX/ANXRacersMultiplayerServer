using LiteNetLib.Utils;

namespace ANXRacersGalaxy.Serializables
{
    /// <summary>
    /// Every Player connected to us, send this packet containing their spaceship's transform and velocity
    /// </summary>
    [System.Serializable]
    public class PShipUpdate : INetSerializable
    {
        public PShipUpdate()
        {

        }
        public PShipUpdate(PShipUpdate packet)
        {
            MPId = packet.MPId;
            t = packet.t;
            x = packet.x;
            y = packet.y;
            z = packet.z;
            iy = packet.iy;
            iz = packet.iz;
        }

        public void Copy(PShipUpdate packet)
        {
            //MPId = packet.MPId;
            t = packet.t;
            x = packet.x;
            y = packet.y;
            z = packet.z;
            iy = packet.iy;
            iz = packet.iz;
        }

        /// <summary>
        /// MultiplayerId
        /// </summary>
        public uint MPId { get; set; }
        /// <summary>
        /// Incrementing sum of (uint)(deltatime * 1000)
        /// </summary>
        public uint t { get; set; }
        /// <summary>
        /// (int)(Spaceship.transform.localPosition.x * 1000)
        /// </summary>
        public int x { get; set; }
        /// <summary>
        /// (int)(Spaceship.transform.localPosition.y * 1000)
        /// </summary>
        public int y { get; set; }
        /// <summary>
        /// (int)(Spaceship.transform.localRotation.eulerAngles.z * 100);
        /// </summary>
        public int z { get; set; }
        /// <summary>
        /// (int)(Spaceship.input.InputTorque * 100)
        /// </summary>
        public int iy { get; set; }
        /// <summary>
        /// (int)(Spaceship.input.InputSurge * 100)
        /// </summary>
        public int iz { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(MPId);
            writer.Put(t);
            writer.Put(x);
            writer.Put(y);
            writer.Put(z);
            writer.Put(iy);
            writer.Put(iz);
        }

        public void Deserialize(NetDataReader reader)
        {
            MPId = reader.GetUInt();
            t = reader.GetUInt();
            x = reader.GetInt();
            y = reader.GetInt();
            z = reader.GetInt();
            iy = reader.GetInt();
            iz = reader.GetInt();
        }
    }
}

