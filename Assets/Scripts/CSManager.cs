using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
public class CSManager : MonoBehaviour
{
    public static bool isTouching;

    //↓　LeftStickとBlast以外でのスワイプのみ検知　↓
    public static bool swipeUp;
    public static bool swipeDown;
    public static bool swipeRight;
    public static bool swipeLeft;
    int detectableNum = 4;
    Touch[] currentTouches;
    public static Vector2[] startPoses;
    public static Vector2[] currentPoses;
    float[] swipeSpeeds;

    void Start()
    {
        currentTouches = new Touch[detectableNum];
        startPoses = new Vector2[detectableNum];
        currentPoses = new Vector2[detectableNum];
        swipeSpeeds = new float[detectableNum];
    }

    void Update()
    {
        if(Input.touchCount > 0)
        {
            isTouching = true;
            for(int i = 0; i < Mathf.Min(Input.touchCount, detectableNum); i ++)
            {
                Touch touch = Input.GetTouch(i);
                int Id = touch.fingerId;
                if(Id < detectableNum)
                {
                    currentTouches[Id] = touch;
                }
            }
            foreach(Touch currentTouch in currentTouches)
            {
                switch(currentTouch.phase)
                {
                    case TouchPhase.Began:
                    startPoses[Array.IndexOf(currentTouches, currentTouch)] = currentTouch.position;
                    currentPoses[Array.IndexOf(currentTouches, currentTouch)] = currentTouch.position;
                    break;

                    case TouchPhase.Moved:
                    currentPoses[Array.IndexOf(currentTouches, currentTouch)] = currentTouch.position;
                    swipeSpeeds[Array.IndexOf(currentTouches, currentTouch)] = currentTouch.deltaPosition.magnitude/currentTouch.deltaTime;
                    break;

                    case TouchPhase.Stationary:
                    swipeSpeeds[Array.IndexOf(currentTouches, currentTouch)] = 0;
                    break;

                    case TouchPhase.Ended:
                    startPoses[Array.IndexOf(currentTouches, currentTouch)] = Vector2.zero;
                    currentPoses[Array.IndexOf(currentTouches, currentTouch)] = Vector2.zero;
                    swipeSpeeds[Array.IndexOf(currentTouches, currentTouch)] = 0;
                    break;
                }
            }
        }
        else
        {
            isTouching = false;
            for(int i = 0; i < detectableNum; i ++)
            {
                startPoses[i] = Vector2.zero;
                currentPoses[i] = Vector2.zero;
                swipeSpeeds[i] = 0;
            }
        }
        SwipeCheker();
    }
    void SwipeCheker()
    {
        float swipeThresh = 1200;
        if(swipeSpeeds.Any(s => s > swipeThresh
            && uGUIMannager.StoC(startPoses[Array.IndexOf(swipeSpeeds, s)]).x
                < uGUIMannager.Blast.rectTransform.anchoredPosition.x + uGUIMannager.Blast.rectTransform.rect.width/2
            && (uGUIMannager.StoC(startPoses[Array.IndexOf(swipeSpeeds, s)]).x
                > uGUIMannager.leftStickBack.rectTransform.anchoredPosition.x + uGUIMannager.leftStickBack.rectTransform.rect.width/2
            || uGUIMannager.StoC(startPoses[Array.IndexOf(swipeSpeeds, s)]).y
                > uGUIMannager.leftStickBack.rectTransform.anchoredPosition.y + uGUIMannager.leftStickBack.rectTransform.rect.height/2)))
        {
            Touch[] swipes = currentTouches.Where(t => t.deltaPosition.magnitude/t.deltaTime > swipeThresh).ToArray();
            foreach(Touch swipe in swipes)
            {
                Vector2 diffPos = currentPoses[swipe.fingerId] - startPoses[swipe.fingerId];
                if(Vector2.SignedAngle(Vector2.up, diffPos) >= -45 && Vector2.SignedAngle(Vector2.up, diffPos) < 45)
                {
                    swipeUp = true;
                }
                else if(Vector2.SignedAngle(Vector2.up, diffPos) >= 45 && Vector2.SignedAngle(Vector2.up, diffPos) < 135)
                {
                    swipeLeft = true;
                }
                else if(Vector2.SignedAngle(Vector2.up, diffPos) >= -135 && Vector2.SignedAngle(Vector2.up, diffPos) < -45)
                {
                    swipeRight = true;
                }
                else
                {
                    swipeDown = true;
                }
            }
        }
        else
        {
            swipeUp = false;
            swipeDown = false;
            swipeRight = false;
            swipeLeft = false;
        }
    }
}
