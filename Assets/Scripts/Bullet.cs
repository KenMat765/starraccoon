using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Bullet : Weapon
{
    public override float lifespan {get; set;} = 1;
    public override float speed {get; set;} = 40;

    void Update()
    {
        elapsedTime += Time.deltaTime;
        if(elapsedTime > lifespan)
        {
            KillWeapon();
        }
    }

    void OnEnable()
    {
        PreHoming();
        Fire();
    }

    void OnCollisionEnter(Collision col)
    {
        KillWeapon();
    }

    public override void Fire()
    {
        transform.parent = null;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.velocity = transform.forward*speed;
    }

    public override void KillWeapon()
    {
        base.KillWeapon();
        GetComponent<Rigidbody>().isKinematic = true;
    }
}