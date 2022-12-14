using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class CoinManager : MonoBehaviourPun
{   
    private static CoinManager instance;
    public static CoinManager Instance{
        get{
            if(instance==null) return null;
            else return instance;
        }
    }
    //===for master===
    public bool coinCreated{get; private set;}
    int maxCoins=35, blockSize=3;
    Vector3 coinPos=new Vector3(5,50,5);
    //bool[,] coinMap=new bool[10,10];
    List<int> coinMap=new List<int>(100);
    string coinPath="CoinCoin/Coin";
    Coin[] coins;
    GameObject[] coinObjs;
    IEnumerator cor;
    void Awake(){
        if(instance==null){
            instance=this;
        }
        else if(instance!=this){
            Destroy(instance.gameObject);
            instance=this;
        }

        coins=new Coin[maxCoins];
        coinObjs=new GameObject[maxCoins];
        for(int i=0;i<100;i++){
            coinMap.Add(i);
        }
    }
    private void Start() {
        if(!PhotonNetwork.IsMasterClient) return;
        CreateCoins();
        cor=RandomCoin();
        StartCoroutine(cor);
    }
    void CreateCoins(){
        if(!PhotonNetwork.IsMasterClient) return;
        if(coinCreated) return;
        
        for(int i=0;i<maxCoins;i++){
            coinObjs[i]=PhotonNetwork.InstantiateRoomObject(coinPath,coinPos,Quaternion.identity);
            coins[i]=coinObjs[i].GetComponent<Coin>();
            coinObjs[i].SetActive(false);
        }
        coinCreated=true;
    }
    IEnumerator RandomCoin(){
        while(!coinCreated){
            yield return new WaitForSeconds(0.1f);
        }
        while(!CoinCoin.Instance.isGameStart){
            yield return new WaitForSeconds(0.1f);
        }
        int idx=0;
        float y=0f;
        while(CoinCoin.Instance.isGameStart){
            for(int i=0;i<maxCoins;i++){
                /*if(!coinObjs[i].activeSelf){
                    int x=Random.Range(0,coinMap.Count);
                    Vector3 pos=new Vector3(x%10*blockSize,0,x/10*blockSize);
                    coinObjs[i].SetActive(true);
                    coinObjs[i].transform.position=pos;
                    coinObjs[i].GetComponent<Coin>().ActiveCoin(coinMap[x]);
                    coinMap.RemoveAt(x);
                    yield return new WaitForSeconds(0.3f);
                }*/
                if(!coins[i].gameObject.activeSelf){
                    idx=Random.Range(0,coinMap.Count);
                    y=Random.Range(0.5f,2f);
                    Vector3 pos=new Vector3(coinMap[idx]%10*blockSize,y,coinMap[idx]/10*blockSize);
                    coins[i].gameObject.transform.position=pos;
                    coins[i].gameObject.SetActive(true);
                    coins[i].ActiveCoin(coinMap[idx]);
                    coinMap.RemoveAt(idx);
                    yield return new WaitForSeconds(0.7f);
                }
            }
            yield return new WaitForSeconds(0.2f);
        }
    }
    public void GetCoin(int mapIdx){
        coinMap.Add(mapIdx);
    }

}