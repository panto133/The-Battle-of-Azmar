using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MyNetworkManager : NetworkManager
{
    [Header("Other variables")]
    [SerializeField] private int minPlayers = 2;

    [SerializeField] private MyNetworkPlayer roomPlayerPrefab = null;
    [SerializeField] private MyNetworkGamePlayer gamePlayerPrefab = null;
    [SerializeField] private Canvas mainCanvas;

    [SerializeField] private TMP_InputField ipAddressInputField;

    [SerializeField] private GameObject hostLobbyPanel;
    [SerializeField] private GameObject joinLobbyPanel;

    //Kada se klikne pridruzi dugme bez pokrenutog servera treba se 10s disablovati dugme za
    //nazad jer ne moze da se pokrene server u toku tih 10s pauze

    public List<MyNetworkPlayer> RoomPlayers { get; } = new List<MyNetworkPlayer>();
    public List<MyNetworkGamePlayer> GamePlayers { get; } = new List<MyNetworkGamePlayer>();
    public override void OnServerConnect(NetworkConnection conn)
    {
        if (numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }
    }
    public override void OnClientConnect(NetworkConnection conn)
    {
        joinLobbyPanel.SetActive(false);
        mainCanvas.enabled = false;
        base.OnClientConnect(conn);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
    }
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        bool isLeader = RoomPlayers.Count == 0;

        MyNetworkPlayer myPlayer = Instantiate(roomPlayerPrefab);

        NetworkServer.AddPlayerForConnection(conn, myPlayer.gameObject);

        myPlayer.IsLeader = isLeader;
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.identity != null)
        {
            var player = conn.identity.GetComponent<MyNetworkPlayer>();

            RoomPlayers.Remove(player);

            NotifyPlayersOfReadyState();
        }
        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        RoomPlayers.Clear();
    }
    public void NotifyPlayersOfReadyState()
    {
        foreach (var player in RoomPlayers)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }
    //returns true if all players in lobby are ready
    //returns false if at least one player in lobby
    //is not ready
    private bool IsReadyToStart()
    {
        if (numPlayers < minPlayers) return false;

        foreach (var player in RoomPlayers)
        {
            if (!player.IsReady) return false;
        }

        return true;
    }

    public void HostLobby()
    {
        StartHost();
        hostLobbyPanel.SetActive(false);
        mainCanvas.enabled = false;
    }
    public void JoinLobby()
    {
        networkAddress = ipAddressInputField.text;
        StartClient();
    }
    public override void ServerChangeScene(string newSceneName)
    {
        for (int i = RoomPlayers.Count - 1; i >= 0; i--)
        {
            var conn = RoomPlayers[i].connectionToClient;
            var gamePlayerInstance = Instantiate(gamePlayerPrefab);
            gamePlayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);

            NetworkServer.Destroy(conn.identity.gameObject);

            NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject);
        }

        base.ServerChangeScene(newSceneName);
    }
    public void StartGame()
    {
        if (!IsReadyToStart()) { return; }

        ServerChangeScene("GameScene");
    }

}
