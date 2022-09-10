using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;



public class CustomRoom : MonoBehaviourPunCallbacks
{
    [SerializeField] string[] gameList;
    [SerializeField] GameObject playerInfo;
    [SerializeField] GameObject mainLobbyObject;
    [SerializeField] Text roomNumber;
    [SerializeField] Text gameName;
    //[SerializeField] PlayerInfoInLobby myInfo;
    [SerializeField] GameObject scrollViewContent;
    [SerializeField] GameObject[] masterButtonObjs;

    float offsetX=250f, offsetY=50f, posX=-121.5f, posY=120;
    List<PlayerInfoInLobby> playerList;
    GameObject[] playerInfoCards;
    Vector2 myInfoPosition;
    Vector2[] infoPrefabsPositions;
    IEnumerator cor;
    int maxPlayers;
    int currentGame;


    void Awake(){
        playerList= new List<PlayerInfoInLobby>();
        maxPlayers=PhotonNetwork.CurrentRoom.MaxPlayers;
        infoPrefabsPositions=new Vector2[maxPlayers];
        myInfoPosition= new Vector2(posX,posY);
        //Infoobject transform. don't use grid layout
        for(int i=0; i<maxPlayers;i++){
            infoPrefabsPositions[i]=new Vector2(myInfoPosition.x+i%2*offsetX, myInfoPosition.y-i/2*offsetY);
        }
        CreatePlayerCard();

    }
    new void OnEnable(){
        PhotonNetwork.AddCallbackTarget(this);
        RenewalRoom();
    }
    new void OnDisable() {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    /*override public void OnEnable() {
        PhotonNetwork.AddCallbackTarget(this);
        RenewalRoom();
    }
    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    */
    void Start(){
        if(mainLobbyObject==null) Debug.LogError("mainLobbyObj is null");
        if(roomNumber==null) Debug.LogError("roomNumber is null");
        if(gameName==null) Debug.LogError("gameName is null");
        if(playerInfo==null) Debug.LogError("playerInfoPrefab is null");
        if(scrollViewContent==null) Debug.LogError("scrollViewContent is null");
        //if(startButtonObj==null) Debug.LogError("startButtonObj is null");

        
    }
    

    void CreatePlayerCard(){
        playerInfoCards=new GameObject[maxPlayers];
        for(int i=0;i<maxPlayers;i++){
            playerInfoCards[i]=Instantiate(playerInfo, infoPrefabsPositions[i], Quaternion.identity);
            playerInfoCards[i].transform.SetParent(scrollViewContent.transform,false);
            playerList.Add(playerInfoCards[i].GetComponent<PlayerInfoInLobby>());
            playerInfoCards[i].SetActive(false);
        }
    }

    void ClearPlayerList(){
        for(int i=0;i<maxPlayers;i++){
            playerInfoCards[i].SetActive(false);
        }
    }

    public void RenewalRoom(){

        ClearPlayerList();
        if(PhotonNetwork.IsMasterClient) {
            foreach(GameObject obj in masterButtonObjs){
                obj.SetActive(true);
            }
            currentGame=0;
            photonView.RPC("SetGameName", RpcTarget.AllBufferedViaServer, currentGame);
        }
        else foreach(GameObject obj in masterButtonObjs){
                obj.SetActive(false);
            }
        roomNumber.text="Room ID : "+NetworkManager.Instance.GetRoomID();

        for(int i=0;i<PhotonNetwork.PlayerList.Length;i++){
            playerInfoCards[i].SetActive(true);
            playerList[i].SetPlayerInfo(PhotonNetwork.PlayerList[i].NickName, PhotonNetwork.PlayerList[i].ActorNumber);
        }

    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {  
        int playerCount=PhotonNetwork.PlayerList.Length;
        playerInfoCards[playerCount-1].SetActive(true);
        playerList[playerCount-1].SetPlayerInfo(newPlayer.NickName, newPlayer.ActorNumber);
        if(PhotonNetwork.IsMasterClient) photonView.RPC("SetGameName", RpcTarget.AllViaServer, currentGame);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        RenewalRoom();
    }
    public override void OnLeftRoom()
    {
    
    }
//====Button=========
    public void BackButton(){
        NetworkManager.Instance.BackToLobby();
        mainLobbyObject.SetActive(true);
        gameObject.SetActive(false);
    }
    public void ButtonNextGame(){
        if(!PhotonNetwork.IsMasterClient) return;
        currentGame++;
        if(currentGame==gameList.Length) currentGame=0;
        photonView.RPC("SetGameName", RpcTarget.AllViaServer, currentGame);
    }
    public void ButtonPrevGame(){
        if(!PhotonNetwork.IsMasterClient) return;
        currentGame--;
        if(currentGame<0) currentGame=gameList.Length-1;
        photonView.RPC("SetGameName", RpcTarget.AllViaServer, currentGame);
        
    }
    [PunRPC]
    void SetGameName(int gameNum){
        gameName.text=gameList[gameNum];
    }
    public void StartButton(){
        PhotonNetwork.LoadLevel(gameList[currentGame]);
    }
}