using UnityEngine;
using System.Collections;

public class DragMap : MonoBehaviour
{
    [SerializeField]
    private float m_DragMultiplier;

    private Vector2 m_LastMousePosition;

    private void Update()
    {
        Vector2 currentPosition = Input.mousePosition;

        //On click
        if (Input.GetMouseButtonDown(0))
        {
            m_LastMousePosition = currentPosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector2 delta = currentPosition - m_LastMousePosition;

            transform.Translate(new Vector3(delta.x * m_DragMultiplier, 0.0f, delta.y * m_DragMultiplier));

            m_LastMousePosition = currentPosition;
            m_LastMousePosition = currentPosition;
        }
    }
}
