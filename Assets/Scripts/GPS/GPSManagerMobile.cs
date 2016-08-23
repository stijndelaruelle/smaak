using UnityEngine;
using System.Collections;
using System;

public class GPSManagerMobile : GPSManager
{
    //GPSManager
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

        OnStartTrackingCompleted();
    }

    protected override void StopTracking()
    {
        base.StopTracking();
        Input.location.Stop();
    }


    public override CustomLocationInfo GetLocationInfo()
    {
        LocationInfo origInfo = Input.location.lastData;
        CustomLocationInfo info = new CustomLocationInfo();

        info.latitude = origInfo.latitude;
        info.longitude = origInfo.longitude;
        info.timestamp = origInfo.timestamp;

        info.altitude = origInfo.altitude;
        info.horizontalAccuracy = origInfo.horizontalAccuracy;
        info.verticalAccuracy = origInfo.verticalAccuracy;

        return info;
    }

    public override float GetLatitude()
    {
        return Input.location.lastData.latitude;
    }

    public override float GetLongitude()
    {
        return Input.location.lastData.longitude;
    }

    public override float GetHorizontalAccuracy()
    {
        return Input.location.lastData.horizontalAccuracy;
    }

    public override double GetTimestamp()
    {
        return Input.location.lastData.timestamp;
    }
}
