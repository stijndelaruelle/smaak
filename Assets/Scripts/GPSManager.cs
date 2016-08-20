using UnityEngine;
using System.Collections;

public class GPSManager : Singleton<GPSManager>
{
    public enum GPSState
    {
        NotRunning,
        Running,
        Disabled,
        TimedOut,
        Failed,
    }

    [SerializeField]
    private int m_TimeoutTime;

    private GPSState m_GPSState;
    private double m_InitializeTime;

    private void OnEnable()
    {
        StartTracking();
    }

    private void OnDisable()
    {
        StopTracking();
    }

    private void StartTracking()
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

    private void StopTracking()
    {
        m_GPSState = GPSState.NotRunning;
        Input.location.Stop();
    }


    public GPSState GetState()
    {
        return m_GPSState;
    }

    public LocationInfo GetLocationInfo()
    {
        return Input.location.lastData;
    }

    public float GetLongitude()
    {
        return Input.location.lastData.longitude;
    }

    public float GetLatitude()
    {
        return Input.location.lastData.latitude;
    }

    public float GetHorizontalAccuracy()
    {
        return Input.location.lastData.horizontalAccuracy;
    }

    public double GetTimestamp()
    {
        return Input.location.lastData.timestamp;
    }

    public double GetInitializationTime()
    {
        return m_InitializeTime;
    }
}
