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

public class LiarGame : MonoBehaviourPunCallbacks,IPunObservable
{
    //Vector3[] startPoint;

    private static LiarGame instance;
    public static LiarGame Instance{
        get{
            if(instance==null) return null;
            else return instance;
        }
    }
    //==class==
    [SerializeField] LiarUI liarUI;

    //===game setting==
    int maxPlayer=8, minPlayer=4;
    float fallingPoint=-5f;
    string VoteZonePath="Liar/VoteZone";
    Vector3 VoteZonePosition=new Vector3(-9, 0, 9.5f);
    string category,title;
    IEnumerator cor;
    enum GameState{ //room's custom property
        DBLoading,  //0 complete title db loading and set random title
        SetLiar,    //1 mix players order and set liar player => gameready and start count
        StartCount,
        Gaming,     //2 each player explane title 
        VoteTime,   //3 vote time
        OneMoreTurn,    //4 End vote time, notice result
        AnswerTime,
        Waiting,    //5 wait game restart
    }
    
    GameState gameState;
    Hashtable state;
    //===for masterClient====
    int[] playerOrder;
    int currentOrder, timer, sendTime, voteResult, currentPlayers;
    bool isDBLoading, DBLoad, isGameReady, isAnswerTime, isPlaying, isReadExplanation, isVoteEnd, isVoteZoneCreate;
    DatabaseReference DBTitle;
    List<KeyValuePair<string,string>> titleList;
    int[] votePlayer;   //idx: playernumber, value: voted playernumber
    int[] voteCount;    //how many count voted for other players
    GameObject[] voteZones;
    int turnTime=30, voteTime=15, wattingTime=5;
    
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
        voteZones=new GameObject[maxPlayer];
        votePlayer=new int[maxPlayer];
        voteCount=new int[maxPlayer];
        playerOrder=new int[maxPlayer];
        titleList= new List<KeyValuePair<string,string>>();
        state=new Hashtable();
        gameState=GameState.DBLoading;
        state.Add("gameState", gameState);
        isDBLoading=false; DBLoad=false; isGameReady=false; isVoteZoneCreate=false;

        for(int i=0;i<maxPlayer;i++) votePlayer[i]=-1;

        InitValue();
        if(PhotonNetwork.IsMasterClient) {
            PhotonNetwork.KeepAliveInBackground=3f;
            PhotonNetwork.CurrentRoom.SetCustomProperties(state);
            GetDB();
            //game state
        }
       
        
    }
    //DB for title
    void GetDB(){
        if(!PhotonNetwork.IsMasterClient) return;
        if(isDBLoading||DBLoad) return;
        isDBLoading=true;
        DBTitle=FirebaseDatabase.DefaultInstance.GetReference("LiarGameTitle");
        DBTitle.GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted) {
                isDBLoading=false; DBLoad=false;
                Debug.LogError("error GetValueAsync()");
                return;
            }
            else if (task.IsCompleted) {
                DataSnapshot snapshot=task.Result;
                foreach(DataSnapshot category in snapshot.Children){
                    foreach(DataSnapshot title in category.Children){
                        titleList.Add(new KeyValuePair<string, string>
                                (category.Key,(string)title.Key)
                        );
                    }
                }
                //isGameReady=true;
                DBLoad=true;
                //photonView.RPC("SendList", RpcTarget.AllBufferedViaServer, titleList);
                
            }
        });
    }
    [PunRPC]
    void SendList(List<KeyValuePair<string, string>> TitleList){
        //list????????????
        titleList=TitleList;

        
    }
    void InitValue(){
        isVote=false; isLiar=false; isAnswerTime=false; isPlaying=false; isVoteEnd=false;
        liarPlayer=0; currentOrder=0; voteResult=0;
        title=""; category="";
        for(int i=0;i<maxPlayer;i++){
            //votePlayer[i]=-1;
            voteCount[i]=0;
            if(i<PhotonNetwork.PlayerList.Length){
                playerOrder[i]=i;
            }
            else playerOrder[i]=-1;
        }
    }

    void Start(){
        if(PhotonNetwork.IsMasterClient){
            cor=GameStateCheck();
            StartCoroutine(cor);

            CreateVoteZone();
        }
    }

    void CreateVoteZone(){
        if(!PhotonNetwork.IsMasterClient) return;
        for(int i=0;i<maxPlayer;i++){
            object[] data= new object[1];
            data[0]=i;
            Vector3 zonePos=VoteZonePosition+(new Vector3(6*(i%4),0,(-19)*(i/4)));
            voteZones[i]=PhotonNetwork.InstantiateRoomObject(VoteZonePath,zonePos, Quaternion.identity,0,data);
            voteZones[i].SetActive(false);
        }
        photonView.RPC("VoteZoneCreateRPC", RpcTarget.AllViaServer);
    }

    [PunRPC]
    void VoteZoneCreateRPC(){
        isVoteZoneCreate=true;
    }
    IEnumerator GameStateCheck(){
        //?????? ??????
       if(!PhotonNetwork.IsMasterClient) StopCoroutine(cor);

        while(!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("gameState")){
                yield return new WaitForSeconds(0.1f);
        }
        
        string str;
        while(true){
            switch(gameState){
            case GameState.DBLoading:
                if(!(isDBLoading||DBLoad)) GetDB();
                while(!DBLoad){
                    yield return new WaitForSeconds(0.2f);
                }
                gameState=GameState.SetLiar;
                state["gameState"]=gameState;
                PhotonNetwork.CurrentRoom.SetCustomProperties(state);
                
            break;
            case GameState.SetLiar:
                if(!DBLoad){
                    gameState=GameState.DBLoading;
                    state["gameState"]=gameState;
                    PhotonNetwork.CurrentRoom.SetCustomProperties(state);
                    break;
                }
                if(isPlaying){
                    timer=turnTime;
                    gameState=GameState.Gaming;
                    state["gameState"]=gameState;
                    PhotonNetwork.CurrentRoom.SetCustomProperties(state);
                    break;
                }
                RandomPlayerNumber();      //set players order and Liar
                bool checkPlayerSpawn=false;
                while(!checkPlayerSpawn){   //check all player are spawn
                    checkPlayerSpawn=true;
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
                    yield return new WaitForSeconds(0.1f);
                }
                GameStart();
                while(!isPlaying) yield return new WaitForSeconds(0.1f);

                gameState=GameState.StartCount;
                state["gameState"]=gameState;
                PhotonNetwork.CurrentRoom.SetCustomProperties(state);
                timer=3;
                
            break;
            case GameState.StartCount:
                //Start Timer
                while(timer>0){
                    str=timer+"??? ??? ????????? ???????????????.";
                    photonView.RPC("AnnouncementRPC",RpcTarget.AllViaServer,str);
                    yield return new WaitForSeconds(1f);
                    timer--;
                }
                str="Game Start!";
                photonView.RPC("AnnouncementRPC",RpcTarget.AllViaServer,str);
                yield return new WaitForSeconds(1f);
                timer=turnTime;
                gameState=GameState.Gaming;
                state["gameState"]=gameState;
                PhotonNetwork.CurrentRoom.SetCustomProperties(state);
                
                
            break;
            case GameState.Gaming:
            //??? ??????????????? ????????? ????????? ??? ????????? currentorder??????
            //??? ??????????????? ?????? ????????? ??????????????? rpc?????????
                photonView.RPC("ClearHistoryBox",RpcTarget.AllViaServer);
                while(currentOrder<currentPlayers){
                    photonView.RPC("NextPlayerTurn",RpcTarget.AllViaServer,currentOrder);
                    isReadExplanation=false;
                    while(timer>=0 && !isReadExplanation){
                        photonView.RPC("TimerRPC",RpcTarget.AllViaServer,timer);
                        yield return new WaitForSeconds(1f);
                        timer--;
                    }
                    timer=turnTime;
                    currentOrder++;
                }
                timer=voteTime;
                gameState=GameState.VoteTime;
                state["gameState"]=gameState;
                PhotonNetwork.CurrentRoom.SetCustomProperties(state);
                

            break;
            case GameState.VoteTime:
                if(!isVoteEnd){
                    while(timer>=0){
                        str="???????????? ???????????? ??????????????? ????????? ????????? ???????????????.\n";
                        str+="???????????? "+timer+"???";
                        photonView.RPC("AnnouncementRPC",RpcTarget.AllViaServer,str);
                        yield return new WaitForSeconds(1f);
                        timer--;
                    }
                    voteResult=VoteCheck();
                }
                
                if(voteResult==1){  // ??????????????? ????????? ??????. ????????? ??? ??????.
                    timer=wattingTime;
                    gameState=GameState.OneMoreTurn;
                    state["gameState"]=gameState;
                    PhotonNetwork.CurrentRoom.SetCustomProperties(state);
                }
                else if (voteResult==2){
                    //???????????? ????????? ????????????
                    timer=15;
                    gameState=GameState.AnswerTime;
                    state["gameState"]=gameState;
                    PhotonNetwork.CurrentRoom.SetCustomProperties(state);
                }
                else if( voteResult==3){
                    //???????????? ???????????????
                    timer=wattingTime;
                    gameState=GameState.Waiting;
                    state["gameState"]=gameState;
                    PhotonNetwork.CurrentRoom.SetCustomProperties(state);
                }
            break;
            case GameState.OneMoreTurn:
                photonView.RPC("VoteEnded",RpcTarget.AllBufferedViaServer,0, false);
                while(timer>=0){
                        yield return new WaitForSeconds(1f);
                        timer--;
                }
                timer=turnTime;
                gameState=GameState.Gaming;
                state["gameState"]=gameState;
                PhotonNetwork.CurrentRoom.SetCustomProperties(state);
                
                
            break;
            case GameState.AnswerTime:
                while(isAnswerTime && timer>=0){
                    photonView.RPC("TimerRPC",RpcTarget.AllViaServer,timer);
                    timer--;
                    yield return new WaitForSeconds(1f);
                }
                timer=wattingTime;
                gameState=GameState.Waiting;
                state["gameState"]=gameState;
                PhotonNetwork.CurrentRoom.SetCustomProperties(state);

            break;
            case GameState.Waiting:
                while(timer>=0){
                    yield return new WaitForSeconds(1f);
                    timer--;
                }
                photonView.RPC("RestartGame",RpcTarget.AllViaServer);

                gameState=GameState.SetLiar;
                state["gameState"]=gameState;
                PhotonNetwork.CurrentRoom.SetCustomProperties(state);
            break;
            }
            yield return new WaitForSeconds(0.1f);
        }
        
    }
    // GameState.SetLiar
    void RandomPlayerNumber(){
        //???????????? ?????? ??????
        if(!PhotonNetwork.IsMasterClient) return;
        currentPlayers=PhotonNetwork.PlayerList.Length;
        //int currentPlayers=PhotonNetwork.PlayerList.Length;
        for(int i=0;i<currentPlayers;i++){
            int tmpPlayer=playerOrder[i];
            int randomNum=Random.Range(0,currentPlayers);
            playerOrder[i]=playerOrder[randomNum];
            playerOrder[randomNum]=tmpPlayer;
            
        }
        //????????? ?????????
        liarPlayer=Random.Range(0,currentPlayers);
        for(int i=0;i<currentPlayers;i++){
            Hashtable hash =new Hashtable();
            hash.Add("playerNumber",i);
            if(i==liarPlayer) 
            {
                hash.Add("isLiar", true);
            }
            else hash.Add("isLiar", false);
            PhotonNetwork.PlayerList[playerOrder[i]].SetCustomProperties(hash);

        }
        photonView.RPC("SpawnPlayer", RpcTarget.AllBufferedViaServer,playerOrder,liarPlayer, currentPlayers);
        
    }
    [PunRPC]
    void SpawnPlayer(int[] PlayerOrder, int liar, int CurrentPlayers){
        liarPlayer=liar;
        playerOrder=PlayerOrder;
        currentPlayers=CurrentPlayers;
        //???????????? ?????? ?????? ??????????????????, ????????? ??????
        playerNumber=(int)PhotonNetwork.LocalPlayer.CustomProperties["playerNumber"];
        liarUI.SetMyOrder(playerNumber+1);
        isLiar=(bool)PhotonNetwork.LocalPlayer.CustomProperties["isLiar"];

        if(Player.LocalPlayerInstance!=null) return;
        startPoint=new Vector3(maxPlayer*0.5f-playerNumber, 0, 0);
        PhotonNetwork.Instantiate("Character/TT_male",startPoint,Quaternion.identity);
        Player.LocalPlayerInstance.GameSettingForPlayer(startPoint);

        Hashtable hash=new Hashtable();
        hash.Add("isSpawn",true);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        GameManager.Instance.LoadingProgressFinish();
    }
    //???????????? ??????????????? ??????
    
    void GameStart(){
        if(!PhotonNetwork.IsMasterClient) return;
        if(isPlaying) return;
        int idx=Random.Range(0,titleList.Count);
        category=titleList[idx].Key;
        title=titleList[idx].Value;
        //active votezone
        for(int i=0;i<maxPlayer;i++) voteZones[i].SetActive(false);
        for(int i=0;i<PhotonNetwork.PlayerList.Length;i++) voteZones[i].SetActive(true);
        isPlaying=true;
        photonView.RPC("GameStartRPC",RpcTarget.AllViaServer,category,title);
    }
    [PunRPC]
    void GameStartRPC(string Category, string Title){
        category=Category; title=Title;
        liarUI.SetTitleText(Category,Title,isLiar);
        isPlaying=true;
    }

    public void VotePlayer(int PlayerNumber, int VoteNumber){
        //if(!PhotonNetwork.IsMasterClient) return;
        votePlayer[PlayerNumber]=VoteNumber;
    }
    [PunRPC]    //??????????????? ??????
    void AnnouncementRPC(string str){
        liarUI.SetAnnouncement(str);
    }
    // GameState: Gaming
    [PunRPC]
    void NextPlayerTurn(int CurrentOrder){
        liarUI.ClearAnnouncment();
        currentOrder=CurrentOrder;
        if(playerNumber==CurrentOrder){
            string str;
            if(isLiar) {
                str="???????????? ???????????????.\n";
                str+="??????????????? ????????? ?????? ???????????? ?????? ?????? ?????????.";
            }
            else str="???????????? ?????? ???????????? ?????? ???????????????.";
            liarUI.PlayerTurn(str);
        }
    }
    [PunRPC]
    void TimerRPC(int sec){
        string str;
        if(isAnswerTime){
            if(isLiar){
                if(sec<=0) {
                    SubmitExplanationButton();   //??????????????? ?????? ??????
                    str="????????????";
                }   
                else str="????????????: "+sec+"???";
            }
            else{
                str="???????????? ????????? ????????? ????????????.\n????????????: "+sec+"???";
            }
        }
        else{
            if(playerNumber!=currentOrder)
                str=(currentOrder+1)+"??? ???????????? ???????????????.\n????????????: "+sec+"???";
            else 
            {
                if(sec<=0) {
                    SubmitExplanationButton();   //??????????????? ?????? ??????
                    str="????????????";
                }
                else str="????????????: "+sec+"???";
            }
        }
        liarUI.SetAnnouncement(str);
    }
    public void SubmitExplanationButton(){      //???????????? ????????? ?????? ????????????
        string str=liarUI.GetExplanation();
        if(!isAnswerTime){
            photonView.RPC("ReadExplanation", RpcTarget.AllViaServer,playerNumber, str);
            liarUI.ClearAnnouncment();
        }
        else if(isAnswerTime){
            photonView.RPC("CheckAnswer", RpcTarget.AllViaServer,str);

        }
        
    }
    [PunRPC]    //?????? ????????? ??????????????? ????????? ????????? ?????? ??? ??????????????? ??????
    void ReadExplanation(int PlayerNumber, string ex){
        liarUI.AddHistory(PlayerNumber, ex);
        if(PhotonNetwork.IsMasterClient) 
        {  
            isReadExplanation=true;
        }
    }

    int VoteCheck(){
        if(!PhotonNetwork.IsMasterClient) return 0;
        for(int i=0;i<maxPlayer;i++)voteCount[i]=0;
        int maxCount=-1, maxCountPlayer=-1, sameCountPlayer=-1,count=0;
        int currentVotes=PhotonNetwork.PlayerList.Length;
        
        for(int i=0;i<maxPlayer;i++){
            if(votePlayer[i]<0) continue;
            count++;
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
        int result;
        string str;
        isVoteEnd=true;
        if(count<(currentVotes/2+currentVotes%2)){
            //?????? ????????? ???
            currentOrder=0;
            str="????????? ?????? ??????????????? ??? ?????? ????????? ????????? ??? ?????????.";
            photonView.RPC("AnnouncementRPC",RpcTarget.AllViaServer,str);
            result=1;
            
        }
        
        else if(maxCountPlayer==liarPlayer){
            //????????? ??????
            isAnswerTime=true;
            str=string.Format("?????? ????????? {0}?????? ?????????????????????.\n ???????????? ????????? ????????? ????????????.",maxCountPlayer+1);
            photonView.RPC("AnnouncementRPC",RpcTarget.AllViaServer,str);
            photonView.RPC("LiarAnswerTime",RpcTarget.AllViaServer,true);
            result=2;

        }
        else{
            //????????? ??????
            str=string.Format("?????? ????????? {0}?????? ???????????? ????????????.\n???????????? {1}??? ?????????. ????????? ??????",maxCountPlayer+1, liarPlayer+1);
            photonView.RPC("AnnouncementRPC",RpcTarget.AllViaServer,str);
            result=3;
        }
        photonView.RPC("VoteEnded",RpcTarget.AllViaServer,result, true);
        return result;
    }
    [PunRPC]
    void VoteEnded(int VoteResult, bool voteEnd){
        voteResult=VoteResult;
        isVoteEnd=voteEnd;
    }
    [PunRPC]
    void LiarAnswerTime(bool answerTime){
        isAnswerTime=answerTime;
        if(!isAnswerTime) return;
        if(!isLiar) return;
        string str="???????????? ????????? ????????? ????????????.";
        liarUI.PlayerTurn(str);
    }
    [PunRPC]
    void CheckAnswer(string Answer){
        if(!isAnswerTime) return;
        isAnswerTime=false;
        isPlaying=false;
        if(!PhotonNetwork.IsMasterClient) return;
        string str;
        if(Answer==title){
            str="???????????? ????????? ???????????????.\n????????? ??????!";
        }
        else{
            str="???????????? ????????? ????????? ???????????????.\n????????? ??????: "+Answer;
        }
        photonView.RPC("AnnouncementRPC",RpcTarget.AllViaServer,str);
    }

    [PunRPC]
    void RestartGame(){
        InitValue();
        liarUI.GetExplanation();
    }

    [PunRPC]
    void ClearHistoryBox(){
        liarUI.ClearHistoryBox();
        currentOrder=0;
        voteResult=0;
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if(PhotonNetwork.IsMasterClient){
            if(isPlaying){
                if((bool)otherPlayer.CustomProperties["isLiar"]){
                    StopCoroutine(cor); //stop game state

                    cor=LiarOut();
                    StartCoroutine(cor);
                }
                else{
                    int num=(int)otherPlayer.CustomProperties["playerNumber"];
                    voteZones[num-1].SetActive(false);
                }
            }
        }
    }
    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        if(!PhotonNetwork.IsMasterClient) return;
        if(!isVoteZoneCreate){
            CreateVoteZone();
        }
        gameState=(GameState)PhotonNetwork.CurrentRoom.CustomProperties["gameState"];
        //voteZones
        foreach(VoteZone script in FindObjectsOfType<VoteZone>(true)){
            int idx=script.playerNumberToVote;
            voteZones[idx]=script.gameObject;
        }
        cor=GameStateCheck();
        StartCoroutine(cor);
    }
    IEnumerator LiarOut(){
        string str="???????????? ???????????? ?????? ????????? ?????????.";
        photonView.RPC("AnnouncementRPC",RpcTarget.AllViaServer,str);
         photonView.RPC("RestartGame",RpcTarget.AllViaServer);
        int i=4;
        while(i>0) {
            i--;
            yield return new WaitForSeconds(1f);
        }
        
       
       
        gameState=GameState.SetLiar;
        state["gameState"]=gameState;
        PhotonNetwork.CurrentRoom.SetCustomProperties(state);
        cor=GameStateCheck();
        StartCoroutine(cor);
    }
    public override void OnLeftRoom()
    {
        if(PhotonNetwork.IsMasterClient){
           photonView.RPC("TimerSynchronization", RpcTarget.AllViaServer, timer);
        }
    }

    //IPunObservable
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            if(sendTime!=timer) {
                sendTime=timer;
                stream.SendNext(timer);
            }

        }

        else
        {
            timer = (int)stream.ReceiveNext();
        }
    }
}

