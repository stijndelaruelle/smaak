using UnityEngine;
using System.Collections;

public class ZoomCamera : MonoBehaviour
{
    [SerializeField]
    private Camera m_Camera;

    [SerializeField]
    private float m_ZoomMultiplier;

    private bool m_IsZooming = false;
    public bool IsZooming
    {
        get { return m_IsZooming; }
    }

    private float m_PreviousLength;

    private void Update()
    {
        #if UNITY_EDITOR
            Vector2 firstPosition = Input.mousePosition;
            Vector2 secondPosition = Vector2.zero;

            if (Input.GetMouseButtonDown(3))
            {
                m_PreviousLength = (firstPosition - secondPosition).sqrMagnitude;
            }

            if (Input.GetMouseButtonUp(3))
            {
                m_IsZooming = false;
            }

            if (Input.GetMouseButton(3))
            {
        #else
            Touch firstTouch = Input.GetTouch(0);
            Touch secondTouch = Input.GetTouch(1);

            if (firstTouch.phase == TouchPhase.Ended || secondTouch.phase == TouchPhase.Ended)
            {
                m_IsZooming = false;
                return;
            }

            Vector2 firstPosition = firstTouch.position;
            Vector2 secondPosition = secondTouch.position;  

            //Reset
            if (secondTouch.phase == TouchPhase.Began)
            {
                m_PreviousLength = (firstPosition - secondPosition).sqrMagnitude;
            }
        #endif

            float length = (firstPosition - secondPosition).sqrMagnitude; //Sqr as we only compare it to ourselves
            float diff = length - m_PreviousLength; //positive = zoom out

            m_Camera.orthographicSize += (diff * m_ZoomMultiplier);

            //Cap this for the demo
            if (m_Camera.orthographicSize < 50.0f)
                m_Camera.orthographicSize = 50.0f;

            if (m_Camera.orthographicSize > 255.0f)
                m_Camera.orthographicSize = 255.0f;

            m_IsZooming = true;
            m_PreviousLength = length;

        #if UNITY_EDITOR
            }
        #endif
    }
}
