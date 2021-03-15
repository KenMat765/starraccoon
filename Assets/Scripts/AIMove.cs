using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[DefaultExecutionOrder(-1)]
public class AIMove : BasicMove
{
    enum Conditions
    {
        attack,
        search,
        // heal,
        goBack,
        farEscape,
        flip,
        counter
    }
    Conditions condition = Conditions.search;
    GameObject targetCraft;
    //MainTarget, SubTarget両方に使用。常に存在する。
    float relativeYAngle;
    GameObject Shooter;
    Vector3 SPos;
    Vector3 relativeSPos;
    float relativeSAngle;
    List<Vector3> allSubTargetPos;
    bool underAttack;
    int hitBulletCount;
    float hitInterval;
    public override int myPoint {get; set;} = 100;
    void Start()
    {
        GetAnim();
        GetObjects();
        HPSetter();
        allSubTargetPos = GameObject.FindGameObjectsWithTag("SubTarget").Select(t => t.transform.position).ToList();
    }
    void Update()
    {
        UpdateTrans();
        ChangeCondition();
        ActionOnEachCondition();
        // Blast on any condition when homingTargets.Count is larget than 0
        if(attackScript.homingTargets.Count > 0)
        {
            attackScript.NormalBlast();
        }
        if(underAttack)
        {
            hitInterval += Time.deltaTime;
        }
        if(hitInterval > 6)
        {
            underAttack = false;
            hitBulletCount = 0;
            hitInterval = 0;
        }
        if(HPSliderG.value <= 0 && !isDead)
        {
            StartCoroutine(Dead());
        }
        ArrivalJudge();
        Movement();
    }
    public override void UpdateTrans()
    {
        base.UpdateTrans();
        relativePos = targetPos - MyPos;
        relativeYAngle = (relativeSubPos == null)? (Vector3.SignedAngle(transform.forward * uTurndirection, relativePos, Vector3.up)) : (Vector3.SignedAngle(transform.forward * uTurndirection, relativeSubPos.GetValueOrDefault(), Vector3.up));
        relativeSubPos = (subTargetPos == null)? (null) : (subTargetPos - MyPos);
        SPos = (Shooter != null) ? Shooter.transform.position : Vector3.zero;
        relativeSPos = (Shooter != null) ? SPos - MyPos : Vector3.zero;
        relativeSAngle = (Shooter != null) ? Vector3.SignedAngle(transform.forward*uTurndirection, relativeSPos, Vector3.up) : 0;
    }
    public override void Movement()
    {
        //前進と方向転換
        base.Movement();
        //４アクション
        if(Vector3.SqrMagnitude((relativeSubPos == null)? relativePos : relativeSubPos.GetValueOrDefault()) >= Mathf.Sqrt(FarNearBorder))
        {
            if(relativeYAngle >= -60 && relativeYAngle <= -45)
            {
                LeftRole(3);
            }
            else if(relativeYAngle >= 45 && relativeYAngle <= 60)
            {
                RightRole(3);
            }
            else if(relativeYAngle <= -135 || relativeYAngle >= 135)
            {
                Uturn();
            }
            else
            {
                return;
            }
        }
        else
        {
            if(relativeYAngle <= -135 || relativeYAngle >= 135)
            {
                Uturn();
            }
            else
            {
                return;
            }
        }
    }
    public override void Rotation()
    {
        Vector3 targetAngle = (subTargetPos == null)? (Quaternion.LookRotation(((relativePos == Vector3.zero)? transform.forward : relativePos)*uTurndirection).eulerAngles) : (Quaternion.LookRotation(((relativeSubPos.GetValueOrDefault() == Vector3.zero)? transform.forward : relativeSubPos.GetValueOrDefault())*uTurndirection).eulerAngles);
        if(relativeYAngle < -135)
        {
            targetAngle.z = ((-maxBodyTiltZ/45)*relativeYAngle - 4*maxBodyTiltZ)*uTurndirection*-1;
        }
        else if(-135 <= relativeYAngle && relativeYAngle <= 135)
        {
            float normY = relativeYAngle/135;
            targetAngle.z = (maxBodyTiltZ/2)*(Mathf.Pow(normY, 3) - 3*normY)*uTurndirection;
        }
        else
        {
            targetAngle.z = ((-maxBodyTiltZ/45)*relativeYAngle + 4*maxBodyTiltZ)*uTurndirection*-1;
        }
        Quaternion lookRotation = Quaternion.Euler(targetAngle);
        transform.rotation = Quaternion.Slerp(MyRot, lookRotation, rotationSpeed*Time.deltaTime);
    }
    void ChangeCondition() 
    {
        //Under Attack
        if(underAttack)
        {
            targetCraft = null;
            // When Shooter is Alive
            if(Shooter.activeSelf == true)
            {
                if(relativeSAngle >= -30 && relativeSAngle <= 30)
                {
                    condition = Conditions.counter;
                }
                else if(relativeSAngle <= -135 || relativeSAngle >= 135)
                {
                    if(Vector3.SqrMagnitude(relativeSPos) < FarNearBorder)
                    {
                        condition = Conditions.flip;
                    }
                    else
                    {
                        condition = Conditions.farEscape;
                    }
                }
                else
                {
                    condition = Conditions.goBack;
                }
            }
            // When Shooter is Dead
            else
            {
                condition = Conditions.search;
                underAttack = false;
            }
        }

        //Not Under Attack
        else
        {
            Shooter = null;
            if(attackScript.homingTargets.Count > 0)
            {
                condition = Conditions.attack;
                if(targetCraft == null)
                {
                    targetCraft = attackScript.homingTargets[0];
                }
            }
            else
            {
                condition = Conditions.search;
                targetCraft = null;
            }
            // else if(HPSliderG.value >= maxHP/2)
            // {
            //     condition = Conditions.search;
            //     subTargetNum = Random.Range(0, allSubTargetPos.Count);
            //     targetCraft = null;
            // }
            // else
            // {
            //     condition = Conditions.heal;
            //     targetCraft = null;
            // }
        }
    }
    void ActionOnEachCondition()
    {
        switch(condition)
        {
            // Not Under Attack
            case Conditions.attack:
                SpeedSetter(Mathf.Lerp(speed, (maxSpeed + minSpeed)/1.5f, 0.05f), Mathf.Lerp(rotationSpeed, 0.8f, 0.1f));
                if(targetCraft != null)
                {
                    TargetSetter(targetCraft.transform.position, true);
                }
            break;

            case Conditions.search:
                SpeedSetter(Mathf.Lerp(speed, (maxSpeed + minSpeed)/1.2f, 0.1f), Mathf.Lerp(rotationSpeed, 0.8f, 0.05f));
                if(mainArrived)
                {
                    TargetSetter(allSubTargetPos[Random.Range(0, allSubTargetPos.Count)], true);
                }
                else if(subArrived)
                {
                    TargetSetter(allSubTargetPos[Random.Range(0, allSubTargetPos.Count)], false);
                }
            break;

            // case Conditions.heal:
            // break;

            // Under Attack
            case Conditions.goBack:
                TargetSetter(SPos + Shooter.transform.forward * -FarNearBorder, true);
                SpeedSetter(Mathf.Lerp(speed, maxSpeed, 0.05f), Mathf.Lerp(rotationSpeed, 0.5f, 0.1f));
            break;

            case Conditions.farEscape:
                SpeedSetter(Mathf.Lerp(speed, maxSpeed, 0.05f), Mathf.Lerp(rotationSpeed, 0.8f, 0.1f));
                if(mainArrived)
                {
                    TargetSetter(allSubTargetPos[Random.Range(0, allSubTargetPos.Count)], true);
                }
                else if(subArrived)
                {
                    TargetSetter(allSubTargetPos[Random.Range(0, allSubTargetPos.Count)], false);
                }
            break;

            case Conditions.flip:
                Flip();
            break;

            case Conditions.counter:
                TargetSetter(Shooter.transform.position, true);
                SpeedSetter(Mathf.Lerp(speed, (maxSpeed + minSpeed)/2, 0.05f), Mathf.Lerp(rotationSpeed, 0.8f, 0.5f));
            break;
        }
    }
    public override void Shot(GameObject weapon)
    {
        if(weapon.tag == "MyBullet")
        {
            blastedSound.Play();
        }
        base.Shot(weapon);
        hitBulletCount ++;
        if(hitBulletCount > 10)
        {
            hitInterval = 0;
            underAttack = true;
            Shooter = weapon.GetComponent<Weapon>().owner;
        }
    }
    // ↓ Called in IEnumerator Dead() ↓
    public override void Revival()
    {
        isDead = false;
        GetComponent<Rigidbody>().isKinematic = false;
        HPSetter();
        underAttack = false;
        if(uTurndirection == -1)
        {
            uTurndirection = 1;
        }
        this.enabled = true;
    }
    // ↑ Called in IEnumerator Dead() ↑
    void OnCollisionEnter(Collision col)
    {
        if(attackScript.bulletLayers.Contains(col.gameObject.layer))
        {
            Shot(col.gameObject);
        }
    }
    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.blue;
    //     Gizmos.DrawLine(MyPos, MyPos + relativePos);
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawLine(MyPos, MyPos + relativeSubPos.GetValueOrDefault());
    //     Gizmos.color = Color.white;
    // }
}
