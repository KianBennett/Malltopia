using TMPro;
using UnityEngine;

public class ObjectContextMenu : ContextMenuRoot
{
    [SerializeField]
    private TMP_Text Name;

    private void Start()
    {
        Name.text = PlayerController.Instance.SelectedObject.DisplayText;
    }


    public void DeleteObject()
    {
        if (PlayerController.Instance.SelectedObject)
        {

            if(PlayerController.Instance.SelectedObject == GetComponent<ItemObjectTill>())
            {

            }
            else
            {

            Destroy(PlayerController.Instance.SelectedObject.gameObject);
            PlayerController.Instance.SelectedObject.PlayRemovalSound();
            }
            CloseContextMenu();
        }
    }

    public void Locate()
    {
        CameraController.Instance.SetPositionImmediate(PlayerController.Instance.SelectedObject.transform.position);
    }
}