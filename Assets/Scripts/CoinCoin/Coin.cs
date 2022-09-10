using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Coin : MonoBehaviourPun
{
    bool isActive;
    int mapIndex;
    float rotateSpeed=50f;
    private void Start() {
        transform.rotation=Quaternion.Euler(90,0,0);
    }
    void Update(){
        //회전
        if(photonView.IsMine) transform.Rotate(new Vector3(0,0,rotateSpeed*Time.deltaTime));
    }
    private void OnEnable() {
        if(!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("CoinActive", RpcTarget.Others,true);
    }
    private void OnDisable() {
        mapIndex=-1;
        if(!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("CoinActive", RpcTarget.Others,false);
    }
    public void ActiveCoin(int mapIdx){
        mapIndex=mapIdx;
    }
    private void OnTriggerEnter(Collider other) {
        if(!(other.gameObject.tag=="Player")) return;
        if(!other.gameObject.GetComponent<Player>().photonView.IsMine) return;
        isActive=false;
        photonView.RPC("GetCoin",RpcTarget.MasterClient,mapIndex);
        Debug.Log("get COin");
    }
    [PunRPC]
    void GetCoin(int mapIdx){
        if(!PhotonNetwork.IsMasterClient) return;
        CoinManager.Instance.GetCoin(mapIdx);
        gameObject.SetActive(false);
    }
    [PunRPC]
    void CoinActive(bool act){
        gameObject.SetActive(act);

    }
}