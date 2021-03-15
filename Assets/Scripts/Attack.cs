using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class Attack : MonoBehaviour
{
    public abstract GameObject originalNormalBullet {get; set;}
    List<GameObject> normalBullets {get; set;}
    public abstract float normalInterval {get; set;}
    float normalElapsedTime {get; set;}
    public GameObject energyCanon {get; set;}
    public float canonElapsedTime {get; set;}
    // 40 
    public float canonInterval {get; set;} = 25;
    public List<GameObject> homingTargets;
    public abstract float homingAngle {get; set;}
    public abstract float homingDist {get; set;}
    public List<int> bodyLayers {get; private set;}
    public List<int> bulletLayers {get; set;}
    /*[SerializeField] AudioSource crownGenerateSound;
    [SerializeField] AudioSource crownBlastSound;*/
    public static float normalDistance = 40f;
    public ParticleSystem blastImpact {get; set;}
    public Vector3 MyPos {get; set;}
    public Quaternion MyRot {get; set;}
    Vector3 BPos;
    Quaternion BRot;
    // Call GetObjects() in Start() Method in every attack script
    public virtual void GetObjects()
    {
        blastImpact = transform.Find("BlastImpact").GetComponent<ParticleSystem>();
    }
    public void UpdateTrans()
    {
        MyPos = transform.position;
        MyRot = transform.rotation;
        BPos = originalNormalBullet.transform.position;
        BRot = originalNormalBullet.transform.rotation;
    }
    public void SetLayerInteggers()
    {
        //fighterbodyの全Layers(奇数)
        int[] layersInt = {9,11,13,15,29};
        //　↑　手動で追加　↑

        bodyLayers = new List<int>();
        bodyLayers.AddRange(layersInt);
        bodyLayers.Remove(gameObject.layer);

        //弾丸の全Layers(偶数)
        int[] bulletInt = {10,12,14,16,30};
        //　↑　手動で追加　↑

        bulletLayers = new List<int>();
        bulletLayers.AddRange(bulletInt);
        bulletLayers.Remove(originalNormalBullet.layer);
    }

    //↓ Object Pooling  ↓
    //Start()で呼ぶ
    //PoolするのはnormalBulletのみ
    public void PoolNormalBullets(int quantity)
    {
        normalBullets = new List<GameObject>();
        for(int k = 0; k < quantity; k++)
        {
            var bullet = Instantiate(originalNormalBullet, BPos, BRot, transform);
            normalBullets.Add(bullet);
        }
    }
    public GameObject GetNormalBullet()
    {
        foreach(GameObject normalBullet in normalBullets)
        {
            //activeSelf == true : 使用中
            //activeSelf == false : 待機中
            if(normalBullet.activeSelf == false)
            {
                // normalBullet.SetActive(true);
                return normalBullet;
            }
        }
        //全て使用中だったら新たに作成
        var newBullet = Instantiate(originalNormalBullet, BPos, BRot, transform);
        normalBullets.Add(newBullet);
        return newBullet;
    }
    //↑ Object Pooling ↑

    public void NormalBlast()
    {
        normalElapsedTime += Time.deltaTime;
        if(normalElapsedTime > normalInterval)
        {
            Bullet bullet = GetNormalBullet().GetComponent<Bullet>();
            if(homingTargets.Count > 0 && homingTargets[0] != null)
            {
                bullet.Activate(homingTargets[0]);
            }
            else
            {
                bullet.Activate(null);
            }
            blastImpact.Play();
            normalElapsedTime = 0;
        }
    }
    public void EnergyCanon()
    {
        if(!GetComponentInParent<BasicMove>().isDead)
        {
            Bomb bomb = energyCanon.GetComponent<Bomb>();
            bomb.Activate((homingTargets.Count > 0)? homingTargets[0] : null);
            uGUIMannager.Canon.interactable = false;
            canonElapsedTime = 0;
        }
    }
    public virtual void EnergyCharger()
    {
        canonElapsedTime += Time.deltaTime;
    }
    /*public void Crown()
    {
        StartCoroutine(crown());
    }
    IEnumerator crown()
    {
        float crownSpeed = 30f;
        float crownPower = 0.3f;
        float crownTime = 1f;
        GameObject[] crownBullets = new GameObject[5];
        float r = 0.1f;
        //弾丸生成
        for(int k = 0; k < 5; k++)
        {
            crownBullets[k] = Instantiate(originalNormalBullet, MyPos + new Vector3(r*Mathf.Cos(Mathf.PI/10 + 2*Mathf.PI*k/5)*Mathf.Cos(MyRot.eulerAngles.y*Mathf.PI/180),r*Mathf.Sin(Mathf.PI/10 + 2*Mathf.PI*k/5),r*Mathf.Cos(Mathf.PI/10 + 2*Mathf.PI*k/5)*Mathf.Sin(MyRot.eulerAngles.y*Mathf.PI/180)*-1), BRot);
            crownBullets[k].SetActive(true);
            crownGenerateSound.Play();
            yield return new WaitForSeconds(0.25f);
        }
        //弾丸発射
        for(int k = 0; k < 5; k++)
        {
            crownBullets[k].GetComponent<Rigidbody>().velocity = crownBullets[k].transform.forward * crownSpeed;
            crownBullets[k].GetComponent<Bullet>().bulletPower = crownPower;
            crownBlastSound.Play();
            Destroy(crownBullets[k], crownTime);
            yield return new WaitForSeconds(0.25f);
        }
    }*/
    public void GetHomingTarget()
    {
        if(Physics.OverlapSphere(BPos, homingDist).Length > 0)
        {
            LayerMask terrain = LayerMask.GetMask("Default");
            var possibleTargets = Physics.OverlapSphere(BPos, homingDist).Select(t => t.transform.gameObject);
            homingTargets = possibleTargets.Where(p => Vector3.Angle(transform.forward, p.transform.position - MyPos) < homingAngle && bodyLayers.Contains(p.layer) && !Physics.Raycast(MyPos, p.transform.position - MyPos, Vector3.Magnitude(p.transform.position - MyPos), terrain)).ToList();
        }
        else
        {
            homingTargets.Clear();
        }
    }
}