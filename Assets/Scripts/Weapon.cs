using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    // Pool weapons as child gameobjects of fighterbody
    // Set every weapon deactivated in default
    // Deactivate weapon when not used to make sure OnEnable() Method is called when used
    // Original Normal Bullet will never be activated
    public GameObject targetObject {get; set;}
    // owner is figherbody, not Fighter
    public GameObject owner {get; private set;}
    public List<int> enemyLayers {get; private set;}
    Vector3 startPos;
    Quaternion startRot;
    public abstract float lifespan {get; set;}
    public float elapsedTime {get; set;}
    public float power;
    public abstract float speed {get; set;}

    void Awake()
    {
        SetupWeapon();
    }
    public virtual void SetupWeapon()
    {
        owner = transform.parent.gameObject;
        startPos = transform.localPosition;
        startRot = transform.localRotation;
        enemyLayers = owner.GetComponent<Attack>().bodyLayers;
    }

    // Call Activate() in each attack script to use weapon
    public void Activate(GameObject target)
    {
        targetObject = target;
        gameObject.SetActive(true);
    }

    // Call PreHoming() in OnEnabled() Method
    public void PreHoming()
    {
        if(targetObject != null)
        {
            Vector3 relativePos = targetObject.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(relativePos);
        }
    }

    // Call Fire() in OnEnabled() Method
    // Fire() is unique to each weapon
    public abstract void Fire();

    // Call Homing() in Update() Method
    public void Homing()
    {
        if(targetObject != null)
        {
            Vector3 relativePos = targetObject.transform.position - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(relativePos), 0.1f);
        }
    }

    public virtual void KillWeapon()
    {
        transform.parent = owner.transform;
        transform.localPosition = startPos;
        transform.localRotation = startRot;
        elapsedTime = 0;
        gameObject.SetActive(false);
    }
}