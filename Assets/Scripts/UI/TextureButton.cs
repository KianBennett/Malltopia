using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TextureButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private RawImage textureImage;
    [SerializeField] private Image outlineImage;
    [SerializeField] private RectTransform rect;
    [SerializeField] private Color outlineColourSelected;
    [SerializeField] private Color outlineColourNormal;

    public RectTransform Rect { get { return rect; } }

    public void SetTexture(Texture texture)
    {
        textureImage.texture = texture;
    }

    public void SetOnClick(UnityAction action)
    {
        button.onClick.AddListener(action);
    }

    public void SetSelected(bool selected)
    {
        outlineImage.color = selected ? outlineColourSelected : outlineColourNormal;
        button.interactable = !selected;
    }
}
