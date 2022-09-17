using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ColorWar : MonoBehaviourPunCallbacks, IPunObservable{
    private static ColorWar instance;
    public static ColorWar Instance{
        get{
            if(instance==null) return null;
            else return instance;
        }
    }
    enum TeamName{
        Yellow,
        Blue,
        Green,
        Red
    }
    //==game state======
    enum GameState{ //room's custom property
        Start,      //0 start game room
        SetTeam,    //1 finished player's team
        GameReady,  //2 all player spawned and start count
        Gaming,     //3 gaming
        GameEnd, 
    }
    GameState gameState;
    Hashtable stateHash;
    //==========
    TeamName teamName;
    [SerializeField] ColorWarUI ui;
    [SerializeField] Transform spawnPoint;
    [SerializeField] GameObject[] readyWall;
    //GameObject[] blocks;
    int blockSize=2;
    string blockPath="ColorWar/Block";
    //===for Master====
    int currentPlayer, timer,sendTimer, gameTime=30, WinnerTeam;
    string notif;
    public int teamSize{get; private set;}
    IEnumerator cor;
    public bool isGameStart{get; private set;}
    bool isBlockCreated, isGameReady;
    int[] teamScore=new int[4];
    List<int>[] teamPlayer=new List<int>[4];    //팀별 플레이어 번호 리스트

    //==for players====
    public int teamNumber{get; private set;}
    public int playerNumber{get; private set;}
    void Awake(){
        if(instance==null){
            instance=this;
        }
        else if(instance!=this){
            Destroy(instance.gameObject);
            instance=this;
        }
        //blocks=new GameObject[100];
        stateHash=new Hashtable();
    }
    private void Start() {
        if(!PhotonNetwork.IsMasterClient) return;
        gameState=GameState.Start;
        stateHash["gameState"]=gameState;
        PhotonNetwork.CurrentRoom.SetCustomProperties(stateHash);
        cor=GameStart();
        StartCoroutine(cor);
    }

    void CreateBlocks(){
        if(!PhotonNetwork.IsMasterClient) return;
        
        for(int i=0;i<25;i++){
            //yellow zone
                object[] data= new object[2];
                data[0]=0;
                data[1]=i;
                GetScore(0);
                Vector3 pos=new Vector3(i%5*blockSize, 0, i/5*blockSize);
                PhotonNetwork.InstantiateRoomObject(blockPath,pos,Quaternion.identity,0,data);
        }
        for(int i=25;i<50;i++){
            //blue zone
                object[] data= new object[2];
                data[0]=1;
                data[1]=i;
                GetScore(1);
                Vector3 pos=new Vector3(i%5*blockSize, 0, i/5*blockSize);
                PhotonNetwork.InstantiateRoomObject(blockPath,pos,Quaternion.identity,0,data);
        }
        for(int i=50;i<75;i++){
            //green zone
                object[] data= new object[2];
                data[0]=2;
                data[1]=i;
                GetScore(2);
                Vector3 pos=new Vector3((5+i%5)*blockSize, 0, (i/5-10)*blockSize);
                PhotonNetwork.InstantiateRoomObject(blockPath,pos,Quaternion.identity,0,data);
        }
        for(int i=75;i<100;i++){
            //red zone
                object[] data= new object[2];
                data[0]=3;
                data[1]=i;
                GetScore(3);
                Vector3 pos=new Vector3((5+i%5)*blockSize, 0, (i/5-10)*blockSize);
                PhotonNetwork.InstantiateRoomObject(blockPath,pos,Quaternion.identity,0,data);
        }
        isBlockCreated=true;
    }
    IEnumerator GameStart(){
        //CreateBlocks();
        while(true){
        switch(gameState){
            case GameState.Start:
                if(isGameReady) break;
                SetTeam();
                while(!isGameReady) yield return new WaitForSeconds(0.1f);
                break;
            case GameState.SetTeam:
                photonView.RPC("ColorWarReady",RpcTarget.AllViaServer, teamSize);
                while(!CheckAllPlayerReady()) yield return new WaitForSeconds(0.1f);
                gameState=GameState.GameReady;
                stateHash["gameState"]=gameState;
                PhotonNetwork.CurrentRoom.SetCustomProperties(stateHash);
                timer=3;
                break;
            case GameState.GameReady:
                while(timer>0){
                    notif=timer+"초 후 게임을 시작합니다.";
                    photonView.RPC("ColorWarNotification", RpcTarget.AllViaServer, notif);
                    timer--;
                    yield return new WaitForSeconds(1f);
                }
                photonView.RPC("ColorWarStart",RpcTarget.AllViaServer);

                gameState=GameState.Gaming;
                stateHash["gameState"]=gameState;
                PhotonNetwork.CurrentRoom.SetCustomProperties(stateHash);
                timer=gameTime;
                break;
            case GameState.Gaming:
                while(timer>0){
                    notif=timer.ToString();
                    photonView.RPC("ColorWarNotification", RpcTarget.AllViaServer, notif);
                    timer--;
                    yield return new WaitForSeconds(1f);
                }
                ColorWarEnd();
                break;
            case GameState.GameEnd:
                StopCoroutine(cor);
                break;
        }
        yield return new WaitForSeconds(0.1f);
        }
    }
    //====================================

    [PunRPC]
    void ColorWarReady(int TeamSize){
        teamSize=TeamSize;
        SpawnPlayer();
    }
    [PunRPC]
    void ColorWarStart(){
        notif="게임 시작!";
        ui.SetNotification(notif);
        Player.LocalPlayerInstance.canMove=true;
        isGameStart=true;
        foreach(GameObject wall in readyWall) wall.SetActive(false);
    }
    [PunRPC]
    void ColorWarNotification(string str){
        ui.SetNotification(str);
    }
    [PunRPC]
    void ColorWarGameEnd(){
        Player.LocalPlayerInstance.StopMove();
        isGameStart=false;
    }
    void SpawnPlayer(){
        teamNumber=(int)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
        if(Player.LocalPlayerInstance!=null) return;
       // startPoint=new Vector3(maxPlayer*0.5f-playerNumber, 0, 0);
        //photonView.ViewID
        Vector3 pos=new Vector3(-4+photonView.ViewID%5*2,0,-4+photonView.ViewID/5*2);
        pos=spawnPoint.position+pos;
        PhotonNetwork.Instantiate("Character/TT_male",pos,Quaternion.identity);
        Player.LocalPlayerInstance.GameSettingForPlayer(pos);
        Player.LocalPlayerInstance.StopMove();
        Hashtable hash=new Hashtable();
        hash.Add("isSpawn",true);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        //로딩화면 
        GameManager.Instance.LoadingProgressFinish();
    }

    void SetTeam(){
        if(!PhotonNetwork.IsMasterClient) return;
        currentPlayer=PhotonNetwork.PlayerList.Length;
        for(int i=4;i>0;i--) 
            if(currentPlayer%i==0) 
            {
                teamSize=i;
                break;
            }
        if(teamSize==1) Debug.LogError("팀이 안맞음");
        
        int[] arr=new int[currentPlayer];
        for(int i=0;i<currentPlayer;i++){
            arr[i]=i;
        }
        //팀배치 순서 랜덤 섞기
        for(int i=0;i<currentPlayer;i++){
            int rand=Random.Range(0,currentPlayer);
            int tmp=arr[rand];
            arr[rand]=arr[i];
            arr[i]=tmp;
        }
        for(int i=0;i<currentPlayer;i++){
            Hashtable hash=new Hashtable();
            int team=arr[i]%teamSize;
            hash.Add("Team",team);
            //teamPlayer[team].Add(i);
            PhotonNetwork.PlayerList[i].SetCustomProperties(hash);
        }
        isGameReady=true;
        gameState=GameState.SetTeam;
        stateHash["gameState"]=gameState;
        PhotonNetwork.CurrentRoom.SetCustomProperties(stateHash);
    }
    bool CheckAllPlayerReady(){
        bool checkPlayerSpawn=true;
        for(int i=0;i<PhotonNetwork.PlayerList.Length;i++){
            if(!PhotonNetwork.PlayerList[i].CustomProperties.ContainsKey("isSpawn")){
                checkPlayerSpawn=false;
                break;
            }
            else if(!(bool)PhotonNetwork.PlayerList[i].CustomProperties.ContainsKey("isSpawn")){
                checkPlayerSpawn=false;
                break;
            }
        }
        return checkPlayerSpawn;
    }
    public void GetScore(int team, int value=1){
        if(!PhotonNetwork.IsMasterClient) return;
        teamScore[team]+=value;
        photonView.RPC("ColorWarScore", RpcTarget.AllViaServer, teamScore);
    }

    [PunRPC]
    void ColorWarScore(int[] Score){
        if(!PhotonNetwork.IsMasterClient){
            for(int i=0;i<Score.Length;i++){
                teamScore[i]=Score[i];
            }
        }
        ui.UpdateScore(teamScore);
    }
    void ColorWarEnd(){
        photonView.RPC("ColorWarGameEnd", RpcTarget.AllViaServer);
        WinnerTeam=CheckWinnerTeam();
        notif="게임종료\n"+(TeamName)(WinnerTeam)+"팀 승리!";
        photonView.RPC("ColorWarNotification", RpcTarget.AllViaServer, notif);
        gameState=GameState.Gaming;
        stateHash["gameState"]=gameState;
        PhotonNetwork.CurrentRoom.SetCustomProperties(stateHash);
    }
    int CheckWinnerTeam(){
        int WinnerTeam=-1, MaxScore=0;
        for(int i=0;i<teamSize;i++){
            if(teamScore[i]>MaxScore) {
                MaxScore=teamScore[i];
                WinnerTeam=i;
            }
            else if(teamScore[i]==MaxScore){
                //동점자 처리
            }
        }
        return WinnerTeam;
    }

   public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(!isGameStart) return;
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            if(sendTimer!=timer){
                sendTimer=timer;
                stream.SendNext(sendTimer);
            }
        }

        else
        {
           timer = (int)stream.ReceiveNext();
        }
    }
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient){
        if(!PhotonNetwork.IsMasterClient) return;
        gameState=(GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"];
        cor=GameStart();
        StartCoroutine(cor);
    }
}