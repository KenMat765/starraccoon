using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;

public abstract class BasicMove : MonoBehaviour
{
    public Vector3 MyPos {get; set;}
    public Quaternion MyRot {get; set;}
    //Sub Class で設定する。常に存在する。
    public Vector3 targetPos {get; private set;}
    public Vector3 relativePos {get; set;}

    //Sub Class で設定する。存在しない場合がある。
    public Vector3? subTargetPos {get; set;}
    public Vector3? relativeSubPos {get; set;}
    public float rotationSpeed {get; set;}
    public float speed {get; set;}
    public float maxSpeed;
    public float minSpeed;
    public Animator anim {get; set;}
    public int uTurndirection {get; set;} = 1;

    //right:1 left:-1
    public int rollDirection {get; set;} = 1;
    public float rollSpeed {get; set;}
    //縦
    public static float maxBodyTiltX {get; private set;} = 55;
    //左右
    public static float maxBodyTiltZ {get; private set;} = 60;
    public float tiltFixingTime {get; set;} = 0.05f;
    public Slider HPSliderR;
    public Slider HPSliderG;
    public Image HPSliderImg;
    public float maxHP {get; set;} = 100;
    //main と sub のどちらに到着しても true にする。
    public bool mainArrived {get; set;} = true;
    public bool subArrived {get; set;}
    public float FarNearBorder {get; private set;} = 5f;
    public bool ready4action {get; set;} = true;
    public bool rollReady {get; set;} = true;
    Vector3? latestSubTargetPos;
    public Attack attackScript {get; private set;}
    public AudioSource blastedSound;
    public ParticleSystem splash;
    public ParticleSystem explosionDead;
    public AudioSource deadSound;
    public bool isDead {get; set;}
    public Revive revive {get; private set;}
    public int lastShooterLayer {get; set;}
    public abstract int myPoint {get; set;}
    public string myName;
    public float uturnTime {get; private set;} public float flipTime {get; private set;} public float rollTime {get; private set;}
    public void GetAnim()
    {
        anim = GetComponentInChildren<Animator>();
        var rac = anim.runtimeAnimatorController;
        uturnTime = rac.animationClips.Where(a => a.name == "U-Turn").Select(b => b.length).ToArray()[0];
        flipTime = rac.animationClips.Where(a => a.name == "Flip").Select(b => b.length).ToArray()[0];
        rollTime = rac.animationClips.Where(a => a.name == "RightRole").Select(b => b.length).ToArray()[0];
    }
    public void GetObjects()
    {
        revive = GetComponent<Revive>();
        attackScript = GetComponentInChildren<Attack>();
    }
    public virtual void UpdateTrans()
    {
        MyPos = transform.position;
        MyRot = transform.rotation;
    }
    public void HPSetter()
    {
        HPSliderG.maxValue = maxHP;
        HPSliderG.value = maxHP;
        HPSliderR.maxValue = maxHP;
        HPSliderR.value = maxHP;
    }
    public void HPColorChanger()
    {
        if(HPSliderG.value < HPSliderG.maxValue/4)
        {
            HPSliderImg.color = new Color(0.906f, 0, 0.008f, 1);
        }
        else if(HPSliderG.value < HPSliderG.maxValue/2)
        {
            HPSliderImg.color = new Color(0.933f, 0.898f, 0, 1);
        }
        else
        {
            HPSliderImg.color = new Color(0, 0.843f, 0.224f, 1);
        }
    }
    public virtual void Shot(GameObject weapon)
    {
        lastShooterLayer = weapon.layer;
        splash.Play();
        HPSliderG.value -= weapon.GetComponent<Weapon>().power;
        Tween redDecreaser = DOTween.To(() => HPSliderR.value, x => HPSliderR.value = x, HPSliderG.value, 1f);
    }
    public void TargetSetter(Vector3 targetPosSet, bool setMain)
    {
        if(setMain)
        {
            targetPos = targetPosSet;
        }
        if(subArrived)
        {
            LayerMask rayMask = LayerMask.GetMask("Default");
            if(Physics.Raycast(MyPos, relativePos, Vector3.Magnitude(relativePos), rayMask))
            {
                //20
                float searchRaidus = 20;
                LayerMask sphereMask = LayerMask.GetMask("SubTarget");
                List<Vector3> subTargetPosesAround = Physics.OverlapSphere(MyPos, searchRaidus, sphereMask, QueryTriggerInteraction.Collide).Select(s => s.transform.position).Where(t => !Physics.Raycast(MyPos, t - MyPos, Vector3.Magnitude(t - MyPos), rayMask)).ToList();
                subTargetPosesAround.Remove(latestSubTargetPos.GetValueOrDefault());
                float degree = 180;
                foreach(Vector3 subTargetPosAround in subTargetPosesAround)
                {
                    Vector3 relativeSubTargetPosAround = subTargetPosAround - MyPos;
                    if(Mathf.Abs(Vector3.SignedAngle(relativePos, relativeSubTargetPosAround, Vector3.up)) < degree)
                    {
                        degree = Mathf.Abs(Vector3.SignedAngle(relativePos, relativeSubTargetPosAround, Vector3.up));
                        subTargetPos = subTargetPosAround;
                        latestSubTargetPos = subTargetPos;
                    }
                }
            }
            else
            {
                subTargetPos = null;
                latestSubTargetPos = null;
            }
        }
    }
    public void SpeedSetter(float speedSet, float rotationSpeedSet)
    {
        speed = speedSet;
        rotationSpeed = rotationSpeedSet;
    }
    public void ArrivalJudge()
    {
        if(Vector3.SqrMagnitude(relativePos) < Mathf.Sqrt(FarNearBorder))
        {
            mainArrived = true;
        }
        else
        {
            mainArrived = false;
        }
        if(Vector3.SqrMagnitude(relativeSubPos.GetValueOrDefault()) < Mathf.Sqrt(FarNearBorder) && subArrived == false)
        {
            subArrived = true;
        }
        else
        {
            subArrived = false;
        }
    }
    public virtual void Movement()
    {
        //move
        transform.position = Vector3.MoveTowards(MyPos, MyPos + transform.forward*uTurndirection*speed*Time.deltaTime + transform.right*uTurndirection*rollDirection*rollSpeed*Time.deltaTime, Mathf.Sqrt(Mathf.Pow(speed*Time.deltaTime,2) + Mathf.Pow(rollSpeed*Time.deltaTime,2)));

        //rotation
        Rotation();
    }
    public abstract void Rotation();
    public void FixTilt()
    {
        transform.rotation = Quaternion.Slerp(MyRot, Quaternion.Euler(0, MyRot.eulerAngles.y, 0), tiltFixingTime);
    }
    public void Uturn()
    {
        if(ready4action)
        {
            StartCoroutine(uTurn());
        }
    }
    public virtual IEnumerator uTurn()
    {
        ready4action = false;
        anim.SetInteger("FighterAnim",2*uTurndirection);

        yield return new WaitForSeconds(0.33f);

        uTurndirection *= -1;
        anim.SetInteger("FighterAnim",uTurndirection);

        yield return new WaitForSeconds(uturnTime - 0.33f);

        ready4action = true;
    }
    public void Flip()
    {
        if(ready4action)
        {
            StartCoroutine(flip());
        }
    }
    public virtual IEnumerator flip()
    {
        ready4action = false;
        anim.SetInteger("FighterAnim",3*uTurndirection);

        yield return new WaitForSeconds(1.5f);

        anim.SetInteger("FighterAnim",uTurndirection);

        yield return new WaitForSeconds(flipTime - 1.5f);

        ready4action = true;
    }
    public void LeftRole(float delay)
    {
        if(ready4action && rollReady)
        {
            StartCoroutine(leftrole(delay));
        }
    }
    public virtual IEnumerator leftrole(float delay)
    {
        ready4action = false;
        rollReady = false;
        anim.SetInteger("FighterAnim",5*uTurndirection);
        rollDirection = -1;
        rollSpeed = 1f;

        yield return new WaitForSeconds(rollTime);

        anim.SetInteger("FighterAnim",uTurndirection);
        rollSpeed = 0;
        ready4action = true;

        yield return new WaitForSeconds(delay);

        rollReady = true;
    }
    public void RightRole(float delay)
    {
        if(ready4action && rollReady)
        {
            StartCoroutine(rightrole(delay));
        }
    }
    public virtual IEnumerator rightrole(float delay)
    {
        ready4action = false;
        rollReady = false;
        anim.SetInteger("FighterAnim",4*uTurndirection);
        rollDirection = 1;
        rollSpeed = 1f;

        yield return new WaitForSeconds(rollTime);

        anim.SetInteger("FighterAnim",uTurndirection);
        rollSpeed = 0;
        ready4action = true;

        yield return new WaitForSeconds(delay);

        rollReady = true;
    }
    public virtual IEnumerator Dead()
    {
        Death();

        yield return new WaitForSeconds(revive.revivalTime);

        Revival();

        yield return new WaitForSeconds(3);

        ready4action = true;
    }
    public void Death()
    {
        explosionDead.Play();
        deadSound.Play();
        isDead = true;
        GetComponent<Rigidbody>().isKinematic = true;
        transform.Find("fighterbody").gameObject.SetActive(false);
        ready4action = false;
        this.enabled = false;
        revive.enabled = true;
        GivePoint();
    }
    public abstract void Revival();
    public virtual void GivePoint()
    {
        if(lastShooterLayer == LayerMask.NameToLayer("Bullet1"))
        {
            uGUIMannager.points[0] += myPoint;
            uGUIMannager.BookRepo("PLAYER", myName, Color.red);
        }
        else if(lastShooterLayer == LayerMask.NameToLayer("Bullet2"))
        {
            uGUIMannager.points[1] += myPoint;
            uGUIMannager.BookRepo("COM1", myName, Color.blue);
        }
        else if(lastShooterLayer == LayerMask.NameToLayer("Bullet3"))
        {
            uGUIMannager.points[2] += myPoint;
            uGUIMannager.BookRepo("COM2", myName, Color.green);
        }
        else if(lastShooterLayer == LayerMask.NameToLayer("Bullet4"))
        {
            uGUIMannager.points[3] += myPoint;
            uGUIMannager.BookRepo("COM3", myName, Color.yellow);
        }
        else
        {
            uGUIMannager.BookRepo("ZAKO", myName, Color.gray);
        }
    }
}