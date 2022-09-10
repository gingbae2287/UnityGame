using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinCoinUI : MonoBehaviour {

    [SerializeField] Text bestScore;
    [SerializeField] Text myScore;
    [SerializeField] Text timer;
    [SerializeField] Text notif;
    
    private void Start() {
        if(bestScore==null) Debug.LogError("bestScore is null");
        if(myScore==null) Debug.LogError("myScore is null");
        if(timer==null) Debug.LogError("timer is null");
        if(notif==null) Debug.LogError("notif is null");
        myScore.text="My Score: 0";
        bestScore.text="Best Score: 0";
        notif.text="";
        timer.text="";
    }
    public void UpdateScore(int Score){
        myScore.text="My Score: "+Score;
       
    }
    public void UpdateBestScore(int BestScore){
         bestScore.text="Best Score: "+BestScore;
    }
    public void Timer(int Timer){
        timer.text="Time Left: "+Timer;
    }
    public void SetNotification(string str){
        notif.text=str;
    }
    public void ButtonLobby(){
        GameManager.Instance.LeftRoom();
    }
}