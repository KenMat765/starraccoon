using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KariCamera : MonoBehaviour
{
    Camera main;
    Camera back;
    [SerializeField] AIMove aIMove;
    // Start is called before the first frame update
    void Start()
    {
        main = transform.Find("MainCamera").GetComponent<Camera>();
        back = transform.Find("BackCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        CameraRotation();
        if(aIMove.uTurndirection == 1)
        {
            main.enabled = true;
            back.enabled = false;
        }
        else
        {
            back.enabled = true;
            main.enabled = false;
        }
    }
    void CameraRotation()
    {
        Quaternion rootRot = transform.root.rotation;
        gameObject.transform.rotation = Quaternion.Euler(rootRot.eulerAngles.x, rootRot.eulerAngles.y, 0);
    }
}
