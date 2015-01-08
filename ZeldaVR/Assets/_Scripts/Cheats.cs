﻿using UnityEngine;
using Immersio.Utility;


public class Cheats : Singleton<Cheats>
{
    public Collectible forceDroppedItem;        // Forces a specific Collectible to always drop

    public bool cheatingAllowed = false;

    public bool GhostModeIsEnabled { get; private set; }
    public bool SecretDetectionModeIsEnabled { get; private set; }
    public bool InvincibilityIsEnabled { get; private set; }


    float _maxRunMultiplier = 4;
    int _maxJumpHeight = 8;


    void Update()
    {
        if (cheatingAllowed)
        {
            CheckKeyboardInputs();
            CheckControllerInputs();

            //CycleTriforcePieces();
        }
    }

    void CheckKeyboardInputs()
    {
        if (Input.GetKeyDown(KeyCode.F1)) { ToggleGodMode(); }

        if (Input.GetKeyDown(KeyCode.F2)) { ToggleInvincibility(); }
        if (Input.GetKeyDown(KeyCode.F3)) { MaxOutInventory(); }
        if (Input.GetKeyDown(KeyCode.F4)) { ToggleGhostMode(); }

        if (Input.GetKeyDown(KeyCode.F5)) { IncreaseRunMultiplier(); }
        if (Input.GetKeyDown(KeyCode.F6)) { IncreaseJumpHeight(); }

        if (Input.GetKeyDown(KeyCode.F7)) { ToggleAirJumping(); }
        if (Input.GetKeyDown(KeyCode.F8)) { ToggleMoonMode(); }

        if (Input.GetKeyDown(KeyCode.F9)) { RestorePlayerHealth(); }
        if (Input.GetKeyDown(KeyCode.F10)) { DamagePlayer(); }
        if (Input.GetKeyDown(KeyCode.F11)) { KillPlayer(); }

        //if (Input.GetKeyDown(KeyCode.F12)) { ToggleSecretDetectionMode(); }
        if (Input.GetKeyDown(KeyCode.F12)) { MaxOutRupees(); }

        if (Input.GetKeyDown(KeyCode.Alpha1)) { EquipSword("WoodenSword"); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { EquipSword("WhiteSword"); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { EquipSword("MagicSword"); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { EquipSword(null); }
    }

    void CheckControllerInputs()
    {
        if (ZeldaInput.GetButtonUp(ZeldaInput.Button.L1)) { ToggleGodMode(); }
        //if (ZeldaInput.GetButtonUp(ZeldaInput.Button.L2)) { ToggleGhostMode(); }
        if (ZeldaInput.GetButtonUp(ZeldaInput.Button.R1)) { IncreaseJumpHeight(); }
        //if (ZeldaInput.GetButtonUp(ZeldaInput.Button.R2)) { IncreaseRunMultiplier(); }
        if (ZeldaInput.GetButtonUp(ZeldaInput.Button.Extra)) { ToggleGhostMode(); }
    }


    bool _godModeEnabled;
    void ToggleGodMode()
    {
        ToggleGodMode(!_godModeEnabled);
    }
    void ToggleGodMode(bool enable)
    {
        _godModeEnabled = enable;

        ToggleInvincibility(_godModeEnabled);
        ToggleAirJumping(_godModeEnabled);
        ToggleMoonMode(_godModeEnabled);
        //ToggleGhostMode(_godModeEnabled);
        SetRunMultiplier(_godModeEnabled ? _maxRunMultiplier : 1);
        SetJumpHeight(_godModeEnabled ? 1 : 0);

        if (_godModeEnabled)
        {
            MaxOutInventory();
            RestorePlayerHealth();
        }
    }


    void MaxOutInventory()
    {
        Inventory.Instance.MaxOutEverything();
    }

    void ToggleInvincibility()
    {
        ToggleInvincibility(!InvincibilityIsEnabled);
    }
    void ToggleInvincibility(bool enable)
    {
        InvincibilityIsEnabled = enable;
        CommonObjects.Player_C.IsInvincible = enable;
    }

    void ToggleGhostMode()
    {
        ToggleGhostMode(!GhostModeIsEnabled);
    }
    void ToggleGhostMode(bool enable)
    {
        GhostModeIsEnabled = enable;

        int playerLayer = CommonObjects.Player_G.layer;
        int wallLayer = LayerMask.NameToLayer("Walls");
        int blocksLayer = LayerMask.NameToLayer("Blocks");
        int invisibleBlocksLayer = LayerMask.NameToLayer("InvisibleBlocks");

        Physics.IgnoreLayerCollision(playerLayer, wallLayer, GhostModeIsEnabled);
        Physics.IgnoreLayerCollision(playerLayer, blocksLayer, GhostModeIsEnabled);
        Physics.IgnoreLayerCollision(playerLayer, invisibleBlocksLayer, GhostModeIsEnabled);
    }

    void ToggleAirJumping()
    {
        ToggleAirJumping(!CommonObjects.Player_C.IsAirJumpingEnabled);
    }
    void ToggleAirJumping(bool enable)
    {
        CommonObjects.Player_C.IsAirJumpingEnabled = enable;
    }

    void ToggleMoonMode()
    {
        ToggleMoonMode(!CommonObjects.Player_C.IsMoonModeEnabled);
    }
    void ToggleMoonMode(bool enable)
    {
        CommonObjects.Player_C.IsMoonModeEnabled = enable;
    }

    void ToggleSecretDetectionMode()
    {
        ToggleSecretDetectionMode(!SecretDetectionModeIsEnabled);
    }
    void ToggleSecretDetectionMode(bool enable)
    {
        SecretDetectionModeIsEnabled = enable;

        WorldInfo world = WorldInfo.Instance;
        if (world.IsOverworld)
        {
            TileProliferator.Instance.tileMap.HighlightAllSpecialBlocks(enable);
        }
        else if (world.IsInDungeon)
        {
            // TODO
        }
    }


    void IncreaseRunMultiplier()
    {
        float newMultipler = CommonObjects.PlayerController_C.RunMultiplier * 2;
        if (newMultipler > _maxRunMultiplier) { newMultipler = 1; }

        SetRunMultiplier(newMultipler);
    }
    void SetRunMultiplier(float rm)
    {
        CommonObjects.PlayerController_C.RunMultiplier = rm;
    }

    void IncreaseJumpHeight()
    {
        int newHeight = CommonObjects.Player_C.JumpHeight;
        newHeight = (newHeight == 0) ? 1 : newHeight * 2;

        if (newHeight > _maxJumpHeight) { newHeight = 0; }

        SetJumpHeight(newHeight);
    }
    void SetJumpHeight(int h)
    {
        CommonObjects.Player_C.JumpHeight = h;
    }

    void MaxOutRupees()
    {
        SetRupeeCount(Inventory.Instance.GetItem("Rupee").maxCount);
    }
    void SetRupeeCount(int r)
    {
        Inventory.Instance.ReceiveRupees(r);
    }


    void RestorePlayerHealth()
    {
        CommonObjects.Player_C.GetComponent<HealthController>().RestoreHealth();
    }

    void DamagePlayer()
    {
        CommonObjects.Player_C.GetComponent<HealthController>().TakeDamage(64, gameObject);
    }

    void KillPlayer()
    {
        CommonObjects.Player_C.GetComponent<HealthController>().Kill(gameObject, true);
    }


    void EquipSword(string swordName)
    {
        CommonObjects.Player_C.Inventory.EquipSword_Cheat(swordName);
    }


    int _dungeonNum = 1;
    int _count = 0;
    void CycleTriforcePieces()
    {
        if (++_count > 30)
        {
            _count = 0;
            if (_dungeonNum > 8)
            {
                _dungeonNum = 0;
                for (int i = 1; i <= 8; i++)
                {
                    Inventory.Instance.SetHasTriforcePieceForDungeon(i, false);
                }
            }
            else
            {
                Inventory.Instance.SetHasTriforcePieceForDungeon(_dungeonNum, true);
            }
            _dungeonNum++;
        }
    }

}