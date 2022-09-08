using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    //GameObject player;
    float offsetX;
    float offsetY;
    float offsetZ;
    //float offset=6f;
    Vector3 offset;
    [SerializeField] float followSpeed=5f;
    private float camera_distanse=1f;
    float rotateSpeed=350f, moveSpeed=5f;

    float mouseX,mouseY;

    Vector3 camPos, playerPos,rotateCam;    //Camera Position
    bool isFollowing, cameraReady;

    void Awake(){
        isFollowing=false;
        cameraReady=false;
    }
    void start(){
        
    }
    void Update(){
        
    }

    void LateUpdate(){
        camPos=Camera.main.transform.position;
        playerPos=transform.position;
        //playerPos.y+=3;
        playerPos.y=camPos.y;
        MoveCamera();
        RotateCamera();
    }

    public void StartMove(){
        isFollowing=true;
    }

    void MoveCamera(){
        if(!isFollowing) return;
        if(cameraReady) return;
        if(GetRange()>36){
            //Vector3 dir= playerPos-camPos;
            //Camera.main.transform.Translate(dir*Time.deltaTime*moveSpeed);
            //Camera.main.transform.position=Vector3.MoveTowards(camPos,playerPos, moveSpeed*Time.deltaTime);
            
            Camera.main.transform.position=Vector3.Lerp(camPos,playerPos,0.0025f);
        }
        else{
            cameraReady=true;
            offset=playerPos-camPos;
            offset=offset.normalized*8f;
        }
        /*else if(GetRange()<offset*offset){
            Vector3 dir= camPos-playerPos;

            //Camera.main.transform.position=Vector3.MoveTowards(camPos,camPos+dir, moveSpeed*Time.deltaTime);
            Camera.main.transform.position=Vector3.Lerp(camPos,camPos+dir,0.005f);
        }*/
        /*Pos.x=transform.position.x+(offsetX*camera_distanse);
        Pos.y=transform.position.y+(offsetY*camera_distanse);
        Pos.z=transform.position.z+(offsetZ*camera_distanse);*/
    }
    float GetRange(){
        return (transform.position-Camera.main.transform.position).sqrMagnitude;
    }
    void RotateCamera(){
        if(!isFollowing) return;
        if(!cameraReady) return;
        mouseX=Input.GetAxis("Mouse X")*Time.deltaTime * rotateSpeed;
        mouseY=Input.GetAxis("Mouse Y")*Time.deltaTime * rotateSpeed;
        
        
        
        if(mouseX==0 && mouseY==0){

        }
        else{
            Camera.main.transform.RotateAround(transform.position, Vector3.up, mouseX);
            if(camPos.y<transform.position.y+1f) {
                //camPos.y=transform.position.y+0.5f;
                //Camera.main.transform.position=camPos;
                if(mouseY<0) Camera.main.transform.RotateAround(transform.position, Camera.main.transform.right, -mouseY);
            }
            else if(camPos.y>transform.position.y+6f) 
            {
                
                //camPos.y=transform.position.y+3f;
                //Camera.main.transform.position=camPos;
                if(mouseY>0) Camera.main.transform.RotateAround(transform.position, Camera.main.transform.right, -mouseY);
            }
            else{
                Camera.main.transform.RotateAround(transform.position, Camera.main.transform.right, -mouseY);
            }
                
            camPos=Camera.main.transform.position;
            playerPos=transform.position;
            offset=playerPos-camPos;
            offset=offset.normalized*8f;
            
        }
        Vector3 pos=transform.position-offset;
        if(mouseY*mouseY<0.01)pos.y=Camera.main.transform.position.y;
        Camera.main.transform.position=pos;

        Camera.main.transform.LookAt(transform.position);

        

    }
}