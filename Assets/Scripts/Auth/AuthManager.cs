using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Database;

using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using System.Threading.Tasks;

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
    //public InputField emailField;
   // public InputField pswdField;
    //public Button BtSignIn;
    //public Button BtSignUp;
    [SerializeField] Button[] loginButtons;

    [Header("UI Object")]
    [SerializeField] GameObject UILogin;
    //[SerializeField] GameObject UI_SignIn;
    //[SerializeField] GameObject Ui_SignUp;
    [SerializeField] GameObject SetUserNameObj;

//========Firebase==============
    public FirebaseApp firebaseApp{get; private set;}
    public FirebaseAuth firebaseAuth{get; private set;}
    public DatabaseReference DBref{get; private set;}
    public FirebaseUser user{get; private set;}


    

    ///====var====
    string authCode;
    public string userName{get; private set;}
    public bool isGuest{get; private set;}
    bool isConnecting;

    void Awake(){
        //singletone-------------///
        if(instance==null){
            instance=this;
            DontDestroyOnLoad(this.gameObject);
        }
        else Destroy(this.gameObject);
        //------------------------
        //BtSignIn.interactable=false;

        
        FirebaseInitSetting();
        GooglePlayInitSetting();

    }
    void GooglePlayInitSetting(){
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
        .RequestServerAuthCode(false )
        .Build();
        PlayGamesPlatform.InitializeInstance(config);
        // recommended for debugging:
        PlayGamesPlatform.DebugLogEnabled = true;
        // Activate the Google Play Games platform
        PlayGamesPlatform.Activate();
    }
    void FirebaseInitSetting(){
        // firebase unity 연동 메뉴얼 https://firebase.google.com/docs/unity/setup?hl=ko
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>{
            var dependencyStatus = task.Result;
            if(dependencyStatus ==DependencyStatus.Available){
                
                firebaseApp = FirebaseApp.DefaultInstance;
                firebaseAuth=FirebaseAuth.DefaultInstance;
                DBref=FirebaseDatabase.DefaultInstance.RootReference;
                IsFirebaseReady=true;
                foreach(Button button in loginButtons) button.interactable=true;

            }
            else{
                IsFirebaseReady=false;
                UnityEngine.Debug.LogError(System.String.Format(
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
            
        })  ;
    }
    public void GooglePlayLoginButton(){
        if(isConnecting) return;
        isConnecting=true;
        Social.localUser.Authenticate((bool success) => {
            if (success) {
                authCode = PlayGamesPlatform.Instance.GetServerAuthCode();
                //testText.text=Social.localUser.userName+"  "+Social.localUser.id;
                FirebaseConnect();
            }
            else{
                isConnecting=false;
            }
        });
    }
    void FirebaseConnect(){
        Firebase.Auth.Credential credential =
            Firebase.Auth.PlayGamesAuthProvider.GetCredential(authCode);
        firebaseAuth.SignInWithCredentialAsync(credential).ContinueWith(task => {
            isConnecting=false;
            if (task.IsCanceled) {
                Debug.LogError("SignInWithCredentialAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                return;
            }
            user = task.Result;
            CheckUserName();
        

        });
    }

    //기존 로그인 버튼(email)
    /*public void SignInRequest(){
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
            
            CheckUserName();
        });
    }*/

    //Check UserName
    void CheckUserName(){
        DatabaseReference usersRef=FirebaseDatabase.DefaultInstance.GetReference("users");
        usersRef.GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted) {
                Debug.LogError("error GetValueAsync()");
                return;
            }
            else if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                string Id=user.UserId;
                string Name;
                if(snapshot.HasChild(Id)){
                    Name=snapshot.Child(Id).Child("UserName").Value.ToString();
                    if(Name==""){
                        SetUserNameUI();
                    }
                    else{
                        userName=Name;
                        WriteByUserName(Name);
                        SceneManager.LoadScene("Lobby");
                    }
                }
                else{
                    WriteNewUser(Id,null);
                    SetUserNameUI();
                }
            }
        });

        
    }

    void GuestLogin(){
        if(isConnecting) return;
        isConnecting=true;
        firebaseAuth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {
            isConnecting=false;
            if (task.IsCanceled) {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }
            isGuest=true;
            user = task.Result;
            SceneManager.LoadScene("Lobby");
        });
    }
/*
    public void ChangeToSignUp(){
        if(IsSignInOnProgress) return;
        UI_SignIn.SetActive(false);
        Ui_SignUp.SetActive(true);
    }

    public void ChangeToSignIn(){
        UI_SignIn.SetActive(true);
        Ui_SignUp.SetActive(false);
    }
*/
    void SetUserNameUI(){
        //UI_SignIn.SetActive(false);
        UILogin.SetActive(false);
        SetUserNameObj.SetActive(true);
    }
    public void GuestLoginButton(){
        GuestLogin();
    }
    public bool SetUserName(string Name){
        bool isUserNameSet=false;
        DatabaseReference userNameRef=FirebaseDatabase.DefaultInstance.GetReference("userName");
        userNameRef.GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted) {
                Debug.LogError("error GetValueAsync()");
                return;
            }
            else if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                if(snapshot.HasChild(Name)){
                   Debug.Log("이미 사용중인 이름입니다.");

                }
                else{
                    isUserNameSet=true;
                    WriteByUserName(Name);
                    WriteNewUser(user.UserId,Name);
                    SceneManager.LoadScene("Lobby");
                }
            }
        });
        /*DatabaseReference usersRef=FirebaseDatabase.DefaultInstance.GetReference("users");
            usersRef.GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted) {
                Debug.LogError("error GetValueAsync()");
                return;
            }
            else if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                string userId=user.UserId;
                if(snapshot.HasChild(userId)){
                    usersRef.Child(userId).Child("UserName").SetValueAsync(Name);
                    WriteByUserName(Name, user.Email);
                    isUserNameSet=true;
                    userName=Name;
                    SceneManager.LoadScene("Lobby");

                }
                else{
                    Debug.LogError("user is not exist");
                }
            }
        });*/
        return isUserNameSet;
    }

    public void GoLobbyScene(){
        SceneManager.LoadScene("Lobby");
    }

    void WriteNewUser(string userId, string name) {
        User user = new User(name);
        string json = JsonUtility.ToJson(user);
        DBref.Child("users").Child(userId).SetRawJsonValueAsync(json);
    }
    //데이터 색인
    void WriteByUserName(string userName){
        //UserName username=new UserName(email);
        //string json=JsonUtility.ToJson(username);
        //DBref.Child("userName").Child(userName).SetRawJsonValueAsync(json);
        DBref.Child("userName").SetValueAsync(userName);
    }

    
}

public class User {
    public string UserName;
    //public string Email;

    public User() {
    }

    public User(string username) {
        this.UserName = username;
        //this.Email = email;
    }
}
/*
public class UserName{
    public string Eamil;
    public UserName(string email){
        this.Eamil=email;
    }
}
*/