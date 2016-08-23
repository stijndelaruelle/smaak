using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GPSManager : Singleton<GPSManager>
{
    public struct CustomLocationInfo
    {
        //Exact copy of Location info, but we can edit it (needed for debug purposes)

        public float altitude { get; set; }
        public float horizontalAccuracy { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
        public double timestamp { get; set; }
        public float verticalAccuracy { get; set; }
    }

    public enum GPSState
    {
        NotRunning,
        Running,
        Disabled,
        TimedOut,
        Failed,
    }

    //Events
    private Action m_GPSInitializedEvent;
    public Action GPSInitializedEvent
    {
        get { return m_GPSInitializedEvent; }
        set { m_GPSInitializedEvent = value; }
    }

    //Variables
    protected int m_TimeoutTime;
    public int TimeoutTime
    {
        get { return m_TimeoutTime; }
        set { m_TimeoutTime = value; }
    }

    protected GPSState m_GPSState;
    protected double m_InitializeTime;
    private CustomLocationInfo m_LastSendLocation; //The location that was last send to the server

    //Functions
    private void Start()
    {
        StartTracking();
    }

    private void OnDisable()
    {
        StopTracking();
    }


    protected virtual void StartTracking()
    {

    }

    protected void OnStartTrackingCompleted()
    {
        if (m_GPSInitializedEvent != null)
        {
            m_GPSInitializedEvent();
        }
    }

    protected virtual void StopTracking()
    {
        m_GPSState = GPSState.NotRunning;
        m_InitializeTime = 0;
    }

    protected virtual void Update()
    {
        if (!GameSparksManager.Instance.IsLoggedIn())
            return;

        //If the difference between where we are and our LAST SEND location is too great, send a new location to the server
        CustomLocationInfo currentLocation = GetLocationInfo();

        bool update = false;

        if (Mathf.Abs(currentLocation.latitude - m_LastSendLocation.latitude) > 0) update = true;
        if (Mathf.Abs(currentLocation.longitude - m_LastSendLocation.longitude) > 0) update = true;

        if (update)
        {
            List<GameSparksManager.DataAttribute> attributes = new List<GameSparksManager.DataAttribute>();

            Vector2 mapID = Extensions.WorldToTilePos(currentLocation.latitude, currentLocation.longitude, 17);

            attributes.Add(new GameSparksManager.DataAttribute("MAP_ID", mapID.x + "_" + mapID.y));
            attributes.Add(new GameSparksManager.DataAttribute("LAT", currentLocation.latitude));
            attributes.Add(new GameSparksManager.DataAttribute("LON", currentLocation.longitude));

            //Only send this if we changed map to save data
            Vector2 prevMapID = Extensions.WorldToTilePos(m_LastSendLocation.latitude, m_LastSendLocation.longitude, 17);
            if (mapID != prevMapID)
            {
                attributes.Add(new GameSparksManager.DataAttribute("PREV_MAP_ID", prevMapID.x + "_" + prevMapID.y));
            }

            //otherwise just send a 0 to minimize data
            else
            {
                attributes.Add(new GameSparksManager.DataAttribute("PREV_MAP_ID", "0"));
            }

            GameSparksManager.Instance.SendData("SAVE_PLAYER_GPS", attributes);

            Debug.Log("Saved new GPS data! " + mapID.x + " " + mapID.y);
            m_LastSendLocation = currentLocation;
        }
    }

    public virtual CustomLocationInfo GetLocationInfo()
    {
        CustomLocationInfo info = new CustomLocationInfo();
        return info;
    }

    public virtual float GetLatitude()
    {
        return 0.0f;
    }

    public virtual float GetLongitude()
    {
        return 0.0f;
    }

    public virtual float GetHorizontalAccuracy()
    {
        return 0.0f;
    }

    public virtual double GetTimestamp()
    {
        return 0.0f;
    }

    public GPSState GetState()
    {
        return m_GPSState;
    }

    public double GetInitializationTime()
    {
        return m_InitializeTime;
    }

    public bool IsInitialized()
    {
        return (m_GPSState == GPSState.Running);
    }
}
