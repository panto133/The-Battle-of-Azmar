using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MyNetworkGamePlayer : NetworkBehaviour
{
    [SyncVar]
    private string displayName = "Loading...";

    private MyNetworkManager room;

    private MyNetworkManager Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as MyNetworkManager;
        }
    }

    private bool isLeader;
    public bool IsLeader
    {
        set
        {
            isLeader = value;
        }
    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);
        Room.GamePlayers.Add(this);
    }

    public override void OnStopClient()
    {
        Room.GamePlayers.Remove(this);
    }

    [Server]
    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }


}
