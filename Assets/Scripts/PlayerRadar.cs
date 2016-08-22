using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Radar that checks & displays nearby players
public class PlayerRadar : MonoBehaviour
{
    [SerializeField]
    private float m_Delay;

    [SerializeField]
    private float m_Radius;

    [SerializeField]
    private Pool m_MarkerPool;
    private bool m_ResetMarkers = false;

    private void Start()
    {
        StartUpdate();
    }

    private void StartUpdate()
    {
        StartCoroutine(UpdateRoutine());
    }

    private void OnUpdateRoutineCompleted()
    {
        StartUpdate();
    }

    private IEnumerator UpdateRoutine()
    {
        m_ResetMarkers = true;

        Vector2 mapID = Extensions.WorldToTilePos(GPSManager.Instance.GetLatitude(), GPSManager.Instance.GetLongitude(), 17);

        List<GameSparksManager.DataAttribute> attributes = new List<GameSparksManager.DataAttribute>();
        attributes.Add(new GameSparksManager.DataAttribute("MAP_ID", mapID.x + "_" + mapID.y));

        List<string> attributeNames = new List<string>() { "PLAYER_ID", "LAT", "LON" };
        GameSparksManager.Instance.LoadData("LOAD_PLAYER_GPS", attributes, attributeNames, OnCoordinatesReceived);

        yield return new WaitForSeconds(m_Delay);

        OnUpdateRoutineCompleted();
    }

    private void OnCoordinatesReceived(List<GameSparksManager.DataAttribute> result)
    {
        //We reset all the markers when we receive the first update
        if (m_ResetMarkers)
        {
            m_MarkerPool.ResetAll();
            m_ResetMarkers = false;
        }

        //Don't have a marker for the local player
        string playerID = result[0].Data;
        if (playerID == GameSparksManager.Instance.GetPlayerID())
            return;

        float latitude = result[1].GetDataAsFloat();
        float longitude = result[2].GetDataAsFloat();

        Marker marker = m_MarkerPool.ActivateAvailableObject() as Marker;
        marker.SetLocation(latitude, longitude);
    }
}
