using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoInLobby : MonoBehaviour {
    string playerName;
    int playerNumber;
    [SerializeField] Text info;
    void Start(){
        if(info==null) Debug.LogError("playerinfo text is null");
    }

    public void SetPlayerInfo(string Name, int Number){
        playerName=Name;
        playerNumber=Number;
        info.text=string.Format("{0}. {1}", playerNumber, playerName);
    }

    public int GetPlayerActorNumber(){
        return playerNumber;
    }
    public void ClearInfo(){
        
    }
}