using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class VoteZone : MonoBehaviourPun {

    int playerNumberToVote;
    bool[] isPlayerInVoteZone;

    void Awake(){
        isPlayerInVoteZone= new bool[GameManager.Instance.maxPlayerOfLiarGame];
        
    }

    private void OnEnable() {
        object[] data=photonView.InstantiationData;
        playerNumberToVote=(int)data[0];
    }
    private void OnTriggerEnter(Collider other) {
        if(!(other.gameObject.tag=="Player")) return;
        if(!other.gameObject.GetComponent<Player>().photonView.IsMine) return;
        if(LiarGame.Instance.isVote==true) return;
        LiarGame.Instance.isVote=true;
        Debug.Log("투표번호: "+playerNumberToVote);
        photonView.RPC("VoteRPC", RpcTarget.MasterClient, LiarGame.Instance.playerNumber, playerNumberToVote);
        
    }
    private void OnTriggerExit(Collider other) {
        if(!(other.gameObject.tag=="Player")) return;
        if(!other.gameObject.GetComponent<Player>().photonView.IsMine) return;
        if(LiarGame.Instance.isVote==false) return;
        LiarGame.Instance.isVote=false;
        photonView.RPC("VoteRPC", RpcTarget.MasterClient, LiarGame.Instance.playerNumber, -1);
    }

    [PunRPC]
    //RPC for Master
    void VoteRPC(int PlayerNumber, int VoteNumber){
        LiarGame.Instance.VotePlayer(PlayerNumber,VoteNumber);

    }

    public void SetVoteNumber(int num){
        playerNumberToVote=num;
    }
}
