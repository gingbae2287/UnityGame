using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

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
        titleList= new List<KeyValuePair<string,string>>();
        VoteInit();
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
                            Debug.Log(category.Key+" "+ title.Value);
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
        if(Player.LocalPlayerInstance==null) {
            SpawnPlayer();
        }
        if(PhotonNetwork.IsMasterClient){
            cor=GameReadyCheck();
            StartCoroutine(cor);
        }
        
    }




    void SpawnPlayer(){
        for(int i=0;i<PhotonNetwork.PlayerList.Length;i++){
            if(PhotonNetwork.LocalPlayer==PhotonNetwork.PlayerList[i]){
                playerNumber=i;
                break;
            }
        }
        startPoint=new Vector3(maxPlayer*0.5f-playerNumber, 0, 0);
       PhotonNetwork.Instantiate("Character/TT_male",startPoint,Quaternion.identity);
       Player.LocalPlayerInstance.GameSettingForPlayer(startPoint);

        object[] data= new object[1];
        data[0]=playerNumber;
        //votezone
        Vector3 zonePos=VoteZonePosition+(new Vector3(6*(playerNumber%4),0,(-19)*(playerNumber/4)));
        PhotonNetwork.Instantiate(VoteZoneName,zonePos, Quaternion.identity,0,data);
    }

    void VoteInit(){
        isVote=false;
        if(!PhotonNetwork.IsMasterClient) return;
        for(int i=0;i<maxPlayer;i++){
            votePlayer[i]=-1;
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
}

