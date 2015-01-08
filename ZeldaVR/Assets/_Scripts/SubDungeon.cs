﻿using UnityEngine;


public class SubDungeon : MonoBehaviour
{
    public static SubDungeon OccupiedSubDungeon;        // The SubDungeon the player is currently in


    public Collectible uniqueItem;
    public Transform uniqueItemContainer;
    public Transform warpInLocation;
    public Transform enemySpawnPointsContainer;


    public SubDungeonSpawnPoint SpawnPoint { get; set; }
    public DungeonRoom ParentDungeonRoom { get { return SpawnPoint.ParentDungeonRoom; } }


    void Start()
    {
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        if (enemySpawnPointsContainer == null) { return; }

        foreach (var sp in enemySpawnPointsContainer.GetComponentsInChildren<EnemySpawnPoint>())
        {
            sp.SpawnEnemy();
        }
    }

    public void OnPlayerEnteredPortal(SubDungeonPortal portal)
    {
        OVRPlayerController pc = CommonObjects.PlayerController_C;
        Transform t = pc.transform;

        SubDungeon destinationSubDungeon = SpawnPoint.warpTo.SpawnSubDungeon();
        Transform warpToLocation = destinationSubDungeon.warpInLocation;

        Vector3 posDiff = warpToLocation.position - portal.transform.position;
        Vector3 eulerDiff = warpToLocation.eulerAngles - portal.transform.eulerAngles;

        Vector3 offset = t.position - portal.transform.position;
        offset = Quaternion.Euler(eulerDiff) * offset;
        t.position = warpToLocation.position + offset;

        Transform camera = pc.CameraController_OVR.transform;
        camera.eulerAngles += eulerDiff;

        CommonObjects.Player_C.ForceNewEulerAngles(camera.eulerAngles);

        pc.Stop();


        ParentDungeonRoom.onPlayerExitedRoom();
        destinationSubDungeon.ParentDungeonRoom.onPlayerEnteredRoom();

        /*print("warpToLocation.eulerAngles: " + warpToLocation.eulerAngles);
        print("portal.transform.eulerAngles: " + portal.transform.eulerAngles);
        print("eulerDiff: " + eulerDiff);
        print("t.forward: " + t.forward);*/
    }

}