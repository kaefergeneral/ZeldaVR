using UnityEngine;


public class HoleMarker : MonoBehaviour 
{
    public bool appearsOnPushBlock;

    void Awake()
    {
        renderer.enabled = false;
    }
}