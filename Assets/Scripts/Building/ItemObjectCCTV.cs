using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObjectCCTV : ItemObject
{
    [SerializeField] private Transform cameraHeadTransform;
    [SerializeField] private Camera securityCamera;

    private float cameraRotY;

    protected override void Awake()
    {
        base.Awake();

        StartCoroutine(rotateIEnum());
    }

    protected override void Start()
    {
        base.Start();

        CameraController.Instance.RegisterAdditionalCamera(securityCamera);
    }

    protected override void Update()
    {
        base.Update();

        cameraHeadTransform.transform.localRotation = Quaternion.Euler(new Vector3(cameraHeadTransform.localRotation.eulerAngles.x, cameraRotY, cameraHeadTransform.localRotation.eulerAngles.z));
    }

    public override void SetSelected(bool selected)
    {
        base.SetSelected(selected);

        securityCamera.gameObject.SetActive(selected);

        if(selected)
        {
            HUD.Instance.SecurityCameraView.Show();
        }
        else
        {
            HUD.Instance.SecurityCameraView.Hide();
        }
    }

    private IEnumerator rotateIEnum()
    {
        float rotMax = 60;
        float rotSpeed = 30;

        while(true)
        {
            while(cameraRotY < 60)
            {
                cameraRotY += Time.deltaTime * rotSpeed;
                yield return null;
            }

            yield return new WaitForSeconds(1.0f);

            while (cameraRotY > -rotMax)
            {
                cameraRotY -= Time.deltaTime * rotSpeed;
                yield return null;
            }

            yield return new WaitForSeconds(1.0f);
        }
    }
}
