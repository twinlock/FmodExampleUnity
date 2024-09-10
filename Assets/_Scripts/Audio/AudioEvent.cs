using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "New Audio Event", menuName = "Scriptable Objects/Audio Event")]
public class AudioEvent : ScriptableObject
{
    [SerializeField] public EventReference fmodEvent;
    public bool isOneshot = false;
    public bool isSpatial = false;
    public bool attachToGameObject = false;
}