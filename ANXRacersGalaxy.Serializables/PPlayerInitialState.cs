using LiteNetLib.Utils;

namespace ANXRacersGalaxy.Serializables
{
    /// <summary>
    /// State sent to a Player or Spectator, informing the current state of a player
    /// </summary>
    [System.Serializable]
    public class PPlayerInitialState : INetSerializable
    {
        public uint MPId { get; set; }
        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        public string ShipSkinId { get; set; }
        public PShipUpdate ShipState { get; set; }

        public PPlayerInitialState()
        {
            ShipState = new PShipUpdate();
        }
        public PPlayerInitialState(Player player)
        {
            MPId = player.MPId;
            UserId = player.UserId.ToString();
            UserDisplayName = player.UserDisplayName;
            ShipState = player.state;
            ShipSkinId = player.ShipSkinId;
        }
        public void Serialize(LiteNetLib.Utils.NetDataWriter writer)
        {
            writer.Put(MPId);
            writer.Put(ShipSkinId);
            writer.Put(UserId);
            writer.Put(UserDisplayName);
            ShipState.Serialize(writer);
        }

        public void Deserialize(LiteNetLib.Utils.NetDataReader reader)
        {
            MPId = reader.GetUInt();
            ShipSkinId = reader.GetString();
            UserId = reader.GetString();
            UserDisplayName = reader.GetString();
            ShipState.Deserialize(reader);
        }
    }
}