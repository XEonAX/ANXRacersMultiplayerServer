using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Encodings;
using ANXRacersGalaxy.Serializables;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public interface IMultiplayerServer
{
    void ReceiveUpdates();
    void SendUpdates(uint deltatime);
    void StartServer();
    void StopServer();
}

public class MultiplayerServer : EventBasedNetListener, IMultiplayerServer
{
    private readonly ILogger<MultiplayerServer> _logger;
    private readonly Communications _communications;
    private readonly IStateService _stateService;
    private readonly string _uniqueKeyForANXRacersNetPeer = Encoding.UTF8.GetString(Convert.FromBase64String("ZTlUSW1kZUJsWTZwSTR3Z0Vu"));
    NetManager litenetMgr;
    NetPacketProcessor packetProcessor;

    int _port;
    bool _isStarted = false;
    bool _isPacketProcessorInitialized = false;

    volatile uint _MPId = 1;
    uint _elapsedTime;


    public List<NetPeer> Peers = new List<NetPeer>();
    public List<Player> Players = new List<Player>();
    public Dictionary<uint, Player> PlayersDict = new Dictionary<uint, Player>();
    public MultiplayerServer(ILogger<MultiplayerServer> logger, IOptions<Configs> options, Communications communications, IStateService stateService)
    {
        _logger = logger;
        _communications = communications;
        _stateService = stateService;
        _port = options.Value.PortUDP;
    }
    public void StartServer()
    {
        if (!_isPacketProcessorInitialized)
        {
            InitializePacketProcessor();
        }

        litenetMgr = new NetManager(this);
        litenetMgr.Start(_port);
        litenetMgr.BroadcastReceiveEnabled = false;
        litenetMgr.UpdateTime = 15;
        litenetMgr.DisconnectTimeout = 50000;
        _isStarted = true;
        RegisterEventHandlers();
        _logger.LogInformation("Server started on port:{_port}", _port);
        _stateService.State.IsServerStarted = true;
        _communications.RegisterServer();
    }

    private void RegisterEventHandlers()
    {
        ConnectionRequestEvent += ConnectionRequestHandler;
        PeerConnectedEvent += PeerConnectedHandler;
        NetworkReceiveUnconnectedEvent += NetworkReceiveUnconnectedHandler;
        NetworkErrorEvent += NetworkErrorHandler;
        NetworkLatencyUpdateEvent += NetworkLatencyUpdateHandler;
        PeerDisconnectedEvent += PeerDisconnectedHandler;
        NetworkReceiveEvent += NetworkReceiveHandler;
    }

    public void StopServer()
    {
        _stateService.State.IsServerStarted = false;
        litenetMgr.Stop();
    }

    public void ReceiveUpdates()
    {
        if (!_isStarted)
            return;

        litenetMgr.PollEvents();
    }


    public void SendUpdates(uint deltatime)
    {
        if (!_isStarted)
            return;

        _elapsedTime += deltatime;
        PPlayerStates PlayerStates = new PPlayerStates();
        PlayerStates.serverTime = _elapsedTime;
        List<PShipUpdate> serializedPlayers = new List<PShipUpdate>();
        PlayerStates.Players = Players.Select(p => p.state).ToArray();
        for (int i = 0; i < Peers.Count; i++)
        {
            Peers[i].Send(packetProcessor.Write(PlayerStates), DeliveryMethod.Unreliable);
        }
    }




    void InitializePacketProcessor()
    {
        packetProcessor = new NetPacketProcessor();
        packetProcessor.RegisterNestedType<PPlayerInitialState>(() => { return new PPlayerInitialState(); });
        packetProcessor.RegisterNestedType<PShipUpdate>(() => { return new PShipUpdate(); });
        packetProcessor.RegisterNestedType<PStringMessage>(() => { return new PStringMessage(); });
        // serializer.SubscribeReusable<PClientHandshake, NetPeer>(OnReceiveHandshake);
        packetProcessor.SubscribeReusable<PShipUpdate, NetPeer>(OnReceiveShipUpdate);
        packetProcessor.SubscribeReusable<PPlayerJoin, NetPeer>(OnReceivePlayerJoin);
        packetProcessor.SubscribeReusable<PSpectatorJoin, NetPeer>(OnReceiveSpectatorJoin);
        packetProcessor.SubscribeReusable<PNewScoreAndRank, NetPeer>(OnReceiveNewScoreAndRank);
        packetProcessor.SubscribeReusable<PStringMessage, NetPeer>(OnReceiveMessage);

        _isPacketProcessorInitialized = true;
    }


    public void ConnectionRequestHandler(ConnectionRequest request)
    {
        request.AcceptIfKey(_uniqueKeyForANXRacersNetPeer);
    }
    public void PeerConnectedHandler(NetPeer peer)
    {
        _logger.LogInformation("New peer connected: {peer.EndPoint}", peer.EndPoint);
        if (!Peers.Contains(peer))
        {
            Peers.Add(peer);
        }
    }

    public void NetworkErrorHandler(IPEndPoint endPoint, SocketError socketErrorCode)
    {
        _logger.LogError("Server error! {socketErrorCode}", socketErrorCode);
    }

    public void NetworkReceiveUnconnectedHandler(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        _logger.LogInformation("Received unconnected {messageType} from {remoteEndPoint}", messageType, remoteEndPoint);
    }

    public void NetworkLatencyUpdateHandler(NetPeer peer, int latency)
    {

    }

    public void PeerDisconnectedHandler(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        if (Peers.Contains(peer))
        {
            Peers.Remove(peer);
        }

        Player lp = GetPlayerFromPeer(peer);

        if (lp != default)
        {
            RemovePlayer(lp);
            PPlayerLeft packet = new PPlayerLeft();
            packet.MPId = lp.MPId;

            for (int i = 0; i < Peers.Count; i++)
            {
                Peers[i].Send(packetProcessor.Write(packet), DeliveryMethod.ReliableOrdered);
            }
        }

        _communications.SetActivePlayerCount(Players.Count);
        _logger.LogInformation("Peer disconnected, reason: {disconnectInfo.Reason}", disconnectInfo.Reason);
    }

    public void NetworkReceiveHandler(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        packetProcessor.ReadAllPackets(reader, peer);
    }

    public void OnReceivePlayerJoin(PPlayerJoin packet, NetPeer peer)
    {
        int id = Players.Count;

        var galaxyPlayer = CreatePlayer(peer, packet);
        if (galaxyPlayer == default)
        {
            peer.Send(packetProcessor.Write(new PStringMessage() { Message = "ServerFull" }), DeliveryMethod.ReliableUnordered);
        }
        else
        {
            PGalaxy GalaxyPacket = new PGalaxy();
            GalaxyPacket.MPId = galaxyPlayer.MPId;
            List<PPlayerInitialState> serializedPlayers = new List<PPlayerInitialState>();

            for (int i = 0; i < Players.Count; i++)
            {
                Player p = Players[i];
                PPlayerInitialState slp = new PPlayerInitialState(p);
                serializedPlayers.Add(slp);
            }
            GalaxyPacket.Players = serializedPlayers.ToArray();
            peer.Send(packetProcessor.Write(GalaxyPacket), DeliveryMethod.ReliableUnordered);

            packet.MPId = galaxyPlayer.MPId;
            for (int i = 0; i < Peers.Count; i++)
            {
                if (Peers[i] != peer)
                {
                    Peers[i].Send(packetProcessor.Write(packet), DeliveryMethod.ReliableUnordered);
                }
            }

            _communications.PlayerJoined(packet.UserId, packet.UserDisplayName);
            _communications.SetActivePlayerCount(Players.Count);
        }
    }


    public void OnReceiveSpectatorJoin(PSpectatorJoin packet, NetPeer peer)
    {
        var client = new Spectator(peer);
        peer.Tag = client;

        PGalaxy GalaxyPacket = new PGalaxy();
        GalaxyPacket.MPId = 0;
        List<PPlayerInitialState> serializedPlayers = new List<PPlayerInitialState>();

        for (int i = 0; i < Players.Count; i++)
        {
            Player p = Players[i];
            PPlayerInitialState slp = new PPlayerInitialState(p);
            serializedPlayers.Add(slp);
        }
        GalaxyPacket.Players = serializedPlayers.ToArray();
        peer.Send(packetProcessor.Write(GalaxyPacket), DeliveryMethod.ReliableUnordered);

        _communications.PlayerJoined(packet.UserId, packet.UserDisplayName);
        _communications.SetActivePlayerCount(Players.Count);
    }
    public void OnReceiveNewScoreAndRank(PNewScoreAndRank packet, NetPeer peer)
    {
        for (int i = 0; i < Peers.Count; i++)
        {
            if (Peers[i] != peer)
            {
                Peers[i].Send(packetProcessor.Write(packet), DeliveryMethod.ReliableOrdered);
            }
        }
        var client = peer.Tag as Spectator;
        _communications.NewScoreAndRank(client.UserId, client.UserDisplayName, packet.Time / 1000f, packet.Rank);
    }

    public void OnReceiveShipUpdate(PShipUpdate packet, NetPeer peer)
    {
        if (PlayersDict.TryGetValue(packet.MPId, out var remotePlayer))
        {
            if (remotePlayer.state != null && remotePlayer.state.t < packet.t)
            {
                remotePlayer.state.Copy(packet);
            }
            else if (remotePlayer.state == null)
            {
                remotePlayer.state = new PShipUpdate(packet);
            }
        }
    }

    public Player GetPlayerFromPeer(LiteNetLib.NetPeer peer)
    {
        return peer.Tag as Player;
    }


    public void RemovePlayer(Player player)
    {
        if (Players.Contains(player))
        {
            Players.Remove(player);
        }
        PlayersDict.Remove(player.MPId, out var _);
    }

    public Player CreatePlayer(LiteNetLib.NetPeer peer, PPlayerJoin packet)
    {
        var slotFound = false;
        byte slotAttempts = 100;
        uint newMPId;
        while (!slotFound && slotAttempts > 0)
        {
            slotAttempts--;
            newMPId = Interlocked.Increment(ref _MPId);
            if (!PlayersDict.ContainsKey(newMPId))
            {
                slotFound = true;

                Player galaxyPlayer = new Player(newMPId, peer, packet);
                Players.Add(galaxyPlayer);
                PlayersDict[newMPId] = galaxyPlayer;
                peer.Tag = galaxyPlayer;
                return galaxyPlayer;
            }
        }
        return null;
    }
    private void OnReceiveMessage(PStringMessage packet, NetPeer peer)
    {
        var client = peer.Tag as Spectator;
        if (client != default)
        {
            _communications.ChatMessage(client.UserId, client.UserDisplayName, packet.Message);
            var packetToForward = new PStringMessage() { Message = $"<b>{client.UserDisplayName}:</b> {packet.Message}" };
            for (int i = 0; i < Peers.Count; i++)
            {
                Peers[i].Send(packetProcessor.Write(packetToForward), DeliveryMethod.ReliableOrdered);
            }
        }
    }
}