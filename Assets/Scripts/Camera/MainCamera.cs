using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] GameObject player;
    [SerializeField] float offsetX=0f;
    [SerializeField] float offsetY=4f;
    [SerializeField] float offsetZ=-5f;
    [SerializeField] float followSpeed=5f;
    private float camera_distanse=1f;
    Vector3 offset;

    Vector3 Pos;    //Camera Position
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        MoveCamera();
    }

    void MoveCamera(){
        Pos.x=player.transform.position.x+(offsetX*camera_distanse);
        Pos.y=player.transform.position.y+(offsetY*camera_distanse);
        Pos.z=player.transform.position.z+(offsetZ*camera_distanse);

        transform.position=Pos; 
    }
}
