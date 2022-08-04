using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    private static GameManager instance;
    public static GameManager Instance{
        get{
            if(instance==null) return null;
            return instance;
        }
    }

    public float fallingPoint{get; private set;}
    public Vector3 startPoint{get; private set;}

    void Awake(){
        if(instance==null){
            instance=this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);
    }

    public void SetFallingPoint(float height){
        fallingPoint=height;
    }
    public void SetStartPoint(Vector3 StartPoint){
        startPoint=StartPoint;
    }

    public void GoalIn(){
        UIManager.Instance.GoalIn();
        Time.timeScale=0;
    }

    
}