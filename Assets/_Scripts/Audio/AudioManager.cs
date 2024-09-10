using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum AudioBus
{
    MASTER,
    ENVIRONMENT,
    SFX
}
public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioEvent music = null;
    private Dictionary<AudioEvent, List<EventInstance>> activeEvents = new Dictionary<AudioEvent, List<EventInstance>>();
    private Dictionary<AudioBus, Bus> audioBuses = new Dictionary<AudioBus, Bus>();
    private Dictionary<AudioBus, Coroutine> fadeCoroutines = new Dictionary<AudioBus, Coroutine>();

    protected override void Awake()
    {
        base.Awake();
        InitBusses();
        SceneManager.sceneLoaded += (scene, _) => OnSceneLoaded(scene);
    }
    protected void OnSceneLoaded(Scene scene)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        switch (scene.name)
        {
            // Add case statements for scene names here
            default:
                // TODO: have music
                // Play(music);
                break;
        }
    }
    private float timer = 0;
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > 1)
        {
            CheckActiveEvents();
            timer = 0;
        }
    }
    private void OnDestroy()
    {
        StopActiveSounds();
    }
    #region PlaySounds
    /// <summary>
    /// Plays an audio event based on the parameters given in the audioEvent as well as any additional details
    /// passed in through the parameters
    /// </summary>
    /// <param name="audioEvent">The audio event to play, must have a non null FMOD event</param>
    /// <param name="position">If the audio event is spatial, use this transform to set the attenuation parameters</param>
    /// <param name="gameObject">If the audio event will follow a given game object (like a missile) set the game object to be this parameter.</param>
    /// <param name="parameters">For any other parameters that could be set, these are extremely useful for things like footsteps, where you want to
    ///   pass in the ground type.
    /// </param>
    public void Play(AudioEvent audioEvent, Vector3? position = null, GameObject gameObject = null, params ValueTuple<string, float>[] parameters)
    {
        if (audioEvent == null || audioEvent.fmodEvent.IsNull)
        {
            Debug.LogWarning("AudioEvent is null");
            return;
        }
        EventInstance instance = RuntimeManager.CreateInstance(audioEvent.fmodEvent);
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                instance.setParameterByName(param.Item1, param.Item2);
            }
        }
        if (audioEvent.isSpatial)
        {
            if (position == null || position == Vector3.zero)
            {
                Debug.LogWarning("3D sound effect called without position");
                return;
            }
            instance.set3DAttributes(RuntimeUtils.To3DAttributes((Vector3)position));
        }
        if (audioEvent.attachToGameObject)
        {
            if (gameObject == null)
            {
                Debug.LogWarning("AudioEvent is set to attach to game object, but no game object was provided");
                return;
            }
            var rb = gameObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogWarning("Cannot attach oneshot to object without rigidbody");
                return;
            }
            RuntimeManager.AttachInstanceToGameObject(instance, gameObject.transform, rb);
        }
        if (audioEvent.isOneshot)
        {
            instance.start();
            instance.release();
        }
        else
        {
            activeEvents.AddIfList(audioEvent, instance);
            instance.start();
        }
    }

    public void StopActiveSounds()
    {
        foreach (var audioEvent in activeEvents.Keys)
        {
            foreach (var instance in activeEvents[audioEvent])
            {
                instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
        activeEvents.Clear();
    }
    public void Stop(AudioEvent audioEvent)
    {
        if (activeEvents.ContainsKey(audioEvent))
        {
            foreach (var instance in activeEvents[audioEvent])
            {
                if (instance.isValid())
                {
                    instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                }
            }
            activeEvents[audioEvent].Clear();
            activeEvents.Remove(audioEvent);
        }
    }
    public void CheckActiveEvents()
    {
        foreach (var audioEvent in activeEvents.Keys)
        {
            foreach (var instance in activeEvents[audioEvent])
            {
                if (!instance.isValid())
                {
                    activeEvents.RemoveIfList(audioEvent, instance);
                }
            }
        }
    }
    #endregion
    #region Bus Controls
    private void InitBusses()
    {
        audioBuses.Add(AudioBus.MASTER, RuntimeManager.GetBus("bus:/"));
        audioBuses.Add(AudioBus.ENVIRONMENT, RuntimeManager.GetBus("bus:/Environment"));
        audioBuses.Add(AudioBus.SFX, RuntimeManager.GetBus("bus:/SFX"));
    }
    public void AdjustBusVolume(AudioBus bus, float volume)
    {
        audioBuses[bus].setVolume(volume);
    }

    private IEnumerator FadeBus(AudioBus bus, float targetVolume, float fadeTime)
    {
        float startVolume = 0;
        audioBuses[bus].getVolume(out startVolume);
        float currentTime = 0;
        while (currentTime < fadeTime)
        {
            currentTime += Time.deltaTime;
            audioBuses[bus].setVolume(Mathf.Lerp(startVolume, targetVolume, currentTime / fadeTime));
            yield return null;
        }
    }

    /// <summary>
    /// Fades a bus to a target volume over a given time
    /// </summary>
    /// <param name="bus">which bus to fade</param>
    /// <param name="targetVolume">how loud it should be, this should be between 0 and 1</param>
    /// <param name="fadeTime">how long in seconds</param>
    public void Fade(AudioBus bus, float targetVolume, float fadeTime)
    {
        if (fadeCoroutines.ContainsKey(bus))
        {
            StopCoroutine(fadeCoroutines[bus]);
        }
        fadeCoroutines[bus] = StartCoroutine(FadeBus(bus, targetVolume, fadeTime));
    }
    public void StopFade(AudioBus bus)
    {
        if (fadeCoroutines.ContainsKey(bus))
        {
            StopCoroutine(fadeCoroutines[bus]);
        }
    }
    public void StopAllFades()
    {
        foreach (KeyValuePair<AudioBus, Coroutine> fade in fadeCoroutines)
        {
            StopCoroutine(fade.Value);
        }
    }
    public void StopAllSounds(AudioBus? bus = null)
    {
        foreach (KeyValuePair<AudioBus, Bus> audioBus in audioBuses)
        {
            if (bus == null || audioBus.Key == bus)
            {
                audioBus.Value.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }
    #endregion

}

