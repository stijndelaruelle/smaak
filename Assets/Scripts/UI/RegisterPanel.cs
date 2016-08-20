using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RegisterPanel : MonoBehaviour
{
    [SerializeField]
    private InputField m_UserNameField;

    [SerializeField]
    private InputField m_PasswordField;

    [SerializeField]
    private InputField m_DisplayNameField;

    public void PressedRegister()
    {
        GameSparksManager.Instance.Register(m_UserNameField.text, m_PasswordField.text, m_DisplayNameField.text);
    }
}

