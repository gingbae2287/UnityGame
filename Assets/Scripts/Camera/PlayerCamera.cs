using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    //GameObject player;
    Vector3 offset;
    Camera mainCam;
    private float camera_distanse=1f;
    float rotateSpeed=250f, moveSpeed=5f;

    float mouseX,mouseY;

    Vector3 camPos, playerPos,rotateCam,pos;    //Camera Position
    bool isFollowing, cameraReady;

    void Awake(){
        isFollowing=false;
        cameraReady=false;
        
    }
    void Start(){
        mainCam=Camera.main;
        if(mainCam==null) Debug.LogError("maincam is null");
        
        if(GetRange()>144){
            offset=(transform.position-mainCam.transform.position).normalized*12;
            mainCam.transform.position=transform.position-offset;
           
        }
         mainCam.transform.LookAt(transform.position);
    }

    private void LateUpdate() {
        camPos=mainCam.transform.position;
        playerPos=transform.position;
        playerPos.y+=1;
        MoveCamera();
        RotateCamera();
    }
    public void StartMove(){
        isFollowing=true;
    }

    void MoveCamera(){
        if(!isFollowing) return;
        if(cameraReady) return;
        if(GetRange()>64){
            mainCam.transform.position=Vector3.Lerp(camPos,playerPos,0.4f*Time.deltaTime);
        }
        else{
            offset=(playerPos-camPos).normalized*8;
            mainCam.transform.position=playerPos-offset;
            mainCam.transform.LookAt(playerPos);
            cameraReady=true;
        }

    }
    float GetRange(){
        return (transform.position-mainCam.transform.position).sqrMagnitude;
    }
    void RotateCamera(){
        if(!isFollowing) return;
        if(!cameraReady) return;
        mainCam.transform.position=playerPos-offset;
        if(GameManager.Instance.pause) return;
        mouseX=Input.GetAxis("Mouse X")*Time.deltaTime * rotateSpeed;
        mouseY=Input.GetAxis("Mouse Y")*Time.deltaTime * rotateSpeed;
        
        if(mouseX==0 && mouseY==0){

        }
        else{
            mainCam.transform.position=playerPos-offset;
            mainCam.transform.RotateAround(transform.position, Vector3.up, mouseX);
            /*
                if(mainCam.transform.position.y<transform.position.y+1f) {
                    camPos=mainCam.transform.position;
                    camPos.y=transform.position.y+1f;
                    mainCam.transform.position=camPos;
                }
                else if(mainCam.transform.position.y>transform.position.y+6f) 
                {
                    camPos=mainCam.transform.position;
                    camPos.y=transform.position.y+6f;
                    mainCam.transform.position=camPos;
                }
            */
            if(camPos.y<playerPos.y+1f) {
                if(mouseY<0) mainCam.transform.RotateAround(playerPos, mainCam.transform.right, -mouseY);
            }
            else if(camPos.y>playerPos.y+5f) 
            {
                if(mouseY>0) mainCam.transform.RotateAround(playerPos, mainCam.transform.right, -mouseY);
            }
            else{

                mainCam.transform.RotateAround(playerPos, mainCam.transform.right, -mouseY);
            }
            
            camPos=mainCam.transform.position;
            //offset=(playerPos-camPos).normalized*8f;
            offset=playerPos-camPos;
        }
        //mainCam.transform.position=playerPos-offset;
        /*if(mainCam.transform.position.y<playerPos.y+1f) {
                camPos=mainCam.transform.position;
                camPos.y=playerPos.y+1f;
                mainCam.transform.position=camPos;
            }
            else if(mainCam.transform.position.y>playerPos.y+5f) 
            {
                camPos=mainCam.transform.position;
                camPos.y=playerPos.y+5f;
                mainCam.transform.position=camPos;
            }
        */
        //mainCam.transform.LookAt(playerPos);
    }
}