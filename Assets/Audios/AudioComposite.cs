using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class AudioComposite : MonoBehaviour
{
    public AudioSource basic;
    public AudioSource violin;
    public AudioSource bell;
    public AudioSource flute;
    public AudioSource basicAlter;
    public AudioSource bellAlter;
    public AudioSource fluteAlter;
    public AudioSource end;
    public float switchSpeed = 1.0f;
    public CameraFocus focusScript;
    public HexTilemap tilemap;

    void Start()
    {
        basic.volume = violin.volume = bell.volume = flute.volume = 0;
        basicAlter.volume = end.volume = bellAlter.volume = fluteAlter.volume = 0;
    }

    void AdjustVolume(AudioSource source, float target)
    {
        source.volume = source.volume + Mathf.Sign(target - source.volume) * Mathf.Min(Mathf.Abs(target - source.volume), switchSpeed * Time.deltaTime);
    }

    void AdjustVolume(AudioSource source, bool on)
    {
        AdjustVolume(source, on ? 1 : 0.00f);
    }

    void Update()
    {
        bool cutoff = false;
        bool useAlter = tilemap.NeedDisplayFertility;
        bool useViolin = (math.unlerp(focusScript.minDiatance, focusScript.maxDistance, focusScript.distance) >= 1.0f);
        bool useBell = (math.unlerp(focusScript.minDiatance, focusScript.maxDistance, focusScript.distance) <= 0.2f);
        Debug.Log(math.unlerp(focusScript.minDiatance, focusScript.maxDistance, focusScript.distance));
        bool useBasic = (math.unlerp(focusScript.minDiatance, focusScript.maxDistance, focusScript.distance) >= 0.1f);
        bool useFlute = (math.unlerp(focusScript.minDiatance, focusScript.maxDistance, focusScript.distance) >= 0.7f);
        bool useEnd = false;
        useEnd = EventChecker.Instance.downedEventC3;
        AdjustVolume(end, !cutoff && useEnd);
        if (useEnd) cutoff = true;

        AdjustVolume(basic, !cutoff && !useAlter && useBasic);
        AdjustVolume(violin, !cutoff && !useAlter && useViolin);
        AdjustVolume(bell, !cutoff && !useAlter && useBell);
        AdjustVolume(flute, !cutoff && !useAlter && useFlute);
        AdjustVolume(basicAlter, !cutoff && useAlter && useBasic);
        AdjustVolume(bellAlter, !cutoff && useAlter && useBell);
        AdjustVolume(fluteAlter, !cutoff && useAlter && useFlute);

        flute.time = bell.time = violin.time = basic.time;
        fluteAlter.time = bellAlter.time = basicAlter.time = end.time = basic.time;
    }
}
