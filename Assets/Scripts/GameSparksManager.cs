using UnityEngine;
using System.Collections;
using GameSparks.Core;
using GameSparks.Api.Requests;
using System.Collections.Generic;
using System.Globalization;
using SimpleJSON;
using System;

public delegate void LoadDataCallback(List<GameSparksManager.DataAttribute> attributes);

public class GameSparksManager : Singleton<GameSparksManager>
{
    public struct DataAttribute
    {
        public DataAttribute(string name, float data)
        {
            m_Name = name;
            m_Data = data.ToString(); //GameSparks doesn't support floats, casting from and to strings is apparantly the best option (according to https://support.gamesparks.net/support/discussions/topics/1000053627)
        }

        public DataAttribute(string name, string data)
        {
            m_Name = name;
            m_Data = data;
        }

        private string m_Name;
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        private string m_Data;
        public string Data
        {
            get { return m_Data; }
            set { m_Data = value; }
        }

        public float GetDataAsFloat()
        {
            try
            {
                return float.Parse(m_Data, CultureInfo.InvariantCulture.NumberFormat);
            }
            catch(Exception e)
            {
                Debug.LogWarning(e);
            }

            return 0.0f;
        }
    }

    [SerializeField]
    private MenuManager m_MenuManager;

    private string m_DisplayName;
    public string DisplayName
    {
        get { return m_DisplayName; }
    }

    private string m_PlayerID;
    private string m_FacebookID;

    private Texture2D m_ProfilePicture;
    public Texture2D ProfilePicture
    {
        get { return m_ProfilePicture; }
    }

    private void Start()
    {
        //For testing purposes, always log out.
        GS.Reset();
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
            m_PlayerID = response.UserId;
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
            m_PlayerID = response.UserId;

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

    public bool IsLoggedIn()
    {
        return GS.Authenticated;
    }

    public string GetPlayerID()
    {
        return m_PlayerID;
    }

    public void SendData(string eventName, List<DataAttribute> attributes)
    {
        LogEventRequest request = new LogEventRequest();
        request.SetEventKey(eventName);
        
        //Parameters
        foreach (DataAttribute attribute in attributes)
        {
            request.SetEventAttribute(attribute.Name, attribute.Data);
        }

        request.Send((response) =>
        {
            if (!response.HasErrors)
            {
                Debug.Log(eventName + " succesfully sent!");
            }
            else
            {
                Debug.LogError("Error sending " + eventName);
            }
        });
    }

    public bool LoadData(string eventName, List<DataAttribute> attributes, List<string> returnValueNames, LoadDataCallback callback)
    {
        if (!IsLoggedIn())
            return false;

        LogEventRequest request = new LogEventRequest();
        request.SetEventKey(eventName);

        //Parameters
        foreach (DataAttribute attribute in attributes)
        {
            request.SetEventAttribute(attribute.Name, attribute.Data);
        }

        //Return values
        request.Send((response) =>
        {
            if (!response.HasErrors)
            {
                //For some reason the GSData lib can't read arrays???
                JSONNode rootNode = JSON.Parse(response.ScriptData.JSON);

                rootNode = rootNode["ReturnValue"];

                for (int i = 0; i < rootNode.Count; ++i)
                {
                    JSONNode mapNode = rootNode[i];

                    for (int j = 0; j < mapNode.Count; ++j)
                    {
                        List<DataAttribute> resultData = new List<DataAttribute>();

                        JSONNode playerNode = mapNode[j];

                        foreach (string attributeName in returnValueNames)
                        {
                            string result = playerNode[attributeName].Value;
                            resultData.Add(new DataAttribute(attributeName, result));
                        }

                        if (resultData.Count > 0)
                            callback(resultData);
                    }
                }

                Debug.Log(eventName + " succesfully loaded!");
            }
            else
            {
                Debug.LogError("Error Loading " + eventName);
            }
        });

        return true;
    }
}
