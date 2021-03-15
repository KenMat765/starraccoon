using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class TankMove : BasicMove
{
    Quaternion targetRot;
    [SerializeField] float maxRotSpeed;
    public int stickReverse;
    public int k;
    CameraController cameraController;
    bool canUpdateSpeed = true;
    public override int myPoint {get; set;} = 100;
    void Start()
    {
        base.GetAnim();
        GetObjects();
        cameraController = GetComponentInChildren<CameraController>();
        HPSetter();
    }
    void Update()
    {
        base.UpdateTrans();
        base.Movement();
        FourActionExe();
        if(HPSliderG.value <= 0 && !isDead)
        {
            StartCoroutine(Dead());
        }
    }
    public override void Rotation()
    {
        if(uGUIMannager.onStick)
        {
            float targetRotX = uGUIMannager.OriginalFuncY(maxBodyTiltX, k);
            float relativeRotY = uGUIMannager.OriginalFuncX(maxRotSpeed, k);
            float targetRotZ = uGUIMannager.OriginalFuncX(maxBodyTiltZ, k);
            targetRot = Quaternion.Euler(targetRotX*stickReverse*uTurndirection, MyRot.eulerAngles.y + relativeRotY, targetRotZ*-1*uTurndirection);
            transform.rotation = Quaternion.Slerp(MyRot, targetRot, 0.05f);
        }
        else
        {
            base.FixTilt();
        }
    }
    public void UpdateSpeed()
    {
        if(canUpdateSpeed)
        {
            base.speed = (maxSpeed - minSpeed)*uGUIMannager.norSSliderValue + minSpeed;
        }
    }
    void FourActionExe()
    {
        if(CSManager.swipeDown)
        {
            Uturn();
        }
        else if(CSManager.swipeUp)
        {
            Flip();
        }
        else if(CSManager.swipeLeft)
        {
            LeftRole(0);
        }
        else if(CSManager.swipeRight)
        {
            RightRole(0);
        }
    }
    public override IEnumerator uTurn()
    {
        StartCoroutine(base.uTurn());

        yield return new WaitForSeconds(0.33f);

        cameraController.CameraChange();
    }
    public override IEnumerator flip()
    {
        StartCoroutine(base.flip());
        canUpdateSpeed = false;
        speed = 0;

        yield return new WaitForSeconds(1.5f);

        canUpdateSpeed = true;
        speed = (maxSpeed - minSpeed)*uGUIMannager.norSSliderValue + minSpeed;
    }
    public override void Shot(GameObject hitBullet)
    {
        base.Shot(hitBullet);
        blastedSound.Play();
        uGUIMannager.damage.color = new Color(1,0,0,0.5f);
    }
    public override void Revival()
    {
        isDead = false;
        GetComponent<Rigidbody>().isKinematic = false;
        HPSetter();
        if(uTurndirection == -1)
        {
            uTurndirection = 1;
            cameraController.CameraChange();
        }
        this.enabled = true;
    }
    void OnCollisionEnter(Collision col)
    {
        if(attackScript.bulletLayers.Contains(col.gameObject.layer))
        {
            Shot(col.gameObject);
        }
    }
}
