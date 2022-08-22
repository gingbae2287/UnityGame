using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks {
    private static GameManager instance;
    public static GameManager Instance{
        get{
            if(instance==null) return null;
            return instance;
        }
    }
    //===GlassBridge======
    public int maxPlayerOfGlassBridge=12;
    public int minPlayerOfGlassBridge=3;

    //===LiarGame==========

    public int maxPlayerOfLiarGame=8;
    public int minPlayerOfLiarGame=4;




    void Awake(){
        if(instance==null){
            instance=this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);

    }
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void LeftRoom(){
        //PhotonNetwork.LeaveRoom();
        PhotonNetwork.Disconnect();
    }
    
}

