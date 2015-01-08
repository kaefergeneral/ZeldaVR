﻿using UnityEngine;
using System.Collections;
using Immersio.Utility;


public class Player : Singleton<Player> 
{
    const float ShieldBlockDotThreshold = 0.6f;   // [0-1].  Closer to 1 means player has to be facing an incoming projectile more directly in order to block it.
    const float MoonModeGravityModifier = 1 / 6.0f;
    const float DefaultJinxDuration = 2.0f;
    public const float DefaultLikeLikeTrapDuration = 3.5f;
    const int LikeLikeEscapeCount = 8;
    const float WhistleMelodyDuration = 4.0f;


    public OVRPlayerController playerController;
    public int jumpHeight = 0;

    
    Inventory _inventory;
    GameObject _sword;
    GameObject _secondaryItem;
    HealthController _healthController;

    public string Name { get; set; }
    public int DeathCount { get; set; }

    public bool IsAtFullHealth { get { return _healthController.IsAtFullHealth; } }
    public int HealthInHalfHearts { get { return PlayerHealthDelegate.HealthToHalfHearts(_healthController.Health); } }

    public int JumpHeight { 
        get { return jumpHeight; }
        set {
            jumpHeight = value;
            playerController.JumpForce = jumpHeight * 0.25f;
        } 
    }
    public Inventory Inventory { get { return _inventory; } }
    public GameObject SecondaryItem { get { return _secondaryItem; } }

    public Transform WeaponContainerLeft { get { return playerController.weaponContainerLeft; } }
    public Transform WeaponContainerRight { get { return playerController.weaponContainerRight; } }

    public bool IsJinxed { get; set; }      // Can't use sword when jinxed
    public bool IsInLikeLikeTrap            // Paralyzed, and will lose MagicShield if player doesn't press buttons fast enough
    {          
        get { return _isInLikeLikeTrap; } 
        private set {
            if (value == false && _isInLikeLikeTrap) { _healthController.ActivateTempInvinc(); }
            _isInLikeLikeTrap = value; 
            _likeLikeTrapCounter = 0; 
            IsParalyzed = value;
        } 
    }
    public bool IsParalyzed { get { return _isParalyzed; } set { _isParalyzed = value; playerController.enabled = !_isParalyzed; } }
    public bool IsInvincible { get { return _healthController.isIndestructible; } set { _healthController.isIndestructible = value; } }
    public bool IsAirJumpingEnabled { get { return playerController.airJumpingEnabled; } set { playerController.airJumpingEnabled = value; } }

    bool _IsMoonModeEnabled;
    float _normalGravityModifier;
    public bool IsMoonModeEnabled { 
        get { return _IsMoonModeEnabled; } 
        set {
            if (!_IsMoonModeEnabled) { _normalGravityModifier = playerController.GravityModifier; }
            _IsMoonModeEnabled = value;

            float gravMod = _normalGravityModifier;
            if (_IsMoonModeEnabled) { gravMod *= MoonModeGravityModifier; }
            playerController.GravityModifier = gravMod;
        }
    }
    public bool IsDead { get { return !_healthController.IsAlive; } }

    public bool IsAttackingWithSword
    {
        get
        {
            if (_sword == null) { return false; }
            return _sword.GetComponent<Sword>().IsAttacking;
        }
    }

    public void MakeInvincible(float duration)
    {
        StartCoroutine("MakeInvincibleCoroutine", duration);
    }
    IEnumerator MakeInvincibleCoroutine(float duration)
    {
        IsInvincible = true;
        yield return new WaitForSeconds(duration);
        IsInvincible = Cheats.Instance.InvincibilityIsEnabled;
    }


    bool _isInLikeLikeTrap;                 
    int _likeLikeTrapCounter;
    bool _isParalyzed;


    void Awake()
    {
        _inventory = Inventory.Instance;
        _healthController = GetComponent<HealthController>();

        Name = "";
    }


    public void EquipSword(string swordName)
    {
        if (_sword != null) { DeequipSword(); }

        Item sworditem = _inventory.GetItem(swordName);
        GameObject swordPrefab = sworditem.weaponPrefab;

        _sword = Instantiate(swordPrefab) as GameObject;
        _sword.name = swordName;
        _sword.transform.parent = WeaponContainerRight;
        _sword.transform.localPosition = Vector3.zero;
        _sword.transform.localRotation = Quaternion.Euler(90, 0, 0);

        EnableSwordProjectiles(_healthController.IsAtFullHealth);
    }

    public void DeequipSword()
    {
        Destroy(_sword);
        _sword = null;
    }

    public void EnableSwordProjectiles(bool doEnable = true)
    {
        if (_sword == null) { return; }
        Sword st = _sword.GetComponent<Sword>();
        st.projectileEnabled = doEnable;
    }


    public void EquipSecondaryItem(string itemName)
    {
        if (_secondaryItem != null) { DeequipSecondaryItem(); }

        Item item = _inventory.GetItem(itemName);
        if (item.weaponPrefab != null)
        {
            EquipSecondaryWeapon(itemName);
        }
        else
        {
            _secondaryItem = item.gameObject;
        }
    }

    void EquipSecondaryWeapon(string weaponName)
    {
        Item weaponItem = _inventory.GetItem(weaponName);

        _secondaryItem = Instantiate(weaponItem.weaponPrefab) as GameObject;
        _secondaryItem.name = weaponName;
        _secondaryItem.transform.parent = WeaponContainerLeft;
        _secondaryItem.transform.localPosition = Vector3.zero;
        _secondaryItem.transform.localRotation = Quaternion.identity;
    }

    public void DeequipSecondaryItem()
    {
        if (_secondaryItem != null && _secondaryItem.GetComponent<Item>() == null)
        {
            Destroy(_secondaryItem);
        }
        _secondaryItem = null;
    }


    void Update()
    {
        if (Pause.Instance.IsTimeFrozen) { return; }
        if (IsDead) { return; }

        if (IsInLikeLikeTrap)
        {
            if (ZeldaInput.GetButtonDown(ZeldaInput.Button.SwordAttack))
            {
                if (++_likeLikeTrapCounter >= LikeLikeEscapeCount)
                {
                    IsInLikeLikeTrap = false;
                }
            }
        }

        if (IsParalyzed) { return; }


        if (ZeldaInput.GetButtonDown(ZeldaInput.Button.SwordAttack))
        {
            if (_sword != null && !IsJinxed)
            {
                _sword.GetComponent<Sword>().Attack();
            }
        }

        WeaponContainerLeft.transform.forward = playerController.LineOfSight;

        if (_secondaryItem != null)
        {
            Bow bow = _secondaryItem.GetComponent<Bow>();
            if (bow != null)
            {
                if (ZeldaInput.GetButtonUp(ZeldaInput.Button.UseItemB))
                {
                    UseSecondaryItem();
                }
            }
            else
            {
                Vector3 frwd = WeaponContainerLeft.transform.forward;
                frwd.y = 0;
                WeaponContainerLeft.transform.forward = frwd;

                if (ZeldaInput.GetButtonDown(ZeldaInput.Button.UseItemB))
                {
                    UseSecondaryItem();
                }
            }
        }
    }

    void UseSecondaryItem()
    {
        BombDropper w = _secondaryItem.GetComponent<BombDropper>();
        if (w != null)
        {
            if (w.CanUse)
            { 
                w.DropBomb();
                _inventory.UseItemB(); 
            }
            return;
        }

        Candle c = _secondaryItem.GetComponent<Candle>();
        if (c != null)
        {
            if (c.CanUse)
            {
                c.DropFlame();
            }
            return;
        }

        Boomerang b = _secondaryItem.GetComponent<Boomerang>();
        if (b != null)
        {
            if (b.CanUse) 
            {
                b.Throw(WeaponContainerLeft, playerController.ForwardDirection);
            }
            return;
        }

        Bow bow = _secondaryItem.GetComponent<Bow>();
        if (bow != null)
        {
            if (_inventory.GetItem("WoodenArrow").count > 0 || _inventory.GetItem("SilverArrow").count > 0)
            {
                if (bow.CanUse && _inventory.GetItem("Rupee").count > 0)
                {
                    bow.Fire();
                    _inventory.UseItem("Rupee");
                }
            }
            return;
        }

        MagicWand wand = _secondaryItem.GetComponent<MagicWand>();
        if (wand != null)
        {
            if (wand.CanUse)
            {
                wand.spawnFlame = Inventory.Instance.GetItem("MagicBook").count > 0;
                wand.Fire();
            }
            return;
        }

        BaitDropper bd = _secondaryItem.GetComponent<BaitDropper>();
        if (bd != null)
        {
            if (bd.CanUse)
            {
                bd.DropBait();
            }
            return;
        }

        if (_secondaryItem.name == "Whistle")
        {
            StartCoroutine("UseWhistle");
            return;
        }

        _inventory.UseItemB();
    }

    int _nextWarpDungeonNum = 1;
    IEnumerator UseWhistle()
    {
        if (IsJinxed)
        {
            DeactivateJinx();
        }

        ParalyzeAllNearbyEnemies(WhistleMelodyDuration);
        ActivateParalyze(WhistleMelodyDuration);

        Music.Instance.Stop();
        SoundFx.Instance.PlayOneShot(SoundFx.Instance.whistle);

        yield return new WaitForSeconds(WhistleMelodyDuration);

        Music.Instance.PlayAppropriateMusic();

        if (WorldInfo.Instance.IsInDungeon)
        {
            DungeonRoom dr = OccupiedDungeonRoom();
            if (dr != null)
            {
                EnemyAI_Digdogger digdogger = null;
                foreach (var enemy in dr.Enemies)
                {
                    digdogger = enemy.GetComponent<EnemyAI_Digdogger>();
                    if (digdogger != null && !digdogger.HasSplit)
                    {
                        digdogger.SplitIntoBabies();
                        break;
                    }
                }
            }
        }
        else if (WorldInfo.Instance.IsOverworld)
        {
            GameObject g = GameObject.FindGameObjectWithTag("Dungeon7Entrance");
            float dist = Vector3.Distance(g.transform.position, playerController.transform.position);
            if (dist < 7)
            {
                g.GetComponent<Dungeon7Entrance>().EmptyLake();
            }
            else
            {
                Locations.Instance.WarpToOverworldDungeonEntrance(_nextWarpDungeonNum);

                // Determine next dungeon to warp to (if Whistle is blown again);
                Inventory inv = Inventory.Instance;
                bool canWarpToDungeon = false;
                do
                {
                    if (++_nextWarpDungeonNum > 8)
                    {
                        _nextWarpDungeonNum = 1;
                    }

                    canWarpToDungeon = inv.HasTriforcePieceForDungeon(_nextWarpDungeonNum);
                } while (!canWarpToDungeon);
                
            }
        }
    }


    public DungeonRoom OccupiedDungeonRoom()
    {
        if (!WorldInfo.Instance.IsInDungeon) { return null; }
        
        return DungeonRoom.GetRoomForPosition(playerController.transform.position);
    }

    public float GetDamageModifier()
    {
        float mod = 1;
        if (_inventory.GetItem("RedRing").count > 0)
        {
            mod = 0.25f;
        }
        else if (_inventory.GetItem("BlueRing").count > 0)
        {
            mod = 0.5f;
        }
        return mod;
    }

    public bool CanBlockAttack(bool isBlockableByWoodenShield, bool isBlockableByMagicShield, Vector3 attacksForwardDirection)
    {
        if (_sword != null && _sword.GetComponent<Sword>().IsAttacking)
        {
            return false;
        }

        bool canBlock = false;

        bool attackIsDirectlyInFrontOfPlayer = Vector3.Dot(playerController.ForwardDirection, -attacksForwardDirection) > ShieldBlockDotThreshold;
        if (attackIsDirectlyInFrontOfPlayer)
        {
            if (isBlockableByWoodenShield && _inventory.GetItem("WoodenShield").count > 0)
            {
                canBlock = true;
            }
            else if (isBlockableByMagicShield && _inventory.GetItem("MagicShield").count > 0)
            {
                canBlock = true;
            }
        }

        return canBlock;
    }


    public void ActivateParalyze(float duration)
    {
        StartCoroutine("ParalyzeCoroutine", duration);
    }
    IEnumerator ParalyzeCoroutine(float duration)
    {
        IsParalyzed = true;
        yield return new WaitForSeconds(duration);
        IsParalyzed = false;
    }

    public void ActivateJinx(float duration = DefaultJinxDuration)
    {
        StartCoroutine("JinxCoroutine", duration);
    }
    public void DeactivateJinx()
    {
        if (!IsJinxed) { return; }
        
        StopCoroutine("JinxCoroutine");
        IsJinxed = false;
    }
    IEnumerator JinxCoroutine(float duration)
    {
        IsJinxed = true;
        yield return new WaitForSeconds(duration);
        IsJinxed = false;
    }

    EnemyAI_Random _likeLikeTrappingPlayer;
    public void ActivateLikeLikeTrap(GameObject likeLike, float duration = DefaultLikeLikeTrapDuration)
    {
        if (IsInLikeLikeTrap) { return; }
        _likeLikeTrappingPlayer = likeLike.GetComponent<EnemyAI_Random>();
        StartCoroutine("LikeLikeTrapCoroutine", duration);
    }
    IEnumerator LikeLikeTrapCoroutine(float duration)
    {
        IsInLikeLikeTrap = true;
        _likeLikeTrappingPlayer.enabled = false;
        yield return new WaitForSeconds(duration);

        if (_likeLikeTrappingPlayer != null)
        {
            _likeLikeTrappingPlayer.enabled = true;
            // If player didn't escape from trap in time, he loses MagicShield
            if (IsInLikeLikeTrap) { _inventory.GetItem("MagicShield").count = 0; }
        }

        IsInLikeLikeTrap = false;
    }


    public void ParalyzeAllNearbyEnemies(float duration)
    {
        if (WorldInfo.Instance.IsOverworld)
        {
            GameObject enemiesContainer = GameObject.FindGameObjectWithTag("Enemies");
            foreach (Transform child in enemiesContainer.transform)
            {
                Enemy enemy = child.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.Paralyze(duration);
                }
            }
        }
        else if (WorldInfo.Instance.IsInDungeon)
        {
            DungeonRoom roomPlayerIsIn = OccupiedDungeonRoom();
            foreach (Enemy enemy in roomPlayerIsIn.Enemies)
            {
                enemy.Paralyze(duration);

            }
        }
    }


    public void ForceNewForwardDirection(Vector3 direction)
    {
        //playerController.transform.rotation = Quaternion.Euler(lerpedEuler);
        playerController.transform.forward = direction;
        playerController.InitializeInputs();
    }

    public void ForceNewEulerAngles(Vector3 euler)
    {
        playerController.transform.eulerAngles = euler;
        playerController.InitializeInputs();
    }


    public void RestoreHalfHearts(int halfHearts)
    {
        if (halfHearts <= 0) { return; }

        int healAmount = PlayerHealthDelegate.HalfHeartsToHealth(halfHearts);
        _healthController.RestoreHealth((uint)healAmount);
    }

}