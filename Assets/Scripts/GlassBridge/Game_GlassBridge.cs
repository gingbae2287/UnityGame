using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_GlassBridge : MonoBehaviour
{
    [SerializeField] float FallingPoint=-5;
    Vector3 PlayerStartPoint=new Vector3(0,0.5f,0);
    void Start(){
        GameStart();
    }
    void GameStart(){
        GameManager.Instance.SetFallingPoint(FallingPoint);
        GameManager.Instance.SetStartPoint(PlayerStartPoint);
    }
}