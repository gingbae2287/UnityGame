using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassManager : MonoBehaviour
{
    // Start is called before the first frame update
    float offsetX=3f;
    float offsetz=3f;
    int lineCount=10;
    [Header("Glass Start Point")]
    [SerializeField] float startX=-1.5f;
    [SerializeField] float startY=0.4f;
    [SerializeField] float startZ=4f;
    
    [SerializeField]GameObject glassObj;

    [Header("TestValue (default = 0.5)")] 
    [SerializeField] float GlassProperty=0.5f;
    Glass[,] glassScript;

    void Awake(){
        glassScript= new Glass[2,lineCount];
        CreateGlasses();
        
    }
    

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateGlasses(){
        for(int i=0;i<lineCount;i++){
            GameObject obj=Instantiate(glassObj, new Vector3(startX,startY,startZ+offsetz*i),Quaternion.identity);
            GameObject obj2=Instantiate(glassObj, new Vector3(startX+offsetX,startY,startZ+offsetz*i),Quaternion.identity);
            
            obj.transform.SetParent(transform);
            obj2.transform.SetParent(transform);
            glassScript[0,i]=obj.GetComponent<Glass>();
            glassScript[1,i]=obj2.GetComponent<Glass>();
            bool tmp=(Random.value > GlassProperty);
            glassScript[0,i].SetHard(tmp);
            glassScript[1,i].SetHard(!tmp);
        }
    }
}
