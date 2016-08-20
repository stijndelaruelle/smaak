using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject m_GamePanel;

    [SerializeField]
    private GameObject m_LoginPanel;

    [SerializeField]
    private GameObject m_RegisterPanel;

    private void Awake()
    {
        ShowLoginPanel();   
    }

    public void ShowGamePanel()
    {
        m_GamePanel.SetActive(true);
        m_LoginPanel.SetActive(false);
        m_RegisterPanel.SetActive(false);
    }

    public void ShowLoginPanel()
    {
        m_GamePanel.SetActive(false);
        m_LoginPanel.SetActive(true);
        m_RegisterPanel.SetActive(false);
    }

    public void ShowRegisterPanel()
    {
        m_GamePanel.SetActive(false);
        m_LoginPanel.SetActive(false);
        m_RegisterPanel.SetActive(true);
    }
}
