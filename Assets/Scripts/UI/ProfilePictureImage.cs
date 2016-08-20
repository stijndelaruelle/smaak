using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ProfilePictureImage : MonoBehaviour
{
    private Image m_Image;

    private void Awake()
    {
        m_Image = GetComponent<Image>();
    }

    private void OnEnable()
    {
        UpdateImage();
    }

    private void UpdateImage()
    {
        Texture2D texture = GameSparksManager.Instance.ProfilePicture;

        if (texture != null)
            m_Image.sprite = Sprite.Create(texture, new Rect(0, 0, 200, 200), new Vector2(0, 0));
    }
}
