using Godot;
using System;
using System.Linq;

public enum ServerType
{
    Enet,
    WebSocket
}

public class NetworkManager : Node
{
    [Export]
    public ServerType ServerType;
    private NetworkedMultiplayerPeer Peer;
    private Lobby Lobby;
    public int? ID { get; private set; }
    public string PlayerName { get; private set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        Lobby = GetNode<Lobby>("Lobby");
    }

    public async void InitialiseServer()
    {
        if (ServerType == ServerType.WebSocket)
        {
            var peer = new WebSocketServer();
            peer.Listen(5432, new string[0], true);

            GetTree().Connect("network_peer_connected", Lobby, nameof(Lobby.ServerOnPlayerConnected));
            GetTree().Connect("network_peer_disconnected", Lobby, nameof(Lobby.ServerOnPlayerDisconnected));

            GetTree().Multiplayer.NetworkPeer = peer;
            Peer = peer;
            await ToSignal(GetTree().CreateTimer(1f), "timeout");
            Lobby.ServerOnPlayerConnected(GetTree().Multiplayer.GetNetworkUniqueId());
            SetPlayerName("server");
        }
    }

    public void InitialiseClient(string address)
    {
        if (ServerType == ServerType.WebSocket)
        {
            var peer = new WebSocketClient();
            GetTree().Connect("connected_to_server", Lobby, nameof(Lobby.OnClientConnectedOK));
            GetTree().Connect("connection_failed", Lobby, nameof(Lobby.OnClientConnectionFailed));
            GetTree().Connect("server_disconnected", Lobby, nameof(Lobby.OnServerDisconnectedClient));
            peer.SetTargetPeer(NetworkedMultiplayerPeer.TargetPeerServer);

            var error = peer.ConnectToUrl(address + ":5432", new string[0], true);

            if (error != 0)
            {
                GD.Print("Could not connect to server");
                GetTree().Disconnect("connected_to_server", Lobby, nameof(Lobby.OnClientConnectedOK));
                GetTree().Disconnect("connection_failed", Lobby, nameof(Lobby.OnClientConnectionFailed));
                GetTree().Disconnect("server_disconnected", Lobby, nameof(Lobby.OnServerDisconnectedClient));
                return;
            }

            Peer = peer;
            GetTree().NetworkPeer = peer;
            Lobby.SetNetworkMaster(1);
        }
    }

    public void CancelNetwork()
    {
        if (Peer == null)
        {
            return;
        }
        if (GetTree().IsConnected("connected_to_server", Lobby, nameof(Lobby.OnClientConnectedOK)))
        {
            GetTree().Disconnect("connected_to_server", Lobby, nameof(Lobby.OnClientConnectedOK));
            GetTree().Disconnect("connection_failed", Lobby, nameof(Lobby.OnClientConnectionFailed));
            GetTree().Disconnect("server_disconnected", Lobby, nameof(Lobby.OnServerDisconnectedClient));
        }
        if (GetTree().IsConnected("network_peer_connected", Lobby, nameof(Lobby.ServerOnPlayerConnected)))
        {
            GetTree().Disconnect("network_peer_connected", Lobby, nameof(Lobby.ServerOnPlayerConnected));
            GetTree().Disconnect("network_peer_disconnected", Lobby, nameof(Lobby.ServerOnPlayerDisconnected));
        }
        if (Peer is WebSocketClient)
        {
            ((WebSocketClient)Peer).DisconnectFromHost();
        }
        else if (Peer is WebSocketServer)
        {
            ((WebSocketServer)Peer).Stop();
        }
        else
        {
            ((NetworkedMultiplayerENet)Peer).CloseConnection();
        }
        GetTree().NetworkPeer = null;
        Peer = null;
    }

    public void OnLobbyServerGaveID(int ID)
    {
        this.ID = ID;
    }

    public void SetPlayerName(string name)
    {
        PlayerName = name;
        var ID = this.ID == null ? GetTree().GetNetworkUniqueId() : this.ID;
        Lobby.SetPlayerName((int)ID, name);
    }

}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ExportEnumAsFlagsAttribute : ExportAttribute
{
    public ExportEnumAsFlagsAttribute(Type enumType) : base(PropertyHint.Flags, enumType.IsEnum ? string.Join(",", Enum.GetValues(enumType).OfType<Enum>().Where(e => (int)(object)e != 0)) : "Invalid Type")
    {
    }
}