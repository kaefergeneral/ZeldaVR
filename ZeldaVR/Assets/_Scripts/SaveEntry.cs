﻿using UnityEngine;
using System.Collections;
using System.IO;


public class SaveEntry : MonoBehaviour
{
    public Transform[] linkImages, swordImages;
    public Transform selectedIndicatorImage;
    public SpriteRenderer nameSprite, deathCountSprite;

    public Transform heartsContainer;
    public GameObject heartPrefab;


    public int ID { get; set; }
    public string PlayerName { get; set; }
    public int PlayerDeathCount { get; set; }


    void Awake()
    {
        MarkAsSelected(false);
        //ClearGuiToDefault();
    }


    public void InitWithEntryData(ZeldaSerializer.EntryData data)
    {
        if (data != null)
        {
            UpdateGuiWithEntryData(data);
        }
        else
        {
            ClearGuiToDefault();
        }
    }


    void ClearGuiToDefault()
    {
        PlayerName = "";
        PlayerDeathCount = 0;

        SetText(nameSprite, "");
        SetText(deathCountSprite, "");
        SetArmorLevel(0);
        SetSwordLevel(0);
        SetHearts(0, 0);
    }

    void UpdateGuiWithEntryData(ZeldaSerializer.EntryData data)
    {
        //print(" UpdateGuiWithEntryData: " + data.ToString());

        PlayerName = data.name;
        PlayerDeathCount = data.deathCount;

        SetText(nameSprite, data.name);
        SetText(deathCountSprite, data.deathCount.ToString());
        SetArmorLevel(data.armorLevel);
        SetSwordLevel(data.swordLevel);
        SetHearts(data.numHalfHearts, data.numHeartContainers);
    }

    void SetText(SpriteRenderer spriteRenderer, string text)
    {
        Texture2D tex = ZeldaFont.Instance.TextureForString(text);
        if (tex == null) { return; }

        Rect r = new Rect(0, 0, tex.width, tex.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        spriteRenderer.sprite = Sprite.Create(tex, r, pivot);
    }

    void SetArmorLevel(int level)
    {
        foreach (var image in linkImages)
        {
            image.renderer.enabled = false;
        }
        linkImages[level].renderer.enabled = true;
    }

    void SetSwordLevel(int level)
    {
        foreach (var image in swordImages)
        {
            image.renderer.enabled = false;
        }
        swordImages[level].renderer.enabled = true;
    }

    void SetHearts(int numHalfHearts, int numHeartContainers)
    {
        foreach (Transform child in heartsContainer)
        {
            Destroy(child.gameObject);
        }

        int fullHearts = (int)(numHalfHearts * 0.5f);
        bool plusHalfHeart = numHalfHearts % 2 == 1;

        const int MaxHeartsPerRow = 8;

        float w = 0.5f;
        float h = 0.5f;           
        float x = 0;
        float y = 0;     // (we begin on bottom row)

        for (int i = 0; i < numHeartContainers; i++)
        {
            GameObject g = Instantiate(heartPrefab) as GameObject;
            Heart heart = g.GetComponent<Heart>();

            heart.transform.parent = heartsContainer;

            if (i < fullHearts)
            {
                heart.SetToFull();
            }
            else if (i == fullHearts && plusHalfHeart)
            {
                heart.SetToHalfFull();
            }
            else
            {
                heart.SetToEmpty();
            }

            heart.transform.localPosition = new Vector3(x, y, 0.05f);

            x += w;
            if (i == MaxHeartsPerRow - 1)
            {
                x = 0;
                y = h;    // (move up to first row)
            }
        }
    }


    public void MarkAsSelected(bool doMark = true)
    {
        selectedIndicatorImage.renderer.enabled = doMark;
    }

}