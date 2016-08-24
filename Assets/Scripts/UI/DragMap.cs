using UnityEngine;
using System.Collections;

public class DragMap : MonoBehaviour
{
    [SerializeField]
    private ZoomCamera m_ZoomCamera;

    [SerializeField]
    private float m_DragMultiplier;

    private float m_StartCameraOrthographic;
    private Vector2 m_LastMousePosition;

    private void Start()
    {
        m_StartCameraOrthographic = Camera.main.orthographicSize;
    }

    private void Update()
    {
        if (m_ZoomCamera.IsZooming)
            return;

        Vector2 currentPosition = Input.mousePosition;

        //On click
        if (Input.GetMouseButtonDown(0))
        {
            m_LastMousePosition = currentPosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 delta = currentPosition - m_LastMousePosition;

            float zoomMultiplier = Camera.main.orthographicSize / m_StartCameraOrthographic;
            transform.Translate(new Vector3(delta.x * m_DragMultiplier * zoomMultiplier, 0.0f, delta.y * m_DragMultiplier * zoomMultiplier));

            m_LastMousePosition = currentPosition;
            m_LastMousePosition = currentPosition;
        }
    }
}
