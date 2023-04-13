//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Mirror;
//public class Player : NetworkBehaviour
//{
//    public static Player localPlayer;

//    void Start()
//    {
//        if (isLocalPlayer)
//        {
//            localPlayer = this;
//        }
//    }
//    public void HostGame()
//    {
//        //Generate a new match ID for every new Hosting
//        string matchID = MatchMaker.GetRandomMatchID();
//        CmdHostGame(matchID);
//    }
//    [Command]
//    void CmdHostGame(string _matchID)
//    {
//        if (MatchMaker.instance.HostGame(_matchID, gameObject))
//        {
//            Debug.Log($"<color = green>Game hosted Sucessfully</color>");

//        }
//        else
//        {
//            Debug.Log($"<color = red>Game hosted faild</color>");
//        }
//    }
//}
