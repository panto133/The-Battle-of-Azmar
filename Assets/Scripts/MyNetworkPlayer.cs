using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MyNetworkPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject playerCanvas;
    [SerializeField] private Button startGameButton;
    [SerializeField] public GameObject content;
    [SerializeField] private GameObject playerDataHolderPrefab;

    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Loading...";
    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;

    private bool isLeader;
    private bool amIDisconnected = false;
    public bool IsLeader
    {
        get { return isLeader; }
        set
        {
            isLeader = value;
            startGameButton.gameObject.SetActive(true);
        }
    }

    private MyNetworkManager room; 
    public MyNetworkManager Room
    {
        get
        {
            if (room != null) return room;
            return room = NetworkManager.singleton as MyNetworkManager;
        }
    }
    public override void OnStartAuthority()
    {
        CmdSetDisplayName(PlayerNameInput.DisplayName);
        playerCanvas.SetActive(true);
    }
    public override void OnStartClient()
    {
        Room.RoomPlayers.Add(this);
        AddPlayerTab();
        UpdateDisplay();
    }
    public override void OnStopClient()
    {
        try
        {
            //Nalazi svoj indeks taba u RoomPlayers i prosledjuje ga funkciji RemoveTab
            //kako bi skinuo svoj tab pri diskonektovanju klijenta i uklanja svoj objekat
            //iz liste RoomPlayers
            int br = -1;
            for (int i = 0; i < room.RoomPlayers.Count; i++)
            {
                if (room.RoomPlayers[i].hasAuthority)
                {
                    br = i;
                }
            }
            RemoveTab(br);
            Room.RoomPlayers.Remove(this);
        }
        catch
        {
            Debug.Log("Greska pri uklanjanju taba");
        }
    }
    public void HandleReadyStatusChanged(bool oldValue, bool newValue)
    {
        UpdateDisplay();
    }
    private void UpdateDisplay()
    {
        //Kod koji pronalazi objekat sa svojim autoritetom i poziva ovu funkciju
        if (!hasAuthority)
        {
            foreach (var player in Room.RoomPlayers)
            {
                if (player.hasAuthority)
                {
                    player.UpdateDisplay();
                    break;
                }
            }
            return;
        }

        for (int i = 0; i < Room.RoomPlayers.Count; i++)
        {
            if (!Room.RoomPlayers[i].transform.GetChild(0).gameObject.activeInHierarchy) continue;

            //Updatuje samo aktivne objekte u sceni
            //this content is content
            Transform content = Room.RoomPlayers[i].gameObject.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0);
            for (int j = 0; j < content.childCount; j++)
            {
                //Updatuje ime na displeju i ready status (ako je spreman ispisuje zelenim slovima, ako nije
                //ispisuje crvenim slovima [ternarni operator])
                content.GetChild(j).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text =
                    Room.RoomPlayers[j].DisplayName;
                content.GetChild(j).GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text =
                    Room.RoomPlayers[j].IsReady ?
                        "<color=green>Ready</color>" :
                        "<color=red>Not Ready</color>";
            }
        }
        
    }
    ///<summary>
    ///Pozvano svaki put kada se pridruzi novi igrac i dodaje na svakom klijentu
    ///content za igraca koji se pridruzio
    ///</summary>
    private void AddPlayerTab()
    {
        if (!hasAuthority)
        {
            foreach (var player in Room.RoomPlayers)
            {
                if (player.hasAuthority)
                {

                    //Add a content for an active player and add it to his player's content
                    GameObject playercontent = Instantiate(playerDataHolderPrefab, player.content.transform);
                    playercontent.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = DisplayName;
                    break;
                }
            }
            return;
        }

        for (int i = 0; i < Room.RoomPlayers.Count; i++)
        {
            GameObject playercontent = Instantiate(playerDataHolderPrefab, content.transform);
            playercontent.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = DisplayName;
        }
    }

    public void RemoveTab(int index)
    {
        //Pronadji objekat sa autoritetom i pozovi ovu funkciju
        if (!hasAuthority)
        {
            foreach (var player in Room.RoomPlayers)
            {
                if (player.hasAuthority)
                {
                    player.RemoveTab(index);
                    break;
                }
            }
            return;
        }
        try
        {
            //pronalazi content i unistava svoj tab
            Transform content = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0);
            NetworkServer.Destroy(content.GetChild(index).gameObject);

            if (Room.RoomPlayers.Count == 1)
            {
                //Updatuje display na svim drugim klijentima ako se nije diskonektovao
                if (!amIDisconnected)
                {
                    if (NetworkServer.active)
                    {
                        CmdUpdateDisplayAfterTabRemoval();
                    }
                    else
                    {
                        amIDisconnected = true;
                        VratiULobi();
                        Destroy(gameObject);
                    }
                }
            }
        }
        catch
        {
            Debug.Log("Greska pri unistavanju taba i pozivanja update-a na drugim klijentima nakon unistenja");
        }
    }
    public void HandleDisplayNameChanged(string oldValue, string newValue)
    {
        UpdateDisplay();
    }
    public void HandleReadyToStart(bool readyToStart)
    {
        if (!isLeader) return;

        startGameButton.interactable = readyToStart;
    }

    [Command]
    private void CmdSetDisplayName(string displayName)
    {
        DisplayName = displayName;
    }
    [Command]
    public void CmdReadyUp()
    {
        IsReady = !IsReady;

        Room.NotifyPlayersOfReadyState();
    }
    [Command]
    public void CmdStartGame()
    {
        if (Room.RoomPlayers[0].connectionToClient != connectionToClient) return;

        Room.StartGame();
    }
    private void VratiULobi()
    {
        GameObject.Find("MainMenu_Canvas").GetComponent<Canvas>().enabled = true; 
        Room.StopClient();
        Room.StopServer();
    }
    /// <summary>
    /// Pozvano sa servera da diskonektuje odredjenog igraca i unisti njegov objekat.
    /// Funkcija je pozvana samo na jednom klijentu zahtevan od servera od strane tog
    /// istog klijenta
    /// </summary>
    [TargetRpc]
    private void TargetClientLeavesLobby()
    {
        amIDisconnected = true;
        if (Room.RoomPlayers.Count == 1) Room.StopHost();

        Room.StopClient();
        Destroy(gameObject);
        VratiULobi();
    }
    [ClientRpc]
    private void RpcDisconnectAllPlayers()
    {
        if (Room.RoomPlayers.Count == 1) TargetClientLeavesLobby();
        else Room.StopClient();
    }
    /// <summary>
    /// Pozvano od strane klijenta da se izvrsi na serveru kako bi pozvao drugu funkciju.
    /// Poziva se klikom na dugme "Leave"
    /// </summary>
    [Command]
    public void CmdLeaveLobby()
    {
        TargetClientLeavesLobby();
    }
    /// <summary>
    /// Pozvano od strane klijenta da se izvrsi na serveru kako bi pozvao drugu funkciju
    /// </summary>
    [Command]
    private void CmdUpdateDisplayAfterTabRemoval()
    {
        RpcUpdateTabsAfterDisconnecting();
    }
    /// <summary>
    /// Pozvano od strane servera da se izvrsi na svim klijentima da se updatuje displej
    /// nakon diskonektovanja odredjenog klijenta
    /// </summary>
    [ClientRpc]
    private void RpcUpdateTabsAfterDisconnecting()
    {
        UpdateDisplay();
    }
}
