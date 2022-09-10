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
    public bool isGameStart{get;private set;}
    IEnumerator cor;
    void Awake(){
        if(instance==null){
            instance=this;
        }
        else if(instance!=this){
            Destroy(instance.gameObject);
            instance=this;
        }
    }
    void Start(){
        cor=GameState();
        StartCoroutine(cor);
    }
    IEnumerator GameState(){
        while(!CoinManager.Instance.coinCreated){
            yield return new WaitForSeconds(0.1f);
        }
        photonView.RPC("CoinCoinReady",RpcTarget.AllViaServer);
        while(!CheckAllPlayerReady()){
            yield return new WaitForSeconds(0.1f);
        }
        photonView.RPC("CoinCoinStart",RpcTarget.AllViaServer);
        
    }
    [PunRPC]
    void CoinCoinReady(){
        SpawnPlayer();
    }
    [PunRPC]
    void CoinCoinStart(){
        Player.LocalPlayerInstance.canMove=true;
        isGameStart=true;
    }

    void SpawnPlayer(){
        if(Player.LocalPlayerInstance!=null) return;
       // startPoint=new Vector3(maxPlayer*0.5f-playerNumber, 0, 0);
        PhotonNetwork.Instantiate("Character/TT_male",Vector3.zero,Quaternion.identity);
        Player.LocalPlayerInstance.GameSettingForPlayer(Vector3.zero);
        Player.LocalPlayerInstance.StopMove();
        Hashtable hash=new Hashtable();
        hash.Add("isSpawn",true);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
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