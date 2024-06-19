using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerControls : NetworkBehaviour
{
    private GameObject selectedObject = null;
    [SerializeField] private Material blue;
    [SerializeField] private Material red;
    [SerializeField] private MyNetworkManager netManager;
    [SerializeField] private UIController uiController;

    private Vector3 blueCamPos = new Vector3(0, 15, -5);
    private Vector3 redCamPos = new Vector3(0, 15, 65);
    /// <summary>
    /// Dictionary used to get which player called Command using connection to client.
    /// Use connectionToClient as key to get which player called the function.
    /// </summary>
    private Dictionary<NetworkConnection, string> players = new Dictionary<NetworkConnection, string>(); 
    private void Awake()
    {
        bool blueAsigned = false;
        netManager = GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>();

        foreach (KeyValuePair<int, NetworkConnectionToClient> con in NetworkServer.connections)
        {
            if (!blueAsigned) 
            { 
                players.Add(con.Value, "blue"); Debug.Log(con.Value);
                blueAsigned = true;
            }
            else players.Add(con.Value, "red"); Debug.Log(con.Value);
        }
        //CmdAdjustCamera();

    }
    private void Update()
    {
        if (!hasAuthority) return;
        SelectBuilding();       
    }
    
    private void SelectBuilding()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo = new RaycastHit();
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
            if (hit)
            {
                if (hitInfo.transform.gameObject.tag == "Neutral")
                {
                    selectedObject = hitInfo.transform.gameObject;
                }
                else
                {
                    selectedObject = null;
                }
            }
            if (selectedObject != null)
            {
                if (!hasAuthority) return;
                CmdOwnBuilding(hitInfo.transform.gameObject);
            }
        }
        
    }

    [Command]
    private void CmdOwnBuilding(GameObject building)
    {
        string matPlayer = players[connectionToClient];
        RpcOwnBuilding(building, matPlayer);
    }
    [ClientRpc]
    private void RpcOwnBuilding(GameObject building, string matPlayer)
    {
        Material mat = matPlayer == "blue" ? blue : red;
        building.GetComponent<Renderer>().material = mat;
    }

    /*private void CmdAdjustCamera()
    {
        Debug.Log(connectionToClient);
        RpcAdjustCamera(players[connectionToClient]);
    }
    [ClientRpc]
    private void RpcAdjustCamera(string playerCam)
    {
        bool blue = playerCam == "blue" ? true : false;
        if (blue)
        {
            Camera.main.transform.position = blueCamPos;
        }
        else
        {
            Camera.main.transform.position = redCamPos;
            Camera.main.transform.rotation = Quaternion.Euler(40, -180, 0);
        }

    }*/

    /*private void MoveSelectedObject()
    {
        if (Input.GetKey(KeyCode.I) && selectedObject != null)
        {
            CmdMoveSelectedObject(selectedObject); 
        }
    }
    [Command]
    private void CmdMoveSelectedObject(GameObject selObject)
    {
        RpcMoveSelectedObject(selObject);
    }
    [ClientRpc]
    private void RpcMoveSelectedObject(GameObject selObject)
    {
        MoveObject(selObject);
    }
    private void SelectObject()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo = new RaycastHit();
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
            if (hit)
            {
                Debug.Log("Hit " + hitInfo.transform.gameObject.name);
                if (hitInfo.transform.gameObject.tag == "Building")
                {
                    selectedObject = hitInfo.transform.gameObject;
                    Debug.Log("It's working!");
                }
                else
                {
                    selectedObject = null;
                    Debug.Log("nopz");
                }
            }
        }
    }
    private void MoveObject(GameObject selObject)
    {
        selObject.transform.position += new Vector3(0, 0, 10f * Time.fixedDeltaTime);
    }*/
}
