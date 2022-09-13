using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using System.Threading.Tasks;

using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

public class PlayCenterManager : MonoBehaviour{

    Firebase.Auth.FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;
    string authCode;
    [SerializeField]Text LogText;
    private void Awake() {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
    .RequestServerAuthCode(false )
    .Build();
    PlayGamesPlatform.InitializeInstance(config);
    // recommended for debugging:
    PlayGamesPlatform.DebugLogEnabled = true;
    // Activate the Google Play Games platform
    PlayGamesPlatform.Activate();
    auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
    LogText.text="022";
    }
    private void Start() {
        test2();
    }
    void test1(){
        PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptOnce, (result) =>{
        LogText.text = result.ToString();
        });

    }

    public void test2(){
            Social.localUser.Authenticate((bool success) => {
                LogText.text="0";
            if (success) {
                LogText.text="1";
                authCode = PlayGamesPlatform.Instance.GetServerAuthCode();
                Firebase.Auth.Credential credential =Firebase.Auth.PlayGamesAuthProvider.GetCredential(authCode);
                auth.SignInWithCredentialAsync(credential).ContinueWith(task => {
                if (task.IsCanceled) {
                    LogText.text="5.";
                    Debug.LogError("SignInWithCredentialAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    LogText.text="6 ";
                    Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                    return;
                }
                FirebaseConnect();
                LogText.text="User signed in successfully";
                Firebase.Auth.FirebaseUser newUser = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    newUser.DisplayName, newUser.UserId);
                });
            }
            else{
                LogText.text="2";
            }
            });
        
    }

    void FirebaseConnect(){
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        Firebase.Auth.Credential credential =
            Firebase.Auth.PlayGamesAuthProvider.GetCredential(authCode);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task => {
        if (task.IsCanceled) {
            Debug.LogError("SignInWithCredentialAsync was canceled.");
            return;
        }
        if (task.IsFaulted) {
            Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
            return;
        }

        Firebase.Auth.FirebaseUser newUser = task.Result;
        LogText.text=newUser.DisplayName.ToString();
        Debug.LogFormat("User signed in successfully: {0} ({1})",
            newUser.DisplayName, newUser.UserId);
        });
    }
    
}