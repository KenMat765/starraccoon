using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EnemyAttack : Attack
{
    public override GameObject originalNormalBullet {get; set;}
    public override float homingAngle {get; set;} = 30;
    public override float homingDist {get; set;} = 50;
    public override float normalInterval {get; set;} = 0.15f;
    public bool visible {get; private set;}

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
        EnergyCharger();
    }
    public override void EnergyCharger()
    {
        base.EnergyCharger();
        if(canonElapsedTime > canonInterval && homingTargets.Count > 3)
        {
            EnergyCanon();
        }
    }
    void OnBecameVisible()
    {
        visible = true;
    }
    void OnBecameInvisible()
    {
        visible = false;
    }
}
