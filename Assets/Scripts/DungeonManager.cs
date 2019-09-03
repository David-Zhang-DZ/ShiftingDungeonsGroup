﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    //levels
    public List<Level> Levels;

    //stats of various rooms
    private float _player_offset; //-player's y coordinate
    private int _dungeon_size;
    private int _level;
    private int _stage;
    private int _spawn_size;

    //references to rooms and wall
    private GameObject _room_left; //also used to store filler room (dirt)
    public GameObject _room_current;
    private GameObject _room_right;
    private GameObject _room_spawn;
    private GameObject _room_boss;
    public GameObject RoomStage; //should always hold a reference to current stage (if it exists)
    private GameObject _wall;
    private GameObject _wall_another;

    //Dungeon States
    public string DungeonState;
    public bool MobsCleared;
    public bool SpawnBoss;
    public bool BossDefeated;

    //Private variables
    private GameObject Player;
    private GameObject Grid;
    private GameObject CameraObject;
    private MonsterSpawner MonsterSpawner;

    private void Start()
    {
        _player_offset = 3.5f;
        _dungeon_size = 18;
        _spawn_size = 40;
        _level = 0;

        MonsterSpawner = FindObjectOfType<MonsterSpawner>().GetComponent<MonsterSpawner>();
        if (MonsterSpawner == null)
            Debug.Log("DungeonManager could not find MonsterSpawner");
        Player = FindObjectOfType<PlayerMovement>().gameObject;
        if (Player == null)
            Debug.Log("DungeonManager could not find Player");
        Grid = FindObjectOfType<Grid>().gameObject;
        if (Grid == null)
            Debug.Log("DungeonManager could not find Grid");
        CameraObject = FindObjectOfType<Camera>().gameObject;
        if (CameraObject == null)
            Debug.Log("DungeonManager could not find Camera");
        CameraObject.GetComponent<CameraMovement>().CameraState = "Follow";

        //Instantiate Spawn for the first time
        _room_spawn = MyInstantiate(
                    Levels[_level].Spawn,
                    Player.transform.position.x, 0);
        _room_left = MyInstantiate(
                    Levels[_level].Filler,
                    Player.transform.position.x - _spawn_size/2 - _dungeon_size/2, 0);
        RoomStage = _room_current = MyInstantiate(
                    Levels[_level].Stages[_stage],
                    Player.transform.position.x + _spawn_size / 2 + _dungeon_size / 2, 0);

        _wall = MyInstantiate(
                    Levels[_level].Walls.SpawnWallsRight,
                    Player.transform.position.x, 0);
        _wall_another = MyInstantiate(
                    Levels[_level].Walls.StaticWallsLeft,
                    _room_current.transform.position.x, 0);

        //Update Dungeon State
        DungeonState = "Spawn";
    }

    void Update()
    {
        switch (DungeonState)
        {
            #region Spawn
            case "Spawn":
                if (Player.transform.position.x > _room_current.transform.position.x)
                {
                    //Lock Camera
                    CameraObject.GetComponent<CameraMovement>().CameraState = "Locked";
                    CameraObject.transform.position = new Vector3(_room_current.transform.position.x, CameraObject.transform.position.y, -10);

                    //Destroy previous rooms
                    Destroy(_room_spawn);
                    Destroy(_room_left);

                    //Instatiate new rooms
                    _room_left = MyInstantiate(
                        Levels[_level].Stages[0],
                        _room_current.transform.position.x - _dungeon_size,
                        _room_current.transform.position.y);
                    _room_right = MyInstantiate(
                        Levels[_level].Stages[0],
                        _room_current.transform.position.x + _dungeon_size,
                        _room_current.transform.position.y);

                    //Update Walls
                    Destroy(_wall);
                    Destroy(_wall_another);
                    _wall = MyInstantiate(
                        Levels[_level].Walls.StaticWalls,
                        _room_current.transform.position.x,
                        _room_current.transform.position.y);

                    DungeonState = "Stage";

                    MonsterSpawner.SpawnMobs(_level, _stage);
                }
                break;
            #endregion
            #region Stage
            case "Stage":
                if (MobsCleared)
                {
                    //Wait for certain conditions then transition to next state
                    if (Player.transform.position.x <= _room_current.transform.position.x + 0.05 &&
                        Player.transform.position.x >= _room_current.transform.position.x - 0.05)
                    {
                        //Update Walls
                        Destroy(_wall);
                        _wall = MyInstantiate(
                            Levels[_level].Walls.ShiftingWalls,
                            _room_current.transform.position.x,
                            _room_current.transform.position.y);
                        _wall_another = MyInstantiate(
                            Levels[_level].Walls.ShiftingWallsAnimated,
                            _room_current.transform.position.x,
                            _room_current.transform.position.y);

                        CameraObject.GetComponent<CameraMovement>().CameraState = "Follow";
                        MobsCleared = false;
                        _stage++;
                        DungeonState = "TransitionToShifting";
                    }
                }
                break;
            #endregion
            #region TransitionToShifting
            case "TransitionToShifting":
                //Loop empty rooms
                if (Player.transform.position.x > _room_current.transform.position.x + 10)
                {
                    Destroy(_room_left);
                    _room_left = _room_current;
                    _room_current = _room_right;
                    if (!SpawnBoss)
                    {
                        RoomStage = _room_right = MyInstantiate(
                            Levels[_level].Stages[_stage],
                            _room_current.transform.position.x + _dungeon_size,
                            _room_current.transform.position.y);
                    }
                    else if (SpawnBoss)
                    {
                        _room_right = MyInstantiate(
                            Levels[_level].Stages[0],
                            _room_current.transform.position.x + _dungeon_size,
                            _room_current.transform.position.y);
                    }
                    DungeonState = "Shifting";
                }
                else if (Player.transform.position.x < _room_current.transform.position.x - 10)
                {
                    Destroy(_room_right);
                    _room_right = _room_current;
                    _room_current = _room_left;
                    if (!SpawnBoss)
                    {
                        RoomStage = _room_left = MyInstantiate(
                            Levels[_level].Stages[_stage],
                            _room_current.transform.position.x - _dungeon_size,
                            _room_current.transform.position.y);
                    }
                    else if (SpawnBoss)
                    {
                        _room_left = MyInstantiate(
                            Levels[_level].Stages[0],
                            _room_current.transform.position.x - _dungeon_size,
                            _room_current.transform.position.y);
                    }
                    DungeonState = "Shifting";
                }
                break;
            #endregion
            #region Shifting
            case "Shifting":
                //Continue looping rooms (room depends on whether or not to spawn boss)
                if (Player.transform.position.x > _room_current.transform.position.x + 10)
                {
                    if (_room_left == RoomStage)
                        RoomStage = null; //Destroy won't change RoomStage to null before check
                    Destroy(_room_left);
                    _room_left = _room_current;
                    _room_current = _room_right;
                    if (!SpawnBoss)
                    {
                        _room_right = MyInstantiate(
                            Levels[_level].Stages[_stage],
                            _room_current.transform.position.x + _dungeon_size,
                            _room_current.transform.position.y);
                        if (RoomStage == null)
                            RoomStage = _room_right;
                    }
                    else
                        _room_right = MyInstantiate(
                            Levels[_level].Stages[0],
                            _room_current.transform.position.x + _dungeon_size,
                            _room_current.transform.position.y);
                }
                else if (Player.transform.position.x < _room_current.transform.position.x - 10)
                {
                    if (_room_right == RoomStage)
                        RoomStage = null; //Destroy won't change RoomStage to null before check
                    Destroy(_room_right);
                    _room_right = _room_current;
                    _room_current = _room_left;
                    if (!SpawnBoss)
                    {
                        _room_left = MyInstantiate(
                            Levels[_level].Stages[_stage],
                            _room_current.transform.position.x - _dungeon_size,
                            _room_current.transform.position.y);
                        if (RoomStage == null)
                            RoomStage = _room_left;
                    }
                    else
                        _room_left = MyInstantiate(
                            Levels[_level].Stages[0],
                            _room_current.transform.position.x - _dungeon_size,
                            _room_current.transform.position.y);
                }

                //If not spawning boss, wait for certain conditions, then transition back to stage
                if (!SpawnBoss)
                {
                    if (Player.transform.position.x <= _room_current.transform.position.x + 0.05 &&
                        Player.transform.position.x >= _room_current.transform.position.x - 0.05 &&
                        _room_current == RoomStage)
                    {
                        //Lock Camera
                        CameraObject.GetComponent<CameraMovement>().CameraState = "Locked";
                        CameraObject.transform.position = new Vector3(_room_current.transform.position.x, CameraObject.transform.position.y, -10);

                        //Destroy previous rooms
                        Destroy(_room_left);
                        Destroy(_room_right);

                        //Instatiate new rooms
                        _room_left = MyInstantiate(
                            Levels[_level].Stages[0],
                            _room_current.transform.position.x - _dungeon_size,
                            _room_current.transform.position.y);
                        _room_right = MyInstantiate(
                            Levels[_level].Stages[0],
                            _room_current.transform.position.x + _dungeon_size,
                            _room_current.transform.position.y);

                        //Update Walls
                        Destroy(_wall);
                        Destroy(_wall_another);
                        _wall = MyInstantiate(
                            Levels[_level].Walls.StaticWalls,
                            _room_current.transform.position.x,
                            _room_current.transform.position.y);

                        DungeonState = "Stage";

                        MonsterSpawner.SpawnMobs(_level, _stage);
                        break;
                    }
                }

                //When conditions for boss fight are met, Wait for certain conditions then transition to boss fight state
                if (SpawnBoss)
                {
                    if (Player.transform.position.x <= _room_current.transform.position.x + 0.05 &&
                        Player.transform.position.x >= _room_current.transform.position.x - 0.05 &&
                        RoomStage == null)
                    {
                        //Lock Camera
                        CameraObject.GetComponent<CameraMovement>().CameraState = "Locked";
                        CameraObject.transform.position = new Vector3(_room_current.transform.position.x, CameraObject.transform.position.y, -10);

                        //Update Rooms
                        Destroy(_room_left);
                        Destroy(_room_right);

                        _room_boss = MyInstantiate(
                            Levels[_level].BossRoom,
                            _room_current.transform.position.x + _dungeon_size,
                            _room_current.transform.position.y);

                        //Updates Walls
                        Destroy(_wall);
                        Destroy(_wall_another);
                        _wall = MyInstantiate(
                            Levels[_level].Walls.StaticWallsRight,
                            _room_current.transform.position.x,
                            _room_current.transform.position.y);
                        _wall_another = MyInstantiate(
                            Levels[_level].Walls.StaticWallsLeft,
                            _room_current.transform.position.x + _dungeon_size,
                            _room_current.transform.position.y);

                        DungeonState = "TransitionToBossA";
                        SpawnBoss = false;
                        _stage = 0;
                    }
                }
                break;
            #endregion
            #region TransitionToBoss
            case "TransitionToBossA": //Player is in current room
                //Move Camera to boss room
                if (Player.transform.position.x > _room_current.transform.position.x + _dungeon_size / 2)
                {
                    CameraObject.GetComponent<CameraMovement>().ShiftRight(_dungeon_size);
                    DungeonState = "TransitionToBossB";
                }
                break;

            case "TransitionToBossB": //Player is in boss room
                //Move Camera to curr room
                if (Player.transform.position.x < _room_boss.transform.position.x - _dungeon_size / 2)
                {
                    CameraObject.GetComponent<CameraMovement>().ShiftLeft(_dungeon_size);
                    DungeonState = "TransitionToBossA";
                }
                //Transition to boss fight
                if (Player.transform.position.x > _room_boss.transform.position.x - _dungeon_size / 4)
                {
                    Destroy(_room_current);

                    Destroy(_wall);
                    Destroy(_wall_another);

                    _wall = MyInstantiate(
                        Levels[_level].Walls.StaticWalls,
                        _room_boss.transform.position.x,
                        _room_boss.transform.position.y);

                    DungeonState = "BossFight";
                }
                break;
            #endregion
            #region BossFight
            case "BossFight":
                if (BossDefeated)
                {
                    _room_spawn = MyInstantiate(
                        Levels[_level].Spawn,
                        _room_boss.transform.position.x + _dungeon_size / 2 + _spawn_size / 2,
                        _room_boss.transform.position.y);
                    _room_current = MyInstantiate(
                        Levels[_level].Stages[_stage],
                        _room_boss.transform.position.x + _dungeon_size + _spawn_size,
                        _room_boss.transform.position.y);

                    Destroy(_wall);

                    _wall = MyInstantiate(
                        Levels[_level].Walls.SpawnWallsLeft,
                        _room_spawn.transform.position.x,
                        _room_spawn.transform.position.y);
                    _wall_another = MyInstantiate(
                        Levels[_level].Walls.StaticWallsRight,
                        _room_boss.transform.position.x,
                        _room_boss.transform.position.y);

                    DungeonState = "TransitionToSpawnA";
                    BossDefeated = false;
                }
                break;
            #endregion
            #region TransitionToSpawn
            case "TransitionToSpawnA": //Player is in boss room
                //Move Camera to spawn room
                if (Player.transform.position.x > _room_boss.transform.position.x + _dungeon_size / 2)
                {
                    CameraObject.GetComponent<CameraMovement>().ShiftRight(_dungeon_size);
                    DungeonState = "TransitionToSpawnB";
                }
                break;

            case "TransitionToSpawnB": //Player is in spawn room
                //Move Camera to boss room
                if (Player.transform.position.x < _room_spawn.transform.position.x - _spawn_size / 2)
                {
                    CameraObject.GetComponent<CameraMovement>().ShiftLeft(_dungeon_size);
                    DungeonState = "TransitionToSpawnA";
                }

                //Transition to Game State Spawn
                if (Player.transform.position.x > _room_spawn.transform.position.x - _spawn_size / 2 
                        + CameraObject.GetComponent<Camera>().orthographicSize * 2)
                {
                    CameraObject.GetComponent<CameraMovement>().CameraState = "Follow";

                    Destroy(_room_boss);

                    _room_left = MyInstantiate(
                        Levels[_level].Filler,
                        _room_spawn.transform.position.x - _spawn_size / 2 - _dungeon_size / 2,
                        _room_spawn.transform.position.y);

                    Destroy(_wall);
                    Destroy(_wall_another);

                    _wall = MyInstantiate(
                        Levels[_level].Walls.SpawnWallsRight,
                        _room_spawn.transform.position.x,
                        _room_spawn.transform.position.y);
                    _wall_another = MyInstantiate(
                        Levels[_level].Walls.StaticWallsLeft, 
                        _room_current.transform.position.x, 
                        _room_current.transform.position.y);

                    DungeonState = "Spawn";
                }
                break;
                #endregion
            
        }
    }

    //used to decrease lines of code
    private GameObject MyInstantiate(GameObject obj, float x, float y)
    {
        return Instantiate(obj, new Vector3(x, y, 0), new Quaternion(0, 0, 0, 0), Grid.transform);
    }
}
