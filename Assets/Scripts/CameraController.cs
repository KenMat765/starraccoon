using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    GameObject maincamera;
    GameObject backcamera;
    Camera mcamera;
    Camera bcamera;
    public static Camera currentCam;
    void Start()
    {
        maincamera = transform.Find("Main Camera").gameObject;
        backcamera = transform.Find("Back Camera").gameObject;
        mcamera = maincamera.GetComponent<Camera>();
        bcamera = backcamera.GetComponent<Camera>();
        currentCam = mcamera;
    }
    void Update()
    {
        CameraRotation();
        ViewChange();
    }
    void CameraRotation()
    {
        Quaternion rootRot = transform.root.rotation;
        gameObject.transform.rotation = Quaternion.Euler(rootRot.eulerAngles.x, rootRot.eulerAngles.y, 0);
    }
    public void CameraChange()
    {
        if(maincamera.activeSelf && !backcamera.activeSelf)
        {
            maincamera.SetActive(false);
            backcamera.SetActive(true);
            currentCam = bcamera;
        }
        else if(!maincamera.activeSelf && backcamera.activeSelf)
        {
            backcamera.SetActive(false);
            maincamera.SetActive(true);
            currentCam = mcamera;
        }
    }
    void ViewChange()
    {
        float maxView = 90;
        float minView = 30;
        float norSliderValue = uGUIMannager.norSSliderValue;
        mcamera.fieldOfView = Mathf.Lerp(mcamera.fieldOfView, (maxView - minView) * norSliderValue + minView, 0.1f);
        bcamera.fieldOfView = Mathf.Lerp(bcamera.fieldOfView, (maxView - minView) * norSliderValue + minView, 0.1f);
    }
}
