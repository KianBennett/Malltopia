using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float Scale;
    [SerializeField] private float ScaleTime;
    private bool Active;

    private void Update()
    {
        Vector3 a = new Vector3(Scale / ScaleTime * Time.deltaTime, Scale / ScaleTime * Time.deltaTime, Scale / ScaleTime * Time.deltaTime);
        if (Active)
        {
            transform.localScale += a;
        }
        else
        {
            transform.localScale -= a;
        }
        float num = Mathf.Clamp(transform.localScale.x, 1, Scale);
        transform.localScale = new Vector3(num, num, num);
    }

    public void OnPointerEnter(PointerEventData EventData)
    {
        Active = true;
    }

    public void OnPointerExit(PointerEventData EventData)
    {
        Active = false;
    }
}