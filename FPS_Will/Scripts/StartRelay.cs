using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class StartRelay : MonoBehaviour
{
    private readonly relay _relay = new relay();

    private async void Start()
    {
        var token = "b2636e53-0a44-479a-9143-22e217ae3ee0";
        await _relay.SendRequest(token);
    }

}


