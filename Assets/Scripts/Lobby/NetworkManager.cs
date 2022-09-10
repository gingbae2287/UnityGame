using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

using Hashtable = ExitGames.Client.Photon.Hashtable;


public class NetworkManager : MonoBehaviourPunCallbacks
{
    private static NetworkManager instance;
    public static NetworkManager Instance{
        get{
            if(instance==null){
                return null;
            }
            return instance;
        }
    }

    public string gameVersion="1.0";
    byte maxPlayers=10;

    //Current State
    public bool isConnectedToMasterServer{get; private set;}
    public bool isConnectedToLobby{get; private set;}
    public bool isConnectedToRoom{get; private set;}

    bool isConnecting;  
    bool isLogin;
    //connecting to any state (master server, lobby, room)
    //while the value is true, can't connect any state.

    //bool[] playerNumber;

    //====Coroutine====
    IEnumerator cor;

    ///----Lobby Setting=======
    //TypedLobby CustomLobby=new TypedLobby("Custom",LobbyType.Default);
    Hashtable roomCP;
    string roomID="RoomID";
    public string infoMessage="";


    void Awake(){
        if(instance==null){
            instance=this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);
        isLogin=false;

        //--------------------

        PhotonNetwork.AutomaticallySyncScene = true;
    }
    void Start()
    {
        //====state Init====
        StateInit();
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();

        /*cor=ConnectToMasterServer();
        StartCoroutine(cor);*/
    }
    /*IEnumerator ConnectToMasterServer(){
        isConnecting=true;
        while(!isConnectedToMasterServer){
            Debug.Log("Connecting To Master Server....");
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
            yield return new WaitForSeconds(2f);
        }
    }*/

    void StateInit(){
        isConnecting=false;
        isConnectedToMasterServer=false;
        isConnectedToLobby=false;
        isConnectedToRoom=false;
    }
    public void Connect(){
    
        Debug.Log("Try to Connect");
        
        if(PhotonNetwork.IsConnected){
            PhotonNetwork.JoinRandomRoom();
            Debug.Log("Join RandomRoom");
        }
        else{
            Debug.Log("ConnectUsingSetting");
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void BackToLobby(){
        if(isConnecting) return;
        if(!PhotonNetwork.InRoom) return;
        
        if(isConnectedToRoom) 
        {
            isConnecting=true;
            PhotonNetwork.LeaveRoom();
            Debug.Log("Try to Leave room");
        }
        
    }

    public void CreateCustomRoom(){
        
        if(isConnecting) return;
        if(isConnectedToRoom) return;
        isConnecting=true;
        int RandomRoomID=Random.Range(1000,10000);
        RoomOptions roomOption=new RoomOptions();
        roomOption.MaxPlayers=8;
        roomOption.CustomRoomProperties=new Hashtable(){{roomID,RandomRoomID}};
        PhotonNetwork.CreateRoom(RandomRoomID.ToString(), roomOption);
    }

    public void JoinCustomRoom(int RoomID){
        //roomCP=new Hashtable(){{roomID, RoomID}};
        PhotonNetwork.JoinRoom(RoomID.ToString());
    }

//=======CallBack==========
    
    public override void OnConnectedToMaster()
    {
        StateInit();
        //임시 닉네임
        if(!isLogin){
            isLogin=true;
            PhotonNetwork.LocalPlayer.NickName="nick"+Random.Range(1000,10000);
        }
        isConnecting=false;
        isConnectedToMasterServer=true;
        Debug.Log("Success Connecting to Master Server. nickname: "+PhotonNetwork.LocalPlayer.NickName);
        
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("disconnected");
        isConnectedToMasterServer=false;
        isConnecting=true;
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();

    }
    public override void OnJoinedRoom(){
        isConnecting=false;
        isConnectedToRoom=true;
        //if(PhotonNetwork.IsMasterClient) PhotonNetwork.LoadLevel("GlassBridge");
        //PhotonNetwork.LoadLevel("GlassBridge");
       // roomCP=PhotonNetwork.CurrentRoom.CustomProperties;
        //Debug.Log("Join Room. Room Id: "+roomCP[roomID]);
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("JoinRoomFailed");
    }
    public override void OnLeftRoom()
    {
        //마스터 서버로 다시 연결
        isConnectedToRoom=false;
        SceneManager.LoadScene("Lobby");
        //PhotonNetwork.LeaveRoom();
        Debug.Log("Left Room");
    }


    public int GetRoomID(){
        if(!PhotonNetwork.InRoom) return 0;
        roomCP=PhotonNetwork.CurrentRoom.CustomProperties;
        return (int)roomCP[roomID];

    }

}
