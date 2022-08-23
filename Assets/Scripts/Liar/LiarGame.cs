using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class LiarGame : MonoBehaviourPun
{
    //Vector3[] startPoint;

    private static LiarGame instance;
    public static LiarGame Instance{
        get{
            if(instance==null) return null;
            else return instance;
        }
    }
    //==obejct==
    [SerializeField] Text categoryObj;
    [SerializeField] Text titleObj;

    //===game setting==
    int maxPlayer=8, minPlayer=4;
    float fallingPoint=-5f;
    int[] votePlayer;   //idx: playernumber, value: voted playernumber
    string VoteZoneName="Liar/VoteZone";
    Vector3 VoteZonePosition;
    DatabaseReference DBTitle;
    List<KeyValuePair<string,string>> titleList;
    string category;
    string title;
    bool isGameReady;
    IEnumerator cor;
    int[] playerOrder;
    
    //===player setting

    public int playerNumber{get; private set;}
    public bool isVote;
    Vector3 startPoint;

    void Awake(){
        if(instance==null){
            instance=this;
        }
        else if(instance!=this){
            Destroy(instance.gameObject);
            instance=this;
        }
        maxPlayer=GameManager.Instance.maxPlayerOfLiarGame;
        minPlayer=GameManager.Instance.minPlayerOfLiarGame;
        votePlayer=new int[maxPlayer];
        playerOrder=new int[maxPlayer];

        titleList= new List<KeyValuePair<string,string>>();
        InitValue();
        isGameReady=false;
        
        GetDB();
       
        
    }
    //DB for title
    void GetDB(){
        if(PhotonNetwork.IsMasterClient){
            DBTitle=FirebaseDatabase.DefaultInstance.GetReference("LiarGameTitle");
            DBTitle.GetValueAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted) {
                    Debug.LogError("error GetValueAsync()");
                    return;
                }
                else if (task.IsCompleted) {
                    DataSnapshot snapshot=task.Result;
                    foreach(DataSnapshot category in snapshot.Children){
                        foreach(DataSnapshot title in category.Children){
                            titleList.Add(new KeyValuePair<string, string>
                                    (category.Key,(string)title.Value)
                                
                            );
                        }
                    }
                    isGameReady=true;

                }
            });
        }
    }
    void Start(){
        VoteZonePosition=new Vector3(-9, 0, 9.5f);
        RandomPlayerNumber();
        if(PhotonNetwork.IsMasterClient){
            cor=GameReadyCheck();
            StartCoroutine(cor);
        }
        
    }



    void RandomPlayerNumber(){
        if(!PhotonNetwork.IsMasterClient) return;
        Debug.Log("random player num");
        int currentPlayers=PhotonNetwork.PlayerList.Length;
        for(int i=0;i<currentPlayers;i++){
            int tmpPlayer=playerOrder[i];
            int randomNum=Random.Range(0,currentPlayers);
            playerOrder[i]=playerOrder[randomNum];
            playerOrder[randomNum]=tmpPlayer;
            
        }
        for(int i=0;i<currentPlayers;i++){
            Hashtable hash =new Hashtable();
            hash.Add("playerNumber",i);
            PhotonNetwork.PlayerList[playerOrder[i]].SetCustomProperties(hash);
        }
        photonView.RPC("SpawnPlayer", RpcTarget.AllBufferedViaServer);
        
    }
    [PunRPC]
    void SpawnPlayer(){

        if(Player.LocalPlayerInstance!=null) return;
        playerNumber=(int)PhotonNetwork.LocalPlayer.CustomProperties["playerNumber"];
        Debug.Log("playernumber: "+playerNumber);
        startPoint=new Vector3(maxPlayer*0.5f-playerNumber, 0, 0);
        PhotonNetwork.Instantiate("Character/TT_male",startPoint,Quaternion.identity);
        Player.LocalPlayerInstance.GameSettingForPlayer(startPoint);

        object[] data= new object[1];
        data[0]=playerNumber;
        //votezone
        Vector3 zonePos=VoteZonePosition+(new Vector3(6*(playerNumber%4),0,(-19)*(playerNumber/4)));
        PhotonNetwork.Instantiate(VoteZoneName,zonePos, Quaternion.identity,0,data);
    }
    

    void InitValue(){
        isVote=false;
        int currentPlayers=PhotonNetwork.PlayerList.Length;
        if(!PhotonNetwork.IsMasterClient) return;
        for(int i=0;i<maxPlayer;i++){
            votePlayer[i]=-1;
        }
        for(int i=0;i<maxPlayer;i++){
            if(i<currentPlayers){
                playerOrder[i]=i;
            }
            else playerOrder[i]=-1;
        }
    }

    public void VotePlayer(int PlayerNumber, int VoteNumber){
        votePlayer[PlayerNumber]=VoteNumber;
    }

    

    void GameStart(){
        if(!PhotonNetwork.IsMasterClient) return;
        int idx=Random.Range(0,titleList.Count);
        category=titleList[idx].Key;
        title=titleList[idx].Value;
        photonView.RPC("GameStartForPlayers",RpcTarget.AllBufferedViaServer,category,title);

    }
    IEnumerator GameReadyCheck(){
        while(!isGameReady){
            yield return new WaitForSeconds(0.1f);
        }
        GameStart();
    }
    [PunRPC]
    void GameStartForPlayers(string Category, string Title){
        category=Category;
        title=Title;
        categoryObj.text="카테고리: "+category;
        titleObj.text="주제: "+title;
    }

    public void LobbyButton(){
        GameManager.Instance.LeftRoom();
    }
}

