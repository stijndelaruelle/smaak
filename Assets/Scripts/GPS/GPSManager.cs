using UnityEngine;
using System.Collections;

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

    protected int m_TimeoutTime;
    public int TimeoutTime
    {
        get { return m_TimeoutTime; }
        set { m_TimeoutTime = value; }
    }

    protected GPSState m_GPSState;
    protected double m_InitializeTime;

    private void OnEnable()
    {
        StartTracking();
    }

    private void OnDisable()
    {
        StopTracking();
    }


    protected virtual void StartTracking()
    {
        StartCoroutine(StartTrackingRoutine());
    }

    private IEnumerator StartTrackingRoutine()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            m_GPSState = GPSState.Disabled;
            yield return null;
        }

        Input.location.Start();

        //Gather data
        int maxWait = m_TimeoutTime;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service timed out
        if (maxWait <= 0)
        {
            m_GPSState = GPSState.TimedOut;
            yield return null;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            m_GPSState = GPSState.Failed;
            yield return null;
        }

        m_InitializeTime = Input.location.lastData.timestamp;
        m_GPSState = GPSState.Running;
    }

    protected virtual void StopTracking()
    {
        m_GPSState = GPSState.NotRunning;
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
}
