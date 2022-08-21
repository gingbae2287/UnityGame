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

    

    public float fallingPoint{get; private set;}
    public Vector3 startPoint{get; private set;}


    void Awake(){
        if(instance==null){
            instance=this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);

    }

    public int playerNumber{get; private set;}

    public void SetPlayerNumber(int PlayerNumber){
        playerNumber=PlayerNumber;
    }
    public void SetFallingPoint(float height){
        fallingPoint=height;
    }
    public void SetStartPoint(Vector3 StartPoint){
        startPoint=StartPoint;
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

