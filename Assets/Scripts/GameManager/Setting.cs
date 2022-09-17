using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Setting : MonoBehaviour {
    [SerializeField] Image bgmBar;
    [SerializeField] Image soundBar;
    void Start(){
        if(soundBar==null) Debug.LogError("null");
        if(bgmBar==null) Debug.LogError("null");
    }
    private void OnEnable() {
        //GameManager.Instance.SettingUIActive();
    }
    private void OnDisable() {
        GameManager.Instance.SettingUIOff();
    }

    public void ButtonClose(){
        gameObject.SetActive(false);
    }
    public void BgmValueChange(){
        GameManager.Instance.ChangeBgmValue(bgmBar.fillAmount);
    }
    public void SoundValueChange(){
        GameManager.Instance.ChangeSoundValue(soundBar.fillAmount);
    }
}