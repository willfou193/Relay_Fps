using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MyNetworkManager : NetworkManager
{
    // Paste your API token here
    //private string apiToken = "de5ff38d-de97-4233-ac3a-7b670ce2be31";

    public override void OnStartServer()
    {
        Debug.Log("Server Started");
    }
    //public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    //{

    //}
    public override void Start()
    {
        //base.Start();
        // Set the API token in the NetworkManager singleton
        //NetworkManager.singleton.networkAddress = apiToken;
    }

}

