using UnityEngine;
using System.Collections;

public class ManagerInitializer : MonoBehaviour
{
    [SerializeField]
    private int m_GPSTimeoutTime;

    //Depending if we are in debug or not we make managers
	private void Awake()
    {
        #if UNITY_EDITOR

            GPSManagerDebug gpsManager = gameObject.AddComponent<GPSManagerDebug>();
            gpsManager.TimeoutTime = m_GPSTimeoutTime;

        #else

            GPSManagerMobile gpsManager = gameObject.AddComponent<GPSManagerMobile>();
            gpsManager.TimeoutTime = m_GPSTimeoutTime;

        #endif
	}
}
