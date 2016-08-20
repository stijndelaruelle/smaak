using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent (typeof(Text))]
public class DisplayNameLabel : MonoBehaviour
{
    private Text m_Text;

    private void Awake()
    {
        m_Text = GetComponent<Text>();
    }

    private void OnEnable()
    {
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        m_Text.text = "Hallo " + GameSparksManager.Instance.DisplayName + "!";
    }
}
