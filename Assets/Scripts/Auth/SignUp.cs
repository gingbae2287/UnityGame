using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignUp : MonoBehaviour{

    [SerializeField]InputField emailField;
    [SerializeField] InputField pswdField;
    //[SerializeField] Button BtSignUp;
    public bool IsSignUpOnProgress {get; private set;}

    void Awake(){
        IsSignUpOnProgress=false;
    }

    public void SignUpRequest(){
        
        if(IsSignUpOnProgress || !AuthManager.Instance.IsFirebaseReady) return;
        if(emailField.text==""||pswdField.text==""){
            Debug.Log("이메일과 비밀번호를 입력하세요.");
            return;
        }
        IsSignUpOnProgress=true;
        AuthManager.Instance.firebaseAuth.CreateUserWithEmailAndPasswordAsync(emailField.text, pswdField.text).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                IsSignUpOnProgress=false;
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                IsSignUpOnProgress=false;
                return;
            }

            // Firebase user has been created.
            //Firebase.Auth.FirebaseUser newUser = task.Result;
            //AuthManager.Instance.user= task.Result;
            Debug.LogFormat("회원가입 성공. email: {0}", task.Result.Email);
            IsSignUpOnProgress=false;
            });
    }

    public void BackToSignIn(){
        if(IsSignUpOnProgress) return;
        AuthManager.Instance.ChangeToSignIn();
    }
}