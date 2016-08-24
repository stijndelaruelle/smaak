using UnityEngine;
using System.Collections;
using Assets.Helpers;
using System.Collections.Generic;

[RequireComponent(typeof(MeshRenderer))]
public class Marker : PoolableObject
{
    [SerializeField]
    private bool m_UseLocalGPS;

    [SerializeField]
    private float m_Latitude;

    [SerializeField]
    private float m_Longitude;

    [SerializeField]
    private int m_Zoom = 0;


    private MeshRenderer m_Renderer;

    private void Awake()
    {
        m_Renderer = GetComponent<MeshRenderer>();
        m_Renderer.enabled = false;
    }

    private void Update()
    {
        if (!m_UseLocalGPS)
            return;

        GPSManager gpsManager = GPSManager.Instance;
        m_Renderer.enabled = (gpsManager.GetState() == GPSManager.GPSState.Running);

        if (m_Renderer.enabled == false)
            return;

        m_Latitude = gpsManager.GetLatitude();
        m_Longitude = gpsManager.GetLongitude();

        UpdateMarker();
    }

    public void SetLocation(float latitude, float longitude)
    {
        if (m_UseLocalGPS)
        {
            Debug.LogWarning("Unable to set location as this marker follows the local GPS");
        }

        m_Renderer.enabled = true;
        m_Latitude = latitude;
        m_Longitude = longitude;

        UpdateMarker();
    }

    private void UpdateMarker()
    {
        Vector2 mapID = Extensions.WorldToTilePos(m_Latitude, m_Longitude, m_Zoom);
        Rect rect = GM.TileBounds(mapID, m_Zoom);

        Vector2 bm = GM.LatLonToMeters(m_Latitude, m_Longitude);
        Vector2 pm = new Vector2(bm.x - rect.center.x, bm.y - rect.center.y);

        Vector2 offset = World.Instance.CalculateOffset(mapID);
        pm.x += offset.x;
        pm.y += offset.y;

        transform.localPosition = new Vector3(pm.x, transform.position.y, pm.y);
    }

    #region PoolableObject

    public override void Initialize()
    {
    }

    public override void Activate()
    {
        gameObject.SetActive(true);
    }

    public override void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public override bool IsAvailable()
    {
        return (!gameObject.activeSelf);
    }

    #endregion
}
