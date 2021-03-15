using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : Weapon
{
    public override float lifespan {get; set;} = 3;
    public override float speed {get; set;} = 5;
    enum State
    {
        moving,
        exploding
    }
    State state = State.moving;
    List<GameObject> alreadyBombed;
    [SerializeField] AudioSource explodeSound;

    void Update()
    {
        switch(state)
        {
            case State.moving:
            Fire();
            Homing();
            elapsedTime += Time.deltaTime;
            if(elapsedTime > lifespan)
            {
                Explosion();
            }
            break;

            case State.exploding:
            Collider[] hits = Physics.OverlapSphere(transform.position, 4f);
            foreach(Collider hit in hits)
            {
                if(enemyLayers.Contains(hit.gameObject.layer) && !alreadyBombed.Contains(hit.gameObject))
                {
                    hit.gameObject.GetComponentInParent<BasicMove>().Shot(gameObject);
                    alreadyBombed.Add(hit.gameObject);
                }
            }
            break;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if(enemyLayers.Contains(col.gameObject.layer))
        {
            Explosion();
        }
    }

    public override void SetupWeapon()
    {
        base.SetupWeapon();
        alreadyBombed = new List<GameObject>();
    }

    public override void Fire()
    {
        transform.parent = null;
        Vector3 myPos = transform.position;
        float deltaPos = speed*Time.deltaTime;
        transform.position = Vector3.MoveTowards(myPos, myPos + transform.forward*deltaPos, deltaPos);
    }

    // Called once
    void Explosion()
    {
        state = State.exploding;
        ParticleSystem particle = GetComponent<ParticleSystem>();
        particle.Stop();
        particle.TriggerSubEmitter(0);
        explodeSound.Play();
        Invoke("KillWeapon", 2.5f);
    }

    public override void KillWeapon()
    {
        base.KillWeapon();
        alreadyBombed.Clear();
        state = State.moving;
    }
}