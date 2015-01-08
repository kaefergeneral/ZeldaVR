﻿using UnityEngine;


public class EnemyAI_Statue : EnemyAI 
{
    public float baseAttackCooldown = 2.0f;
    public float randomAttackCooldownOffset = 0.5f;


    float _lastAttackTime = float.NegativeInfinity;
    float _attackCooldown;


    void Awake()
    {
        base.Awake();

        if (WorldInfo.Instance.IsInDungeon)
        {
            int dungeonNum = WorldInfo.Instance.DungeonNum;
            renderer.material = CommonObjects.Instance.GetEnemyStatueMaterialForDungeon(dungeonNum);
        }

        InstantiateInvisibleBlock();
    }

    void InstantiateInvisibleBlock()
    {
        GameObject block = Instantiate(CommonObjects.Instance.invisibleBlockStatuePrefab) as GameObject;
        block.transform.parent = DungeonFactory.Instance.blocksContainer;

        Vector3 pos = transform.position;
        pos.y = transform.localScale.y * 0.5f;
        block.transform.position = pos;
    }

    void Start()
    {
        ResetCooldownTimer();
    }


    void Update()
    {
        if (!_doUpdate) { return; }
        if (_enemy.IsParalyzed) { return; }

        bool timesUp = (Time.time - _lastAttackTime >= _attackCooldown);
        if (timesUp)
        {
            Attack();

            renderer.enabled = true;        // TODO: not sure where it is being set to false
        }
    }

    void Attack()
    {
        //Vector3 toPlayer = _enemy.PlayerController.transform.position - transform.position;
        //toPlayer.Normalize();
        //_enemy.Attack(toPlayer);

        Vector3 direction = Random.insideUnitSphere;
        direction.y = 0;
        direction.Normalize();
        _enemy.Attack(direction);

        ResetCooldownTimer();
    }

    void ResetCooldownTimer()
    {
        _lastAttackTime = Time.time;
        _attackCooldown = baseAttackCooldown + Random.Range(-randomAttackCooldownOffset, randomAttackCooldownOffset);
    }

}