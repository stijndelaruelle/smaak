using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour
{
    [SerializeField]
    private InputField m_UserNameField;

    [SerializeField]
    private InputField m_PasswordField;

    public void PressedLogin()
    {
        GameSparksManager.Instance.Login(m_UserNameField.text, m_PasswordField.text);
    }

    public void PressedFacebookLogin()
    {
        FacebookManager.Instance.FacebookLogin();
    }
}
