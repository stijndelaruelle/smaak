﻿using UnityEngine;
using System.Collections;
using GameSparks.Api.Requests;

public class GameSparksManager : Singleton<GameSparksManager>
{
    [SerializeField]
    private MenuManager m_MenuManager;

    private string m_DisplayName;
    public string DisplayName
    {
        get { return m_DisplayName; }
    }

    private string m_UserId;
    private string m_FacebookID;

    private Texture2D m_ProfilePicture;
    public Texture2D ProfilePicture
    {
        get { return m_ProfilePicture; }
    }

    public void Login(string userName, string password)
    {
        AuthenticationRequest request = new AuthenticationRequest();

        request.SetUserName(userName);
        request.SetPassword(password);
            
        request.Send((response) =>
        {
            if (!response.HasErrors)
            {
                Debug.Log("Player Authenticated...");
                OnLogin();
            }
            else
            {
                Debug.LogError("Error Authenticating Player...");
            }
        });
    }

    public void FacebookLogin(string accessToken)
    {
        FacebookConnectRequest request = new FacebookConnectRequest();
        request.SetAccessToken(accessToken);

        request.Send((response) =>
        {
            if (!response.HasErrors)
            {
                Debug.Log("Player registered via facebook!...");
                OnFacebookLogin();
            }
            else
            {
                Debug.LogError("Error Authenticating Player...");
            }
        });
    }

    public void Register(string userName, string password, string displayName)
    {
        RegistrationRequest request = new RegistrationRequest();

        request.SetUserName(userName);
        request.SetPassword(password);
        request.SetDisplayName(displayName);

        request.Send((response) => {
              if (!response.HasErrors)
              {
                  Debug.Log("Player Registered!");

                  //Let's immediatly login!
                  Login(userName, password);
              }
              else
              {
                  Debug.LogError("Error Registering Player!");
              }
          }
        );
    }

    private void OnFacebookLogin()
    {
        //Update details
        AccountDetailsRequest request = new AccountDetailsRequest();

        request.Send((response) =>
        {
            m_DisplayName = response.DisplayName;
            m_UserId = response.UserId;
            m_FacebookID = response.ExternalIds.GetString("FB").ToString();
            StartCoroutine(GetFacebookPicture());
        });
    }

    private void OnLogin()
    {
        //Update details
        AccountDetailsRequest request = new AccountDetailsRequest();

        request.Send((response) =>
        {
            m_DisplayName = response.DisplayName;
            m_UserId = response.UserId;

            ShowGamePanel();
        });
    }

    private void ShowGamePanel()
    {
        m_MenuManager.ShowGamePanel();
    }

    public IEnumerator GetFacebookPicture()
    {
        //To get our facebook picture we use this address which we pass our facebookId into
        var www = new WWW("http://graph.facebook.com/" + m_FacebookID + "/picture?width=200&height=200");

        yield return www;

        m_ProfilePicture = new Texture2D(25, 25);

        www.LoadImageIntoTexture(m_ProfilePicture);

        ShowGamePanel();
    }
}
