using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Revive : MonoBehaviour
{
    public float revivalTime;
    public float elapsedTime {get; private set;}
    GameObject fighterbody;
    Vector3 startPos;
    Quaternion startRot;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        fighterbody = transform.Find("fighterbody").gameObject;
        this.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        if(elapsedTime > revivalTime)
        {
            transform.position = startPos;
            transform.rotation = startRot;
            fighterbody.transform.localPosition = Vector3.zero;
            fighterbody.SetActive(true);
            elapsedTime = 0;
            this.enabled = false;
        }
    }
}
