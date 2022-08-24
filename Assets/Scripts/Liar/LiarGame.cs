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
    [SerializeField] LiarUI liarUI;

    //===game setting==
    int maxPlayer=8, minPlayer=4;
    float fallingPoint=-5f;
    string VoteZoneName="Liar/VoteZone";
    Vector3 VoteZonePosition;
    string category;
    string title;
    IEnumerator cor;
    //===for masterClient====
    int[] playerOrder;
    int currentOrder, timer;
    bool isGameReady, isAnswerTime;
    DatabaseReference DBTitle;
    List<KeyValuePair<string,string>> titleList;
    int[] votePlayer;   //idx: playernumber, value: voted playernumber
    int[] voteCount;    //how many count voted for other players
    
    //===player setting

    public int playerNumber{get; private set;}
    public bool isVote;
    public bool isLiar{get; private set;}
    int liarPlayer;
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
        voteCount=new int[maxPlayer];
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
    void InitValue(){
        isVote=false;
        isAnswerTime=false;
        liarPlayer=0;
        int currentPlayers=PhotonNetwork.PlayerList.Length;
        
        if(!PhotonNetwork.IsMasterClient) return;
        currentOrder=0;
        for(int i=0;i<maxPlayer;i++){
            votePlayer[i]=-1;
            voteCount[i]=0;
        }
        for(int i=0;i<maxPlayer;i++){
            if(i<currentPlayers){
                playerOrder[i]=i;
            }
            else playerOrder[i]=-1;
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
        int currentPlayers=PhotonNetwork.PlayerList.Length;
        for(int i=0;i<currentPlayers;i++){
            int tmpPlayer=playerOrder[i];
            int randomNum=Random.Range(0,currentPlayers);
            playerOrder[i]=playerOrder[randomNum];
            playerOrder[randomNum]=tmpPlayer;
            
        }
        liarPlayer=Random.Range(0,currentPlayers);
        for(int i=0;i<currentPlayers;i++){
            Hashtable hash =new Hashtable();
            hash.Add("playerNumber",i);
            if(i==liarPlayer) hash.Add("isLiar", true);
            else hash.Add("isLiar", false);
            PhotonNetwork.PlayerList[playerOrder[i]].SetCustomProperties(hash);
        }
        photonView.RPC("SpawnPlayer", RpcTarget.AllBufferedViaServer);
        
    }
    [PunRPC]
    void SpawnPlayer(){
        playerNumber=(int)PhotonNetwork.LocalPlayer.CustomProperties["playerNumber"];
        isLiar=(bool)PhotonNetwork.LocalPlayer.CustomProperties["isLiar"];
        if(Player.LocalPlayerInstance!=null) return;
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
    

    

    public void VotePlayer(int PlayerNumber, int VoteNumber){
        if(!PhotonNetwork.IsMasterClient) return;
        votePlayer[PlayerNumber]=VoteNumber;
    }

    

    void GameStart(){
        if(!PhotonNetwork.IsMasterClient) return;
        int idx=Random.Range(0,titleList.Count);
        category=titleList[idx].Key;
        title=titleList[idx].Value;
        photonView.RPC("SetTitleText",RpcTarget.AllBufferedViaServer,category,title);
        Debug.Log("주제어 재설정");
    }
    IEnumerator GameReadyCheck(){
        while(!isGameReady){
            yield return new WaitForSeconds(0.1f);
        }
        GameStart();
        int i=3;
        string str;
        while(i>0){
            str=i+"초 후 게임이 시작됩니다.";
            photonView.RPC("AnnouncementRPC",RpcTarget.AllBufferedViaServer,str);
            yield return new WaitForSeconds(1f);
            i--;
        }
        str="Game Start!";
        photonView.RPC("AnnouncementRPC",RpcTarget.AllBufferedViaServer,str);
        yield return new WaitForSeconds(1f);
        photonView.RPC("NextPlayerTurn",RpcTarget.AllViaServer,currentOrder);
    }
    [PunRPC]
    void SetTitleText(string Category, string Title){
        liarUI.SetTitleText(Category,Title,isLiar);
    }
    [PunRPC]
    void AnnouncementRPC(string str){
        liarUI.SetAnnouncement(str);
    }
    [PunRPC]
    void NextPlayerTurn(int CurrentOrder){
        liarUI.ClearAnnouncment();
        string str;
        if(playerNumber==CurrentOrder){
            
            if(isLiar) {
                str="제시어를 추측하세요.\n";
                str+="라이어임을 들키지 않게 제시어에 관한 말을 하세요.";
            }
            else str="제시어에 관해 하고싶은 말을 제출하세요.";
            liarUI.PlayerTurn(str);
        }
        if(PhotonNetwork.IsMasterClient){
            cor=TurnTimer();
            StartCoroutine(cor);
        }
    }
    [PunRPC]
    void TimerRPC(int CurrentOrder, int sec){
        string str;
        if(playerNumber!=CurrentOrder)
            str=(CurrentOrder+1)+"번 플레이어 차례입니다.\n남은시간: "+sec+"초";
        else 
        {
            if(sec<=0) SubmitExplanationButton();
            str="남은시간: "+sec+"초";
        }
        liarUI.SetAnnouncement(str);
    }

    IEnumerator TurnTimer()
    {   
        //for master client
        int i=30;
        while(i>=0){
            string str="제한시간 "+i+"초";
            photonView.RPC("TimerRPC",RpcTarget.AllBufferedViaServer,currentOrder, i);
            yield return new WaitForSeconds(1f);
            i--;
        }

    }
    public void SubmitExplanationButton(){
        string str=liarUI.GetExplanation();
        if(!isAnswerTime){
            photonView.RPC("ReadExplanation", RpcTarget.AllViaServer,playerNumber, str);
            liarUI.ClearAnnouncment();
        }
        else{
            photonView.RPC("CheckAnswer", RpcTarget.MasterClient,str);
        }
        
    }
    [PunRPC]
    void ReadExplanation(int PlayerNumber, string ex){
        liarUI.AddHistory(PlayerNumber, ex);
        if(PhotonNetwork.IsMasterClient) 
        {
            currentOrder++;
            //cor=TurnTimer();
            StopCoroutine(cor);
            if(currentOrder>=PhotonNetwork.PlayerList.Length) 
            {       
                currentOrder=0;
                //투표 타임
                cor=VoteTimer();
                StartCoroutine(cor);
                return;
            }
            photonView.RPC("NextPlayerTurn",RpcTarget.AllViaServer,currentOrder);
            
        }
    }

    IEnumerator VoteTimer()
    {   
        //for master client
        int i=10;
        string str;
        while(i>=0){
            str="라이어로 생각되는 플레이어의 발판에 들어가 투표하세요.\n";
            str+="남은시간 "+i+"초";
            photonView.RPC("AnnouncementRPC",RpcTarget.AllBufferedViaServer,str);
            yield return new WaitForSeconds(1f);
            i--;
        }
        VoteCheck();
    }

    void VoteCheck(){
        if(!PhotonNetwork.IsMasterClient) return;
        int maxCount=-1, maxCountPlayer=-1, sameCountPlayer=-1;
        for(int i=0;i<maxPlayer;i++){
            if(votePlayer[i]<0) continue;
            voteCount[votePlayer[i]]++;
            if(maxCount<voteCount[votePlayer[i]]){
                maxCount=voteCount[votePlayer[i]];
                maxCountPlayer=votePlayer[i];
                sameCountPlayer=-1;
            }
            else if(maxCount==voteCount[votePlayer[i]])
            {
                sameCountPlayer=votePlayer[i];
            }
        }
        string str;
        if(maxCountPlayer==liarPlayer){
            str=string.Format("최다 득표자 {0}번은 라이어였습니다.\n 라이어가 정답을 맞추는 중입니다.",maxCountPlayer+1);
            photonView.RPC("LiarAnswerTime",RpcTarget.AllBufferedViaServer);

        }
        else{
            str=string.Format("최다 득표자 {0}번은 라이어가 아닙니다.\n라이어는 {1}번 입니다.",maxCountPlayer+1, liarPlayer+1);
            cor=WaitTimer();
            StartCoroutine(cor);
        }
        photonView.RPC("AnnouncementRPC",RpcTarget.AllBufferedViaServer,str);
    }
    [PunRPC]
    void LiarAnswerTime(){
        isAnswerTime=true;
        if(!isLiar) return;
        string str="제시어를 추측해 정답을 맞추세요.";
        liarUI.PlayerTurn(str);
    }
    [PunRPC]
    void CheckAnswer(string Answer){
        if(!PhotonNetwork.IsMasterClient) return;
        string str;
        if(Answer==title){
            str="라이어가 정답을 맞췄습니다.\n라이어 승리!";
        }
        else{
            str="라이어가 정답을 맞추지 못했습니다.\n라이어 정답: "+Answer;
        }
        photonView.RPC("AnnouncementRPC",RpcTarget.AllViaServer,str);
        cor=WaitTimer();
        StartCoroutine(cor);
    }
    IEnumerator WaitTimer()
    {   
        //for master client
        int i=5;
        
        while(i>=0){
            yield return new WaitForSeconds(1f);
            i--;
        }
        photonView.RPC("RestartGame",RpcTarget.AllViaServer);
        
    }

    [PunRPC]
    void RestartGame(){
        InitValue();
        if(PhotonNetwork.IsMasterClient){
            RandomPlayerNumber();
            cor=GameReadyCheck();
            StartCoroutine(cor);
        }
    }
}

