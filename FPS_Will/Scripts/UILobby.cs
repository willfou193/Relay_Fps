using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class UILobby : NetworkBehaviour
{
    [SerializeField] InputField JoinMatchInput;
    [SerializeField] Button joinButton;
    [SerializeField] Button hostButtun;

    private void Start()
    {
        //Update the canvas text if you have manually changed network managers address from the game object before starting the game scene
        if (NetworkManager.singleton.networkAddress != "localhost") { JoinMatchInput.text = NetworkManager.singleton.networkAddress; }

        //Adds a listener to the main input field and invokes a method when the value changes.
        JoinMatchInput.onValueChanged.AddListener(delegate { ValueChangeCheck(); });

        //This updates the Unity canvas, we have to manually call it every change, unlike legacy OnGUI.
    }
    void ValueChangeCheck()
    {
        NetworkManager.singleton.networkAddress = JoinMatchInput.text;
    }
    public void Host()
    {
        NetworkManager.singleton.StartHost();
    }
    public void Join()
    {
        NetworkManager.singleton.StartClient();
    }
}
