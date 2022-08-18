using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour{

    //  singletone  -----////
    private static AuthManager instance;
    public static AuthManager Instance{
        get{
            if(instance==null) return null;
            return instance;
        }
        
    }


    //------              ////
    public bool IsFirebaseReady{get; private set;}
    public bool IsSignInOnProgress {get;private set;}       //로그인 버튼 더블탭 방지

    [Header("Objects")]
    public InputField emailField;
    public InputField pswdField;
    public Button BtSignIn;
    public Button BtSignUp;

    [SerializeField] GameObject UI_SignIn;
    [SerializeField] GameObject Ui_SignUp;
    public FirebaseApp firebaseApp;
    public FirebaseAuth firebaseAuth;

    public FirebaseUser user;

    [Header("로그인 성공후 이동할 Scene Name")]
    [SerializeField] string sceneName;

    void Awake(){
        //singletone-------------///
        if(instance==null){
            instance=this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);
        //------------------------
        BtSignIn.interactable=false;

        // firebase unity 연동 메뉴얼 https://firebase.google.com/docs/unity/setup?hl=ko
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>{
            var dependencyStatus = task.Result;
            if(dependencyStatus ==DependencyStatus.Available){
                firebaseApp = FirebaseApp.DefaultInstance;
                firebaseAuth=FirebaseAuth.DefaultInstance;
                IsFirebaseReady=true;
            }
            else{
                IsFirebaseReady=false;
                UnityEngine.Debug.LogError(System.String.Format(
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }

            BtSignIn.interactable=IsFirebaseReady;
        })  ;   
    }

    public void SignInRequest(){
        if(!IsFirebaseReady || IsSignInOnProgress || user!=null) return;
        
        IsSignInOnProgress=true;
        BtSignIn.interactable=false;

        firebaseAuth.SignInWithEmailAndPasswordAsync(emailField.text, pswdField.text).ContinueWithOnMainThread(task => {
            
            //BtSignIn.interactable=true;
            Debug.Log($"status: {task.Status}");
            IsSignInOnProgress=false;
            if (task.IsCanceled) {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                BtSignIn.interactable=true;
                
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                BtSignIn.interactable=true;
                return;
            }

            // Firebase user has been created.
            user = task.Result;   
            Debug.Log("로그인 성공. Email: "+user.Email);
            SceneManager.LoadScene("Lobby");
            
        });
        
    }

    public void ChangeToSignUp(){
        if(IsSignInOnProgress) return;
        UI_SignIn.SetActive(false);
        Ui_SignUp.SetActive(true);
    }

    public void ChangeToSignIn(){
        UI_SignIn.SetActive(true);
        Ui_SignUp.SetActive(false);
    }
}