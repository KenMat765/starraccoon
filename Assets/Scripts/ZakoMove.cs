using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ZakoMove : BasicMove
{
    float relativeYAngle;
    List<Vector3> allSubTargetPos;
    float HP = 10;
    public override int myPoint {get; set;} = 20;

    void Start()
    {
        GetObjects();
        allSubTargetPos = GameObject.FindGameObjectsWithTag("SubTarget").Select(t => t.transform.position).ToList();
        SpeedSetter(1.5f, 1.5f);
    }

    void Update()
    {
        UpdateTrans();
        if(HP <= 0 && !isDead)
        {
            StartCoroutine(Dead());
        }
        if(attackScript.homingTargets.Count > 0 && !isDead)
        {
            Vector3 target = attackScript.homingTargets[0].transform.position;
            TargetSetter(target, true);
            attackScript.NormalBlast();
        }
        else
        {
            if(mainArrived)
            {
                TargetSetter(allSubTargetPos[Random.Range(0, allSubTargetPos.Count)], true);
            }
            else if(subArrived)
            {
                TargetSetter(allSubTargetPos[Random.Range(0, allSubTargetPos.Count)], false);
            }
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
    }
    public override void Movement()
    {
        transform.position = Vector3.MoveTowards(MyPos, MyPos + transform.forward*speed*Time.deltaTime, speed*Time.deltaTime);
        Rotation();
    }
    public override void Rotation()
    {
        Vector3 relativeEulerAngle = (subTargetPos == null)? (Quaternion.LookRotation(((relativePos == Vector3.zero)? transform.forward : relativePos)*uTurndirection).eulerAngles) : (Quaternion.LookRotation(((relativeSubPos.GetValueOrDefault() == Vector3.zero)? transform.forward : relativeSubPos.GetValueOrDefault())*uTurndirection).eulerAngles);
        Quaternion lookRotation = Quaternion.Euler(relativeEulerAngle.x, relativeEulerAngle.y, Mathf.Clamp(-relativeYAngle * 1.5f, -maxBodyTiltZ, maxBodyTiltZ));
        transform.rotation = Quaternion.Slerp(MyRot, lookRotation, rotationSpeed * Time.deltaTime);
    }
    public override void Shot(GameObject weapon)
    {
        if(weapon.tag == "MyBullet")
        {
            blastedSound.Play();
        }
        lastShooterLayer = weapon.layer;
        splash.Play();
        HP -= weapon.GetComponent<Weapon>().power;
    }
    public override IEnumerator Dead()
    {
        Death();

        yield return new WaitForSeconds(revive.revivalTime);

        Revival();
    }
    public override void Revival()
    {
        this.enabled = true;
        isDead = false;
        GetComponent<Rigidbody>().isKinematic = false;
        HP = 5;
    }
    void OnCollisionEnter(Collision col)
    {
        if(attackScript.bulletLayers.Contains(col.gameObject.layer))
        {
            Shot(col.gameObject);
        }
    }
    public override void GivePoint()
    {
        if(lastShooterLayer == LayerMask.NameToLayer("Bullet1"))
        {
            uGUIMannager.points[0] += myPoint;
        }
        else if(lastShooterLayer == LayerMask.NameToLayer("Bullet2"))
        {
            uGUIMannager.points[1] += myPoint;
        }
        else if(lastShooterLayer == LayerMask.NameToLayer("Bullet3"))
        {
            uGUIMannager.points[2] += myPoint;
        }
        else if(lastShooterLayer == LayerMask.NameToLayer("Bullet4"))
        {
            uGUIMannager.points[3] += myPoint;
        }
    }
}
