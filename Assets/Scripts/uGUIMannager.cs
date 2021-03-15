using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using System;
using DG.Tweening;

public class uGUIMannager : MonoBehaviour
{
    static RectTransform canvasRect;
    public static Slider speedSlider {get; private set;}
    public static bool onStick {get; private set;}
    public static bool onBlast {get; private set;}
    public static Vector2 firstStickPos;
    public static Vector2 diffPosStandard {get; private set;}
    public static Image leftStick {get; private set;}
    public static Image leftStickBack {get; private set;}
    public static Image Blast {get; private set;}
    public static Button Canon {get; private set;}
    public static Image CanonImg {get; private set;}
    public static Image LockOn {get; private set;}
    int Id;
    static AudioSource lockOnSound;
    static GameObject player;
    public static GameObject lastTarget;
    static GameObject[] enemies;
    static Image[] enemyHps;
    static EnemyAttack[] enemyScripts;
    public static int[] points;
    public static Text[] pointTexts;
    float gameTime = 180;
    //1ゲーム/3分
    static Text gameTimeTex;
    public static Image damage;
    Text reviveCount;
    Text killed;
    Text reviveIn;
    TankMove tankMove;
    static GameObject destroyRepo;
    static Text destroyerTex;
    static Image arrow;
    static Text destroyedTex;
    static bool seqPlaying;
    static List<Sequence> sequences;
    static Sequence currSeq;

    //UI
    void Start()
    {
        canvasRect = GetComponent<RectTransform>();
        leftStick = transform.Find("LeftStick").gameObject.GetComponent<Image>();
        leftStickBack = transform.Find("LeftStickBack").gameObject.GetComponent<Image>();
        speedSlider = GetComponentInChildren<Slider>();
        firstStickPos = leftStick.rectTransform.position;
        speedSlider.value = (speedSlider.maxValue + speedSlider.minValue)/2;
        Blast = transform.Find("Blast").GetComponent<Image>();
        Canon = transform.Find("Canon").GetComponent<Button>();
        CanonImg = transform.Find("Canon").GetComponent<Image>();
        LockOn = transform.Find("LockOn").gameObject.GetComponent<Image>();
        lockOnSound = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player");
        // 敵の数
        int enemyNum = 3;
        enemies = new GameObject[enemyNum];
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        enemyHps = new Image[enemyNum];
        enemyScripts = new EnemyAttack[enemyNum];
        for(int k = 0; k < enemyNum; k++)
        {
            enemyHps[k] = enemies[k].transform.Find("HPCanvas/Background").gameObject.GetComponent<Image>();
            enemyScripts[k] = enemies[k].GetComponent<EnemyAttack>();
        }
        points = new int[enemyNum + 1]; pointTexts = new Text[enemyNum + 1];
        pointTexts = transform.Find("Points").GetComponentsInChildren<Text>();
        for(int k = 0; k < enemyNum + 1; k++)
        {
            points[k] = 0;
            pointTexts[k].text = 0.ToString();
        }
        gameTimeTex = transform.Find("GameTime").GetComponent<Text>();
        damage = transform.Find("Damage").GetComponent<Image>();
        reviveCount = transform.Find("ReviveCount").GetComponent<Text>();
        killed = transform.Find("Killed").GetComponent<Text>();
        reviveIn = transform.Find("ReviveIn").GetComponent<Text>();
        tankMove = player.GetComponent<TankMove>();
        destroyRepo = transform.Find("DestroyRepo").gameObject;
        destroyerTex = destroyRepo.transform.Find("Destroyer").GetComponent<Text>();
        arrow = destroyRepo.transform.Find("Arrow").GetComponent<Image>();
        destroyedTex = destroyRepo.transform.Find("Destroyed").GetComponent<Text>();
        sequences = new List<Sequence>();
    }

    void Update()
    {
        LeftStickMannager();
        BlastManager();
        EnemyHPManager();
        GameTimeManager();
        ScrImgManager();
        PointManager();
        ReportKill();
    }
    void GameTimeManager()
    {
        gameTime -= Time.deltaTime;
        gameTimeTex.text = Mathf.CeilToInt(gameTime).ToString();
        if(gameTime <= 0)
        {
            SceneManager.LoadScene("Title");
            MainMenu.nowScore = points[0];
            int highestPoint = Mathf.Max(points[0], points[1], points[2], points[3]);
            if(highestPoint == points[0])
            {
                MainMenu.winner = "PLAYER";
                MainMenu.winnerColor = Color.red;
            }
            else if(highestPoint == points[1])
            {
                MainMenu.winner = "COM1";
                MainMenu.winnerColor = Color.blue;
            }
            else if(highestPoint == points[2])
            {
                MainMenu.winner = "COM2";
                MainMenu.winnerColor = Color.green;
            }
            else if(highestPoint == points[3])
            {
                MainMenu.winner = "COM3";
                MainMenu.winnerColor = Color.yellow;
            }
        }
    }
    void LeftStickMannager()
    {
        if(CSManager.startPoses.Any(s => (s - ThreeToTwo(leftStick)).magnitude < leftStick.rectTransform.rect.width/2))
        {
            onStick = true;
            //Left Stick にのった指のIdを取得
            Id = Array.IndexOf(CSManager.startPoses, CSManager.startPoses.Where(s => (s - ThreeToTwo(leftStick)).magnitude < leftStick.rectTransform.rect.width/2).ToArray()[0]);
        }
        if(onStick)
        {
            float diffPosMaxMag = leftStickBack.rectTransform.rect.width/3;
            Vector2 diffPos;
            if((CSManager.currentPoses[Id] - firstStickPos).magnitude < diffPosMaxMag)
            {
                diffPos = CSManager.currentPoses[Id] - firstStickPos;
            }
            else
            {
                diffPos = (CSManager.currentPoses[Id] - firstStickPos).normalized*diffPosMaxMag;
            }
            leftStick.rectTransform.position = firstStickPos + diffPos;
            diffPosStandard = diffPos/diffPosMaxMag;
            //Left Stick にのっていた指が離れたとき (= startPoses[Id] が 0 に戻った時)
            if(CSManager.startPoses[Id] == Vector2.zero)
            {
                onStick = false;
                leftStick.rectTransform.position = firstStickPos;
                diffPosStandard = Vector2.zero;
            }
        }
    }
    void BlastManager()
    {
        if(CSManager.startPoses.Any(s => (s - ThreeToTwo(Blast)).magnitude < Blast.rectTransform.rect.width/2))
        {
            onBlast = true;
        }
        else
        {
            onBlast = false;
        }
    }
    public static void LockOnManager(GameObject currentTarget)
    {
        if(currentTarget == null)
        {
            Color defaultColor = new Color(0.84f, 0.84f, 0.84f);
            LockOn.rectTransform.position = RectTransformUtility.WorldToScreenPoint(CameraController.currentCam, player.transform.position + player.transform.forward*Attack.normalDistance);
            LockOn.color = defaultColor;
            lastTarget = null;
        }
        else
        {
            LockOn.rectTransform.position = RectTransformUtility.WorldToScreenPoint(CameraController.currentCam, currentTarget.transform.position);
            LockOn.color = Color.red;
            if(currentTarget != lastTarget)
            {
                lockOnSound.Play();
                lastTarget = currentTarget;
            }
        }
    }
    void EnemyHPManager()
    {
        for(int k = 0; k < enemies.Length; k++)
        {
            if(enemies[k] != null && enemyScripts[k].visible == true)
            {
                Vector2 enemyScreenPos = RectTransformUtility.WorldToScreenPoint(CameraController.currentCam, enemies[k].transform.position);
                enemyHps[k].rectTransform.position = enemyScreenPos + new Vector2(0, 50);
            }
            else
            {
                //とにかく画面外に出す(-50に特に意味はない)
                enemyHps[k].rectTransform.position = new Vector2(-50, -50);
            }
        }
    }
    void ScrImgManager()
    {
        if(!tankMove.isDead)
        {
            damage.color = Color.Lerp(damage.color, Color.clear, 0.2f);
            reviveCount.color = Color.clear;
            reviveCount.text = "";
            killed.color = Color.clear;
            reviveIn.color = Color.clear;
        }
        else
        {
            damage.color = new Color(1,0,0,0.5f);
            reviveCount.text = Mathf.Ceil(tankMove.revive.revivalTime - tankMove.revive.elapsedTime).ToString();
            killed.color = Color.Lerp(killed.color, Color.white, 0.1f);
            if(tankMove.revive.elapsedTime > tankMove.revive.revivalTime - 5)
            {
                reviveCount.color = Color.white;
                reviveIn.color = Color.white;
            }
        }
    }
    void PointManager()
    {
        for(int k = 0; k < points.Length; k++)
        {
            int currPoint = int.Parse(pointTexts[k].text);
            if(currPoint < points[k])
            {
                currPoint ++;
                pointTexts[k].text = currPoint.ToString();
            }
        }
    }
    public static Vector2 ThreeToTwo(Image image)
    {
        float x = image.rectTransform.position.x;
        float y = image.rectTransform.position.y;
        return new Vector2(x,y);
    }
    void ReportKill()
    {
        foreach(Sequence sequence in sequences)
        {
            if(sequence != null && !seqPlaying)
            {
                sequence.Play();
                currSeq = sequence;
                seqPlaying = true;
            }
        }
    }
    public static void BookRepo(string destroyer, string destroyed, Color arrowColor)
    {   
        Sequence newSeq = DOTween.Sequence()
            .OnStart(() => {destroyerTex.text = destroyer; destroyedTex.text = destroyed; arrow.color = arrowColor;})
            .Append(destroyRepo.transform.DOLocalMoveX(-645, 0.2f))
            .Append(arrow.DOColor(Color.clear, 0.2f).SetLoops(5, LoopType.Yoyo)). Join(destroyerTex.DOColor(Color.clear, 0.2f).SetLoops(5, LoopType.Yoyo)). Join(destroyedTex.DOColor(Color.clear, 0.2f).SetLoops(5, LoopType.Yoyo))
            .Append(arrow.DOColor(arrowColor, 0.2f)).Join(destroyerTex.DOColor(Color.white, 0.2f)).Join(destroyedTex.DOColor(Color.white, 0.2f))
            .Append(destroyRepo.transform.DOLocalMoveX(-1100, 0.2f).SetDelay(3))
            .OnComplete(() => {sequences.Remove(currSeq); seqPlaying = false;}); 
        sequences.Add(newSeq);
    }
    public static float norSSliderValue 
    {
        get{return speedSlider.value / speedSlider.maxValue;}
    }
    public static float OriginalFuncX(float maxValue, float k)
    {
        return (maxValue/(k-1)) * (Mathf.Pow(k, Mathf.Abs(diffPosStandard.x)) - 1) * Mathf.Sign(diffPosStandard.x);
    }
    public static float OriginalFuncY(float maxValue, float k)
    {
        return (maxValue/(k-1)) * (Mathf.Pow(k, Mathf.Abs(diffPosStandard.y)) - 1) * Mathf.Sign(diffPosStandard.y);
    }
    public static Vector3 StoC(Vector3 screenPos)
    {
        float ratioX = screenPos.x/Screen.width;
        float ratioY = screenPos.y/Screen.height;
        Vector3 canvasPos = new Vector3((ratioX - 0.5f)*canvasRect.rect.width, (ratioY - 0.5f)*canvasRect.rect.height);
        return canvasPos;
    }
}
