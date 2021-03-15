using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-2)]
public class AudioandBurner : MonoBehaviour
{
   AudioSource jetAudio;
   GameObject afterBurner1;
   GameObject afterBurner2;
   GameObject thruster1;
   GameObject thruster2;
   ParticleSystem.MainModule AParticle1;
   ParticleSystem.MainModule AParticle2;
   ParticleSystem.MainModule TParticle1;
   ParticleSystem.MainModule TParticle2;
   void Start()
   {
       jetAudio = GetComponent<AudioSource>();
       afterBurner1 = transform.Find("AfterBurner1").gameObject;;
       afterBurner2 = transform.Find("AfterBurner2").gameObject;
       thruster1 = afterBurner1.transform.Find("Thrust").gameObject;
       thruster2 = afterBurner2.transform.Find("Thrust").gameObject;
       AParticle1 = afterBurner1.GetComponent<ParticleSystem>().main;
       AParticle2 = afterBurner2.GetComponent<ParticleSystem>().main;
       TParticle1 = thruster1.GetComponent<ParticleSystem>().main;
       TParticle2 = thruster2.GetComponent<ParticleSystem>().main;
   }
   void Update()
   {
       AudioChange();
   }
//    public void AudioandBurnerSetter(AudioSource JetEngine, GameObject AfterBurner1, GameObject AfterBurner2, GameObject Thruster1, GameObject Thruster2)
//    {
//        jetAudio = JetEngine;
//        afterBurner1 = AfterBurner1;
//        afterBurner2 = AfterBurner2;
//        thruster1 = Thruster1;
//        thruster2 = Thruster2;
//        AParticle1 = afterBurner1.GetComponent<ParticleSystem>().main;
//        AParticle2 = afterBurner2.GetComponent<ParticleSystem>().main;
//        TParticle1 = thruster1.GetComponent<ParticleSystem>().main;
//        TParticle2 = thruster2.GetComponent<ParticleSystem>().main;
//    }
   void AudioChange()
   {
       float maxPitch = 1f;
       float minPitch = 0.05f;
       jetAudio.pitch = Mathf.Lerp(jetAudio.pitch, (maxPitch - minPitch) * uGUIMannager.norSSliderValue + minPitch, 0.1f);
   }
    public void BurnerChange()
    {
        float maxBurnerSpeed = 0.4f;
        float minBurnerSpeed = 0.1f;
        float maxBurnerSize = 0.04f;
        float minBurnerSize = 0.01f;
        float maxThrusterSpeed = 0.8f;
        float minThrusterSpeed = 0.2f;
        float maxThrusterSize = 0.08f;
        float minThrusterSize = 0.02f;
        AParticle1.startSpeed = (maxBurnerSpeed - minBurnerSpeed)*uGUIMannager.norSSliderValue + minBurnerSpeed;
        AParticle1.startSize = (maxBurnerSize - minBurnerSize)*uGUIMannager.norSSliderValue + minBurnerSize;
        AParticle2.startSpeed = (maxBurnerSpeed - minBurnerSpeed)*uGUIMannager.norSSliderValue + minBurnerSpeed;
        AParticle2.startSize = (maxBurnerSize - minBurnerSize)*uGUIMannager.norSSliderValue + minBurnerSize;
        TParticle1.startSpeed = (maxThrusterSpeed - minThrusterSpeed)*uGUIMannager.norSSliderValue + minThrusterSpeed;
        TParticle1.startSize = (maxThrusterSize - minThrusterSize)*uGUIMannager.norSSliderValue + minThrusterSize;
        TParticle2.startSpeed = (maxThrusterSpeed - minThrusterSpeed)*uGUIMannager.norSSliderValue + minThrusterSpeed;
        TParticle2.startSize = (maxThrusterSize - minThrusterSize)*uGUIMannager.norSSliderValue + minThrusterSize;
    }
}
