using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZakoAttack : Attack
{
    public override GameObject originalNormalBullet {get; set;}
    public override float homingAngle {get; set;} = 10;
    public override float homingDist {get; set;} = 10;
    public override float normalInterval {get; set;} = 1f;
    // Start is called before the first frame update
    void Start()
    {
        originalNormalBullet = transform.Find("Normal_Bullet").gameObject;
        UpdateTrans();
        SetLayerInteggers();
        PoolNormalBullets(5);
        GetObjects();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTrans();
        GetHomingTarget();
    }
}
