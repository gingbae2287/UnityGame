using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Player : MonoBehaviourPun
{

    private static Player localPlayerInstance;
    public static Player LocalPlayerInstance{
        get{
            if(localPlayerInstance==null) return null;
            return localPlayerInstance;
        }
    }
    float hori;     //horizontal axes
    float vert;     //vertical axes
    bool runState;
    bool jump;
    public bool canMove{get; set;}
    

    public int playerNumber{get; private set;}
    public string playerName{get; private set;}
    public bool gameEnd{get;private set;}
    
    [SerializeField] float speed=0.1f;
    [SerializeField] float rotateSpeed=0.1f;
    [SerializeField] float jumpPower;
    int jumpCount=1;      //점프 횟수 제한
    float fallingPoint=-5;
    Vector3 startPoint;
    
    Vector3 movement;  

    Rigidbody rigid; 
    Animator anim;
    PlayerCamera playerCamera;

    

    void Awake(){
        if(localPlayerInstance==null && photonView.IsMine){
            localPlayerInstance=this;
            DontDestroyOnLoad(this.gameObject);

        }
        else{
        }

        
        rigid=GetComponent<Rigidbody>();
        anim=GetComponent<Animator>();
        playerCamera=GetComponent<PlayerCamera>();


    }
    void Start()
    {
        if(photonView.IsMine) {
            playerCamera.StartMove();
            runState=false;
            jump=false;
            canMove=true;
            //Camera.main.transform.SetParent(transform);
        }

        gameEnd=false;
        playerNumber=PhotonNetwork.LocalPlayer.ActorNumber;
        playerName=PhotonNetwork.LocalPlayer.NickName;
       //else Destroy(this.gameObject);
    }

    void FixedUpdate() {
        if(!photonView.IsMine) return;
        hori=Input.GetAxis("Horizontal");
        vert=Input.GetAxis("Vertical");
        Run();
        Turn();
        
        
    }
    void Update()
    {
        if(!photonView.IsMine) return;
        Jump();
        
        
        CheckFall();
       
    }
    void Run(){
        if(!canMove){
            runState=false;
            anim.SetInteger("RunState", 0);
            return;
        }
        if(hori==0 && vert==0) {
            if(runState) {
                //변수값을 같은 값으로 변경하는 명령도 성능에 영향가는가?
                runState=false;
                anim.SetInteger("RunState", 0);
            }
            return;
        }
        if(!runState) {
            runState=true;
            anim.SetInteger("RunState", 2);
        }
        anim.SetInteger("RunState", 2);
        /*
        movement.Set(hori, 0, vert);
        movement=movement.normalized*speed;
        rigid.MovePosition(transform.position+movement);
        */
        Vector3 dirForward=transform.position-Camera.main.transform.position;
        dirForward.y=0;
        dirForward=dirForward.normalized;
        Vector3 dirRight=Quaternion.AngleAxis(90, Vector3.up) * dirForward;
        movement=(dirForward*vert + dirRight*hori);
        movement*=speed;
        rigid.MovePosition(transform.position+movement);

    }

    void Turn(){
        if(!canMove) return;
        if(hori==0&& vert==0) return;
        if(jump) return;
        Quaternion newRotation=Quaternion.LookRotation(movement);
        rigid.rotation=Quaternion.Slerp(rigid.rotation, newRotation,rotateSpeed);
    }

    void Jump(){
        if(!canMove) return;
        if(Input.GetButtonDown("Jump")){
            if(jump) return;
            jump=true;
            anim.SetBool("Jump", true);
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }
    }

    void CheckFall(){
        if(transform.position.y<fallingPoint){
            transform.position=startPoint;
        }
        /*
        */
    }

    public void GameEnd(){
        gameEnd=true;
    }
    public void SetPlayerNumber(int num){
        if(!photonView.IsMine) return;
        playerNumber=num;
    }

    public void SetPlayerName(string Name){
        if(!photonView.IsMine) return;
        playerName=Name;
    }

    public void GameSettingForPlayer(Vector3 pos, float FallingPoint=-5f){
        if(!photonView.IsMine) return;
        this.fallingPoint=FallingPoint;
        startPoint=pos;

    }
    public void StopMove(){
        canMove=false;
        anim.SetInteger("RunState", 0);
    }

    void AnimationUpdate(){
        //if(runState) anim.SetBool("RunState", true);
        
    }

    private void OnTriggerEnter(Collider other) {
        if(!photonView.IsMine) return;
        if(other.gameObject.tag=="Ground"){
            if(jump){
                jump=false;
                anim.SetBool("Jump", false);
            }
        }
    }
    private void OnTriggerExit(Collider other) {
        if(!photonView.IsMine) return;
        /*if(other.gameObject.tag=="Ground"){
                jump=true;
        }*/
    }

}
