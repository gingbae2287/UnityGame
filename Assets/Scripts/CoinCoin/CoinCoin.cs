using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class CoinCoin : MonoBehaviourPun, IPunObservable{
    private static CoinCoin instance;
    public static CoinCoin Instance{
        get{
            if(instance==null) return null;
            else return instance;
        }
    }
    //==for master===
    [SerializeField] CoinCoinUI ui;
    public bool isGameStart{get;private set;}
    IEnumerator cor;
    int[] playerScores=new int[8];  //max player need
    int timer, gameTime=30;
    Hashtable hash;
    //==for Players==
    public int myScore{get; private set;}
    int playerNum, bestPlayer, mapSize=30;
    void Awake(){
        if(instance==null){
            instance=this;
        }
        else if(instance!=this){
            Destroy(instance.gameObject);
            instance=this;
        }
        hash=new Hashtable();
        hash.Add("isSpawn",true);
        hash.Add("playerNumber",0);
    }
    void Start(){
        if(ui==null) Debug.LogError("coincoinui is null");
        cor=GameState();
        StartCoroutine(cor);
    }
    public void GetScore(int Score){
        myScore+=Score;
        ui.UpdateScore(myScore);
        photonView.RPC("UpdateScore",RpcTarget.AllViaServer,playerNum, myScore);
    }
    IEnumerator GameState(){
        while(!CoinManager.Instance.coinCreated){
            yield return new WaitForSeconds(0.1f);
        }
        SetGameSetting();
        photonView.RPC("CoinCoinReady",RpcTarget.AllViaServer);
        while(!CheckAllPlayerReady()){
            yield return new WaitForSeconds(0.1f);
        }
        timer=3;
        string str="";
        while(timer>0){
            str=timer.ToString();
            ui.SetNotification(str);
            yield return new WaitForSeconds(1f);
            timer--;
        }
        photonView.RPC("CoinCoinStart",RpcTarget.AllViaServer);
        yield return new WaitForSeconds(1f);
        while(timer>0){
            photonView.RPC("CoinCoinTimer",RpcTarget.AllViaServer,timer);
            yield return new WaitForSeconds(1f);
            timer--;
        }
        photonView.RPC("CoinCoinTimer",RpcTarget.AllViaServer,timer);
        
    }
    [PunRPC]
    void CoinCoinReady(){
        SpawnPlayer();
    }
    [PunRPC]
    void CoinCoinStart(){
        playerNum=(int)PhotonNetwork.LocalPlayer.CustomProperties["playerNumber"];
        Player.LocalPlayerInstance.canMove=true;
        isGameStart=true;
        timer=gameTime;
        ui.SetNotification("Game Start!");
    }
    [PunRPC]
    void CoinCoinTimer(int Timer){
        if(!PhotonNetwork.IsMasterClient) timer=Timer;
        ui.SetNotification("");
        ui.Timer(Timer);
        if(Timer<=0){
            ui.SetNotification("Game End!");
            CoinCoinEnd();
        }
    }
    [PunRPC]
    void UpdateScore(int PlayerNumber, int newScore){
        playerScores[PlayerNumber]=newScore;
        if(playerScores[bestPlayer]<= playerScores[PlayerNumber]) {
            bestPlayer=PlayerNumber;
            ui.UpdateBestScore(playerScores[PlayerNumber]);
        }
    }


    void SpawnPlayer(){
        if(Player.LocalPlayerInstance!=null) return;
       // startPoint=new Vector3(maxPlayer*0.5f-playerNumber, 0, 0);
        int x=Random.Range(0,mapSize);
        int z=Random.Range(0,mapSize);
        PhotonNetwork.Instantiate("Character/TT_male",new Vector3(x, 1,z),Quaternion.identity);
        Player.LocalPlayerInstance.GameSettingForPlayer(Vector3.zero);
        Player.LocalPlayerInstance.StopMove();
        //Hashtable hash=new Hashtable();
        hash["isSpawn"]=true;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        GameManager.Instance.LoadingProgressFinish();
    }
    void SetGameSetting(){
        if(!PhotonNetwork.IsMasterClient) return;
        
        for(int i=0;i<PhotonNetwork.PlayerList.Length;i++){
            
            
            hash["playerNumber"]=i;
            PhotonNetwork.PlayerList[i].SetCustomProperties(hash);
        }
    }
    bool CheckAllPlayerReady(){
        bool checkPlayerSpawn=true;
        int currentPlayers=PhotonNetwork.PlayerList.Length;
        for(int i=0;i<currentPlayers;i++){
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
    void CoinCoinEnd(){
        Player.LocalPlayerInstance.StopMove();
        isGameStart=false;
        if(bestPlayer==playerNum) ui.SetNotification("You Win!");
        //점수 비교 1등 산출
        if(!PhotonNetwork.IsMasterClient) return;

    }

    

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        /*
        if(!isGameStart) return;
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            stream.SendNext(timer);
        }

        else
        {
           timer = (int)stream.ReceiveNext();
        }
        */
    }
}