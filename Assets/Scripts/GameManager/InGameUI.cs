using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour {
    [SerializeField] GameObject settingUI;
    void Start(){
        if(settingUI==null) Debug.LogError("null");
    }
    public void ButtonSettingUI(){
        GameManager.Instance.SettingUIActive();
    }
}