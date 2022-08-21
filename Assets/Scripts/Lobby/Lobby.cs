using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviour {

    [SerializeField] GameObject CustomRoomObject; 
    [SerializeField] GameObject mainLobbyObject;
    [SerializeField] GameObject joinCustomRoomObject;
    [SerializeField] InputField inputFieldRoomID;
    [SerializeField] GameObject errorMessageObject;


    IEnumerator cor;
    void Start(){
        if(CustomRoomObject==null) Debug.LogError("CustomRoomObject is null");
        if(mainLobbyObject==null) Debug.LogError("mainLobbyObj is null");
        if(inputFieldRoomID==null) Debug.LogError("inputFieldRoomID is null");
        if(errorMessageObject==null) Debug.LogError("errorMessageObject is null");
        
    }
    public void RandomMatchingButton(){
        NetworkManager.Instance.Connect();
    }

    public void CreateCustomRoomButton(){
        NetworkManager.Instance.CreateCustomRoom();
        //NetworkManager.Instance.CreateCustomRoom();
        cor=CheckCreateRoom();
        StartCoroutine(cor);
    }
    public void JoinCustomRoomButton(){
        if(!NetworkManager.Instance.isConnectedToMasterServer) return;
        joinCustomRoomObject.SetActive(true);
    }

    IEnumerator CheckCreateRoom(){
        int corCount=0;
        while(!NetworkManager.Instance.isConnectedToRoom){
            yield return new WaitForSeconds(0.1f);
            corCount++;
            if(corCount>20) break;
            
        }
        //create custom Room
        if(NetworkManager.Instance.isConnectedToRoom){
            CustomRoomObject.SetActive(true);
            mainLobbyObject.SetActive(false);

           //CustomRoomObject.GetComponent<CustomRoom>().RenewalRoom(); 

        }
    }

    public void CheckCustomRoomID(){
        if(NetworkManager.Instance.isConnectedToRoom) return;
        int roomID=int.Parse(inputFieldRoomID.text);
        NetworkManager.Instance.JoinCustomRoom(roomID);
        cor=CheckJoinRoom();
        StartCoroutine(cor);
        

    }
    IEnumerator CheckJoinRoom(){
        int corCount=0;
        while(!NetworkManager.Instance.isConnectedToRoom){
            corCount++;
            if(corCount>10) break;
            yield return new WaitForSeconds(0.1f);
        }
        //create custom Room
        if(NetworkManager.Instance.isConnectedToRoom){
            CustomRoomObject.SetActive(true);
            mainLobbyObject.SetActive(false);
            joinCustomRoomObject.SetActive(false);

            //CustomRoomObject.GetComponent<CustomRoom>().RenewalRoom(); 
        }
        else {
            errorMessageObject.SetActive(true);
            NetworkManager.Instance.infoMessage="방이 존재하지 않습니다. ";
        }
    }

    public void BackButton(){
        NetworkManager.Instance.BackToLobby();
        joinCustomRoomObject.SetActive(false);
    }


}