using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

    private static UIManager instance;
    public static UIManager Instance{
        get{
            if(instance==null) return null;
            return instance;
        }
    }

    void Awake(){
        if(instance==null){
            instance=this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);
    }

    [SerializeField]GameObject GoalTextObj;
    
    public void GoalIn(){
        GoalTextObj.SetActive(true);
    }
}