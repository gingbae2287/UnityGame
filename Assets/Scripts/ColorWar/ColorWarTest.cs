using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorWarTest : MonoBehaviour {
    [SerializeField] GameObject block;
    float blockSize=3f;
    private void Start() {
        create();
    }
    void create(){
        for(int i=0;i<36;i++){
                Vector3 pos=new Vector3(blockSize*3+i%6*blockSize, 0, blockSize*3+i/6*blockSize);
                Instantiate(block,pos,Quaternion.identity);
        }
        for(int i=0;i<10;i++){
            for(int j=0;j<2;j++){
                Vector3 pos=new Vector3(blockSize*1+i*blockSize, blockSize*1, blockSize*1+j*blockSize);
                Instantiate(block,pos,Quaternion.identity);
            }
        }
        for(int i=0;i<8;i++){
            for(int j=0;j<2;j++){
                Vector3 pos=new Vector3(blockSize*1+j*blockSize, blockSize*1, blockSize*1+i*blockSize);
                Instantiate(block,pos,Quaternion.identity);
            }
        }
        for(int i=0;i<12;i++){
                Vector3 pos=new Vector3(i*blockSize, blockSize*2, 0);
                Instantiate(block,pos,Quaternion.identity);
        }
        for(int i=0;i<11;i++){
                Vector3 pos=new Vector3(0, blockSize*2, blockSize*1+i*blockSize);
                Instantiate(block,pos,Quaternion.identity);
        }
    }
}

