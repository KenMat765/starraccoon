using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MyAttack : Attack
{
    public override GameObject originalNormalBullet {get; set;}
    public override float homingAngle {get; set;} = 20;
    public override float homingDist {get; set;} = 20;
    public override float normalInterval {get; set;} = 0.15f;
    void Start()
    {
        originalNormalBullet = transform.Find("Normal_Bullet").gameObject;
        UpdateTrans();
        SetLayerInteggers();
        PoolNormalBullets(5);
        GetObjects();
    }
    public override void GetObjects()
    {
        base.GetObjects();
        energyCanon = transform.Find("EnergyCanon").gameObject;
    }
    void Update()
    {
        UpdateTrans();
        GetHomingTarget();
        if(uGUIMannager.onBlast)
        {
            NormalBlast();
        }
        if(homingTargets.Count > 0)
        {
            uGUIMannager.LockOnManager(homingTargets[0]);
        }
        else
        {
            uGUIMannager.LockOnManager(null);
        }
        EnergyCharger();
    }
    public override void EnergyCharger()
    {
        base.EnergyCharger();
        uGUIMannager.CanonImg.fillAmount = canonElapsedTime/canonInterval;
        if(canonElapsedTime > canonInterval)
        {
            uGUIMannager.Canon.interactable = true;
        }
    }
}
