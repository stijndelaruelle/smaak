using UnityEngine;
using System.Collections;
using Facebook.Unity;
using System.Collections.Generic;

public class FacebookManager : Singleton<FacebookManager>
{
    private void Start()
    {
        FacebookInitialize();
    }

    private void FacebookInitialize()
    {
        FB.Init(OnInitComplete, OnHideUnity);
    }

    private void OnInitComplete()
    {
        Debug.Log("FB.Init competed: Is user logged in? " + FB.IsLoggedIn);
    }

    private void OnHideUnity(bool isGameShown)
    {
        Debug.Log("Is game showing?" + isGameShown);
    }

    public void FacebookLogin()
    {
        List<string> permissions = new List<string>(){"public_profile", "email", "user_friends"};
        FB.LogInWithReadPermissions(permissions, OnFBLoginComplete);
    }

    private void OnFBLoginComplete(ILoginResult loginResult)
    {
        if (loginResult.Error != null)
        {
            Debug.LogError(loginResult.Error);
            return;
        }

        if (FB.IsLoggedIn)
        {
            GameSparksManager.Instance.FacebookLogin(loginResult.AccessToken.TokenString);
        }
    }
}
