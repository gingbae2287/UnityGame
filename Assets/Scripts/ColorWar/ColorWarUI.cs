using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorWarUI : MonoBehaviour {
    [SerializeField] GameObject[] ScoreObjs=new GameObject[4];
    [SerializeField] Text notification;
    Text[] teamScore=new Text[4];
    IEnumerator cor;
    int teamSize;
    void Awake(){
        if(ScoreObjs==null) Debug.LogError("ScoreObjs is null");
        for(int i=0;i<4;i++){
            teamScore[i]=ScoreObjs[i].GetComponentInChildren<Text>();
        }
    }
    private void Start() {
        if(ScoreObjs==null) Debug.LogError("ScoreObjs is null");
        if(notification==null) Debug.LogError("notification is null");
        notification.text="";
        foreach(GameObject obj in ScoreObjs) obj.SetActive(false);
        cor=ScoreBoard();
        StartCoroutine(cor);
    }
    IEnumerator ScoreBoard(){
        while(!ColorWar.Instance.isGameStart){
            yield return new WaitForSeconds(0.1f);
        }
        teamSize=ColorWar.Instance.teamSize;
        for(int i=0;i<teamSize;i++) ScoreObjs[i].SetActive(true);
    }
    public void UpdateScore(int[] Score){
        for(int i=0;i<teamSize;i++){
            teamScore[i].text=": "+Score[i];
        }
    }
    public void ButtonLobby(){
        GameManager.Instance.LeftRoom();
    }
    public void SetNotification(string str){
        notification.text=str;
    }


    
}