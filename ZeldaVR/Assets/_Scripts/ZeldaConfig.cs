using UnityEngine;
using Immersio.Utility;

public class ZeldaConfig : Singleton<ZeldaConfig> 
{

    public float version;
    public bool isDemo;
    public bool isGflBuild;
    public bool defaultMusicEnabled;


    void Awake()
    {
        Music.Instance.IsEnabled = defaultMusicEnabled;
    }

}