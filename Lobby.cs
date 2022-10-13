using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

// using PlayerDict = Godot.Collections.Dictionary<int, Godot.Vector2>;
using PlayerDict = System.Collections.Generic.Dictionary<int, string>;
public class Lobby : Node
{

    public PlayerDict Players = new PlayerDict();
    private NetworkManager NetworkManager;

    [Signal]
    public delegate void ServerGaveID(int ID);
    [Signal]
    public delegate void PlayerCountChanged(GenericObjectWrapper<PlayerDict> players);
    [Signal]
    public delegate void ConnectedToServer();
    [Signal]
    public delegate void DisconnectedFromServer();

    public override void _Ready()
    {
        base._Ready();
        NetworkManager = GetNode<NetworkManager>("/root/NetworkManager");
    }

    public void ServerOnPlayerConnected(int ID)
    {
        AddPlayerToLobby(ID);
        Rpc(nameof(ClientReceivePlayerInfo), JsonConvert.SerializeObject(Players));
        EmitSignal(nameof(PlayerCountChanged), new GenericObjectWrapper<PlayerDict>(Players));
    }

    public void ServerOnPlayerDisconnected(int ID)
    {
        Rpc(nameof(RemovePlayerFromLobby), ID);
    }

    public void OnClientConnectedOK()
    {
        GD.Print("Connected!");
        SetPlayerName(GetTree().GetNetworkUniqueId(), NetworkManager.PlayerName);
        Rpc(nameof(ServerPlayerInfoRequested));
        EmitSignal(nameof(ConnectedToServer));
    }

    public void OnClientConnectionFailed()
    {
        EmitSignal(nameof(DisconnectedFromServer));
        NetworkManager.CancelNetwork();
    }

    public void OnServerDisconnectedClient()
    {
        GD.Print("Server disconnected!");
        EmitSignal(nameof(DisconnectedFromServer));
        NetworkManager.CancelNetwork();
    }

    [Master]
    public void ServerPlayerInfoRequested()
    {
        Rpc(nameof(ClientReceivePlayerInfo), JsonConvert.SerializeObject(Players));
    }

    [PuppetSync]
    public void ClientReceivePlayerInfo(String players)
    {
        GD.Print("Players received: " + players);
        UpdatePlayersLocally(JsonConvert.DeserializeObject<PlayerDict>(players));
    }

    [RemoteSync]
    public void GetID(int ID)
    {
        EmitSignal(nameof(ServerGaveID), ID);
    }

    [RemoteSync]
    public void UpdatePlayersLocally(PlayerDict players)
    {
        Players = players;
        EmitSignal(nameof(PlayerCountChanged), new GenericObjectWrapper<PlayerDict>(players));
    }

    [PuppetSync]
    public void RequestPlayerInfo()
    {
        Rpc(nameof(ServerPlayerInfoRequested));
    }

    [Master]
    private void AddPlayerToLobby(int ID)
    {
        Players[ID] = "";
        EmitSignal(nameof(PlayerCountChanged), Players);
    }

    [RemoteSync]
    public void RemovePlayerFromLobby(int ID)
    {
        Players.Remove(ID);
        EmitSignal(nameof(PlayerCountChanged), new GenericObjectWrapper<PlayerDict>(Players));
    }

    [Master]
    public void MasterSetPlayerName(int ID, string name)
    {
        if (Players.ContainsKey(ID))
        {
            Players[ID] = name;
        }

        var obj = new GenericObjectWrapper<PlayerDict>(Players);
        EmitSignal(nameof(PlayerCountChanged), obj);
        Rpc(nameof(ClientReceivePlayerInfo), JsonConvert.SerializeObject(Players));
    }

    public void SetPlayerName(int ID, string name)
    {
        RpcId(1, nameof(MasterSetPlayerName), ID, name);
    }
}
