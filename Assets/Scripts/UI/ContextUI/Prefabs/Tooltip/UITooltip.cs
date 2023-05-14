using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UITooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private string Text;

    [SerializeField]
    private GameObject ToolTipPrefab;

    private GameObject CurrentToolTip;
    private bool Hover;
    private float HoverTime;

    private void Update()
    {
        if (Hover && !CurrentToolTip)
        {
            HoverTime += Time.deltaTime;
            if (HoverTime >= 0.5f) FadeTooltip();

        }
    }
    enum Direction
    {
        top, bottom, left, right
    }

    [SerializeField] Direction TooltipDirection;
    [SerializeField] private float Offset;

    Vector2 SetDirection(Direction Dir)
    {
        switch (Dir)
        {
            case Direction.top:
                {
                    return new(0, 30);
                }
            case Direction.bottom:
                {
                    return new(0, -30);
                }
            case Direction.left:
                {
                    return new((-CurrentToolTip.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta.x / 6)  - Offset, 0);
                }
            case Direction.right:
                {

                    return new((CurrentToolTip.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().sizeDelta.x / 6) + Offset, 0);
                }
            default: return Vector2.zero;
        }

    }


    public void FadeTooltip()
    {
        StartCoroutine(TooltipStart());


    }

    public void OnPointerEnter(PointerEventData EventData)
    {
        Hover = true;
    }

    public void OnPointerExit(PointerEventData EventData)
    {
        Hover = false;
        HoverTime = 0;
        if (CurrentToolTip) CurrentToolTip.transform.GetChild(0).GetComponent<Animator>().SetTrigger("Exit");
        CurrentToolTip = null;
    }
    void Start()
    {
    }
    IEnumerator TooltipStart()
    {
        CurrentToolTip = Instantiate(ToolTipPrefab);
        CurrentToolTip.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = Text;
       // CurrentToolTip.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().fontSize = (float)(Screen.width * 0.05f * 1.2f);
        CurrentToolTip.transform.SetParent(transform);
        yield return 0;
        CurrentToolTip.GetComponent<RectTransform>().localPosition = SetDirection(TooltipDirection);


    }

}