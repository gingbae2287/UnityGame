using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    //GameObject player;
    [SerializeField] float offsetX=0f;
    [SerializeField] float offsetY=4f;
    [SerializeField] float offsetZ=-5f;
    [SerializeField] float followSpeed=5f;
    private float camera_distanse=1f;
    Vector3 offset;

    Vector3 Pos;    //Camera Position
    bool isFollowing;

    void Awake(){
        isFollowing=false;
    }

    void LateUpdate(){
        MoveCamera();
    }

    public void StartMove(){
        isFollowing=true;
    }

    void MoveCamera(){
        if(!isFollowing) return;
        Pos.x=transform.position.x+(offsetX*camera_distanse);
        Pos.y=transform.position.y+(offsetY*camera_distanse);
        Pos.z=transform.position.z+(offsetZ*camera_distanse);

        Camera.main.transform.position=Pos; 
    }
}