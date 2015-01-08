﻿using UnityEngine;
using System.Collections;

public class EnemyAI_Armos : MonoBehaviour 
{
    const float ProximityThreshold = 1.5f;
    const float ProximityThresholdSq = ProximityThreshold * ProximityThreshold;
    const float SpeedSlow = 1.5f;
    const float SpeedFast = 3.5f;


    public enum StatueType
    {
        Red,
        Green,
        White
    }
    StatueType _type;
    public StatueType Type {
        get { return _type; }
        set {
            _type = value;
            switch (_type)
            {
                case StatueType.Red: _statue = redStatue; break;
                case StatueType.Green: _statue = greenStatue; break;
                case StatueType.White: _statue = whiteStatue; break;
            }
        }
    }


    public EnemyAI_Random enemyAI_Random;
    public Animator animator;
    public GameObject redStatue, greenStatue, whiteStatue;
    public GameObject[] linkedTiles;



    bool _isInStatueMode = true;
    OVRPlayerController _ovrPlayerController;
    GameObject _statue;
    int _meleeDamage;


    public bool HidesCollectibleItem { get; set; }


    void Awake()
    {
        _ovrPlayerController = GameObject.Find("OVRPlayerController").GetComponent<OVRPlayerController>();
        enemyAI_Random.enabled = false;
        animator.gameObject.SetActive(false);
        rigidbody.constraints = RigidbodyConstraints.FreezeAll;

        Enemy enemy = GetComponent<Enemy>();
        _meleeDamage = enemy.meleeDamage;
        enemy.meleeDamage = 0;

        redStatue.SetActive(false);
        greenStatue.SetActive(false);
        whiteStatue.SetActive(false);
    }

    void Start()
    {
        _statue.SetActive(true);
    }


    void OnTriggerEnter(Collider otherCollider)
    {
        if (_isInStatueMode)
        {
            GameObject other = otherCollider.gameObject;
            if (other == _ovrPlayerController.gameObject)
            {
                StartCoroutine("ComeAlive");
            }
        }
    }


    IEnumerator ComeAlive()
    {
        _isInStatueMode = false;
        GetComponent<HealthController>().isIndestructible = false;
        
        _statue.SetActive(false);
        animator.gameObject.SetActive(true);
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        bool linkedTilesWereRemoved = false;
        if (linkedTiles.Length > 0)
        {
            DestroyLinkedTiles();
            linkedTilesWereRemoved = true;
        }

        yield return new WaitForSeconds(1.0f);

        if (linkedTilesWereRemoved)
        {
            SoundFx.Instance.PlayOneShot(SoundFx.Instance.secret);
        }

        if (HidesCollectibleItem)
        {
            ActivateHiddenCollectible();
        }

        GetComponent<Enemy>().speed = Extensions.FlipCoin() ? SpeedFast : SpeedSlow;

        GetComponent<Enemy>().meleeDamage = _meleeDamage;
        enemyAI_Random.enabled = true;
    }

    void ActivateHiddenCollectible()
    {
        GameObject hiddenCollectible = null;

        Transform collectiblesContainer = GameObject.Find("Special Collectibles").transform;
        foreach (Transform child in collectiblesContainer)
        {
            Vector3 toCollectible = child.position - transform.position;
            float distanceToCollectibleSqr = Vector3.SqrMagnitude(toCollectible);
            if (distanceToCollectibleSqr < 1)
            {
                hiddenCollectible = child.gameObject;
            }
        }

        if (hiddenCollectible != null)
        {
            hiddenCollectible.SetActive(true);
            SoundFx.Instance.PlayOneShot(SoundFx.Instance.secret);
        }
    }

    void DestroyLinkedTiles()
    {
        for (int i = 0; i < linkedTiles.Length; i++)
        {
            Destroy(linkedTiles[i]);
        }
    }

}