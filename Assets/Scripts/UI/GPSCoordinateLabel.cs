using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class GPSCoordinateLabel : MonoBehaviour
{
    private Text m_Text;

    //Cached data
    private GPSManager.CustomLocationInfo m_LocationInfo;
    private float m_LatitudeSinceLast;
    private float m_LongitudeSinceLast;
    private float m_HorizontalAccuracySinceLast;
    private double m_TimeSinceLast;

    private void Awake()
    {
        m_Text = GetComponent<Text>();
    }

    private void Start()
    {
        m_LocationInfo = GPSManager.Instance.GetLocationInfo();
    }

    private void Update()
    {
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        GPSManager gpsManager = GPSManager.Instance;

        switch (gpsManager.GetState())
        {
            case GPSManager.GPSState.NotRunning:
                m_Text.text = "Initializing GPS...";
                break;

            case GPSManager.GPSState.Disabled:
                m_Text.text = "Please enable the GPS.";
                break;

            case GPSManager.GPSState.TimedOut:
                m_Text.text = "GPS timed out!";
                break;

            case GPSManager.GPSState.Failed:
                m_Text.text = "GPS failed to locate.";
                break;

            case GPSManager.GPSState.Running:
            {
                double timeSinceInit = gpsManager.GetTimestamp() - gpsManager.GetInitializationTime();
                double timeSinceLast = gpsManager.GetTimestamp() - m_LocationInfo.timestamp;

                if (timeSinceLast > 0.0)
                {
                    m_LatitudeSinceLast = m_LocationInfo.latitude - gpsManager.GetLatitude();
                    m_LongitudeSinceLast = m_LocationInfo.longitude - gpsManager.GetLongitude();
                    m_HorizontalAccuracySinceLast = m_LocationInfo.horizontalAccuracy - gpsManager.GetHorizontalAccuracy();
                    m_TimeSinceLast = timeSinceLast;
                }

                m_LocationInfo = gpsManager.GetLocationInfo();

                m_Text.text = "Location: " + gpsManager.GetLatitude() + " " +
                gpsManager.GetLongitude() + " " +
                gpsManager.GetHorizontalAccuracy() + " " +
                timeSinceInit;


                m_Text.text += "\nDiff: " + m_LatitudeSinceLast + " " +
                                m_LongitudeSinceLast + " " +
                                m_HorizontalAccuracySinceLast + " " +
                                m_TimeSinceLast;


                break;
            }

            default:
                m_Text.text = "Unidentified gps error.";
                break;
        }
    }

}
