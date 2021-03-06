﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        Instance = this;
    }


    [SerializeField] private int _currentWave = -1;
    [SerializeField] private int lvl1Waves = 3, lvl2Waves = 3, lvl3Waves = 2;
    private List<WaveInfo> _waves;

    private List<EnemyBase> _enemiesThisWave;
    private List<Vector3> _positionsToSpawn;
    private int _waveDifficulty;
    private int _currentDifficulty;

    private int _goldenDifficulty;
    private int _gCurrentDifficulty;
    private List<EnemyBase> _goldenThisWave;
    private int piecesToEndWave;
    public int PiecesToEndWave { get => piecesToEndWave; set => piecesToEndWave = value; }
    public List<WaveInfo> Waves { get => _waves; }
    public int CurrentWave { get => _currentWave; }

    //private int _percPerSubwave;

    private List<EnemyBase> _createdEnemies;

    public UnityEngine.UI.Text debugText;

    bool isSpawning = true, debugStopSpawn = false, roundsStarted = false;
    float tPerGolden = 0;
    [SerializeField] float cdPerGolden = 8;
    float tPerSpawn = 0;
    [SerializeField] float cdPerSpawn = 0.75f;
    private EnemyBase lastEnemySpawned = null;

    private void Start()
    {
        _waves = Resources.Load<WaveList>("WaveList").waves;
    }

    public void Init()
    {
        roundsStarted = true;

        UIManager.Instance.UpdateRoundText(1, _waves.Count);
        UIManager.Instance.UpdatePiecesText(_waves[0].GoldenEnemiesThisWave.Count);

        StartCoroutine(FirstWave());
    }
    IEnumerator FirstWave()
    {
        yield return new WaitForSeconds(2);

        EndWave();
    }

    private void UpdateWave()
    {
        _enemiesThisWave = _waves[_currentWave].EnemiesThisWave;
        _positionsToSpawn = _waves[_currentWave].PositionsToSpawn;

        cdPerSpawn = _waves[_currentWave].CdBetweenEnemiesSpawn;
        cdPerGolden = _waves[_currentWave].CdBetweenGoldenSpawn;

        _waveDifficulty = _waves[_currentWave].TotalDifficulty;
        _currentDifficulty = _waves[_currentWave].TotalDifficulty;

        _goldenDifficulty = _waves[_currentWave].GoldenDifficulty;
        _goldenThisWave = new List<EnemyBase>(_waves[_currentWave].GoldenEnemiesThisWave);
        _gCurrentDifficulty = _goldenDifficulty;
        piecesToEndWave = _waves[_currentWave].GoldenEnemiesThisWave.Count;
    }
    public void PrepareToEndWave()
    {
        isSpawning = false;
        tPerSpawn = 0;
        tPerGolden = 0;
    }

    bool isOnLvl2 = false, isOnLvl3 = false;
    private void EndWave()
    {
        //END WAVE STUFF
        if (_currentWave == lvl1Waves - 1 && !isOnLvl2)
        {
            SceneChangeManager.Instance.LoadLevel(2);
            isOnLvl2 = true;
        }
        else if (_currentWave == lvl1Waves + lvl2Waves - 1 && !isOnLvl3)
        {
            SceneChangeManager.Instance.LoadLevel(3);
            isOnLvl3 = true;
        }

        WeaponManager.Instance._player.CanMove(false);
        if (_currentWave < _waves.Count)
        {
            ShowWeaponChoosingPanel(true);
        }
        else
        {
            GameManager.Instance.EndGame(true);
        }
    }
    private void ShowWeaponChoosingPanel(bool show)
    {
        WeaponManager.Instance.Weapons[WeaponManager.Instance.selectedWeapon].GetComponent<ShotBase>().IsShooting(false);
        WeaponPrefabsLists.Instance.inventory.SetActive(show);
    }
    public void BeginNextWave()
    {
        WeaponManager.Instance._player.CanMove(true);

        WeaponManager.Instance.UpdateWeapons();

        ShowWeaponChoosingPanel(false);
        _currentWave++;
        UpdateWave();
        isSpawning = true;

        UIManager.Instance.UpdateRoundText(_currentWave + 1, _waves.Count);
        UIManager.Instance.UpdatePiecesText(PiecesToEndWave);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O) && InputManager.Instance.debug)
        {
            debugStopSpawn = !debugStopSpawn;
        }

        if (roundsStarted)
        {

            if (isSpawning)
            {
                if (!debugStopSpawn)
                {

                    if (_currentDifficulty > 0)
                    {
                        tPerSpawn += Time.deltaTime;
                        if (tPerSpawn >= cdPerSpawn)
                        {
                            tPerSpawn = 0;
                            SpawnEnemy(_enemiesThisWave, ref _currentDifficulty);
                        }
                    }
                    if (_goldenDifficulty > 0)
                    {
                        tPerGolden += Time.deltaTime;
                        if (tPerGolden >= cdPerGolden)
                        {
                            tPerGolden = 0;
                            SpawnEnemy(_goldenThisWave, ref _gCurrentDifficulty);
                            _goldenThisWave.Remove(lastEnemySpawned);
                        }
                    }
                }
            }
            else
            {
                if (_currentDifficulty == _waveDifficulty)
                {
                    EndWave();
                }
            }
        }
        if (debugText != null)
        {
            debugText.text = "Difficulty: " + _currentDifficulty;
        }
    }

    private void SpawnEnemy(List<EnemyBase> enemyList, ref int difficultyToReduce)
    {
        List<EnemyBase> possibleEnemies = new List<EnemyBase>();

        for (int i = 0; i < enemyList.Count; i++)
        {
            if (enemyList[i].Difficulty <= difficultyToReduce)
            {
                possibleEnemies.Add(enemyList[i]);
            }
        }

        if (possibleEnemies.Count > 0)
        {
            int ran = Random.Range(0, possibleEnemies.Count);

            Instantiate(possibleEnemies[ran].gameObject, GetSpawnPosition(), Quaternion.identity);

            lastEnemySpawned = possibleEnemies[ran];
            difficultyToReduce -= possibleEnemies[ran].Difficulty;
        }
    }
    private Vector3 GetSpawnPosition()
    {
        int ran = Random.Range(0, _positionsToSpawn.Count);

        return _positionsToSpawn[ran];
    }
    public void AddDifficulty(int add, bool golden)
    {
        if (!golden)
        {
            _currentDifficulty += add;
        }
        else
        {
            _gCurrentDifficulty += add;
        }
    }
}
