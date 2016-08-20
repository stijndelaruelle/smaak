using UnityEngine;
using System.Collections;
using Assets.Helpers;

[RequireComponent (typeof(SpriteRenderer))]
public class Marker : MonoBehaviour
{
    [SerializeField]
    private float m_Latitude;

    [SerializeField]
    private float m_Longitude;

    [SerializeField]
    private int m_Zoom = 0;

    [SerializeField]
    private World m_World;

    private SpriteRenderer m_Renderer;

    private void Awake()
    {
        m_Renderer = GetComponent<SpriteRenderer>();
        m_Renderer.enabled = false;
    }

    private void Update()
    {
        //GPSManager gpsManager = GPSManager.Instance;
        //m_Renderer.enabled = (gpsManager.GetState() == GPSManager.GPSState.Running);

        //if (m_Renderer.enabled == false)
        //    return;

        //m_Latitude = gpsManager.GetLatitude();
        //m_Longitude = gpsManager.GetLongitude();

        UpdateMarker();
    }

    private void UpdateMarker()
    {
        Vector2 mapID = Extensions.WorldToTilePos(m_Latitude, m_Longitude, m_Zoom);
        Rect rect = GM.TileBounds(mapID, m_Zoom);

        Vector2 bm = GM.LatLonToMeters(m_Latitude, m_Longitude);
        Vector2 pm = new Vector2(bm.x - rect.center.x, bm.y - rect.center.y);

        Vector2 offset = m_World.CalculateOffset(mapID);
        pm.x += offset.x;
        pm.y += offset.y;

        transform.position = new Vector3(pm.x, 1.0f, pm.y);
    }
}
