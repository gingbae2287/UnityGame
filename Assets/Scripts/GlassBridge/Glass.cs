using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glass : MonoBehaviour
{
    bool hard;      //false = no collider
    BoxCollider col;

    void Awake(){
        col=GetComponent<BoxCollider>();
    }
    void Start(){
        
    }
    public void SetHard(bool Hard){
        hard=Hard;
        if(hard==false){
            col.enabled=false;
        }
        else col.enabled=true;
    }
}