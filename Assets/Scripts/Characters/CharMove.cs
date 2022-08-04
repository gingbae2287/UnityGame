using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharMove : MonoBehaviour
{
    float hori;     //horizontal axes
    float vert;     //vertical axes
    bool runState;
    bool jump;
    int jumpCount=1;      //점프 횟수 제한
    [SerializeField] float speed=0.1f;
    [SerializeField] float rotateSpeed=0.1f;
    [SerializeField] float jumpPower=1f;
    Vector3 movement;  

    Rigidbody rigid; 
    Animator anim;

    void Awake(){
        rigid=GetComponent<Rigidbody>();
        anim=GetComponent<Animator>();
    }
    void Start()
    {
        runState=false;
        jump=false;
    }

    void FixedUpdate() {
        hori=Input.GetAxis("Horizontal");
        vert=Input.GetAxis("Vertical");
        Run();
        Turn();
        
        
    }
    void Update()
    {
        Jump();
        
        
        CheckFall();
       
    }
    void Run(){
        
        if(hori==0 && vert==0) {
            if(runState) {
                //변수값을 같은 값으로 변경하는 명령도 성능에 영향가는가?
                runState=false;
                anim.SetBool("RunState", false);
            }
            return;
        }
        if(!runState) {
            runState=true;
            anim.SetBool("RunState", true);
        }
        anim.SetBool("RunState", true);
        movement.Set(hori, 0, vert);
        movement=movement.normalized*speed;
        rigid.MovePosition(transform.position+movement);
    }

    void Turn(){
        if(hori==0&& vert==0) return;
        if(jump) return;
        Quaternion newRotation=Quaternion.LookRotation(movement);
        rigid.rotation=Quaternion.Slerp(rigid.rotation, newRotation,rotateSpeed);
    }

    void Jump(){
        if(Input.GetButtonDown("Jump")){
            if(jump) return;
            jump=true;
            anim.SetBool("Jump", true);
            rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }
    }

    void CheckFall(){
        if(transform.position.y<GameManager.Instance.fallingPoint){
            transform.position=GameManager.Instance.startPoint;
        }
    }

    void AnimationUpdate(){
        //if(runState) anim.SetBool("RunState", true);
        
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag=="Ground"){
            if(jump){
                jump=false;
                anim.SetBool("Jump", false);
            }
        }
    }
    private void OnTriggerExit(Collider other) {
        if(other.gameObject.tag=="Ground"){
                jump=true;
        }
    }

}
