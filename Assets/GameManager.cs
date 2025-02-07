using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int loopCount;
    [SerializeField] private List<Transform> enemyCarSpawnPoints;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private GameObject enemyCarPrefab;
    [SerializeField] private GameObject playerCarPrefab;
    public Dictionary<CarPhysics, int> spawnedCarLap = new Dictionary<CarPhysics, int>();

    [SerializeField] private GameObject endRaceUI;

    [SerializeField] private GameObject informationCanvasElement;
    [SerializeField] private TextMeshProUGUI loopCounter;
    private bool GameIsStarted = false;

    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private CarPhysics playerCarPhysics;
    [SerializeField] private CarPath playerPath;
    
    [Header("VR Things")] 
    [SerializeField] private Button startRaceButton;
    [SerializeField] private GameObject uiGameObject;

    public bool IsDebug;

    
    
    

    private void Awake()
    {
        SpawnCar();
        loopCounter.text = loopCounter.text = $"0/{loopCount}";
    }

    private void Start()
    {
        if (startRaceButton == null || uiGameObject == null) return;
        startRaceButton.onClick.RemoveAllListeners();
        startRaceButton.onClick.AddListener(StartGameButtonClicked);
        
    }

   private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsDebug)
        {
            if (informationCanvasElement.activeSelf && !GameIsStarted)
            {
                informationCanvasElement.SetActive(false);
                GameIsStarted = true;
                StartGame();
            }
        }
    }

    public void StartGameButtonClicked()
    {
        /*if (!informationCanvasElement.activeSelf || GameIsStarted) return;
        informationCanvasElement.SetActive(false);*/
        GameIsStarted = true;
        StartGame();
        Destroy(uiGameObject);
    }
    

    public void SpawnCar()
    {
        CarPath path = GameObject.FindObjectOfType<CarPath>();

        foreach (var spawnPoint in enemyCarSpawnPoints)
        {
            GameObject carPathObj = new GameObject();
            CarPath newPath = carPathObj.AddComponent<CarPath>();
            newPath.SetUpPath(path);
            GameObject carPrefab = Instantiate(enemyCarPrefab, spawnPoint.position, spawnPoint.rotation);
            CarPhysics carPhysics = carPrefab.GetComponent<CarPhysics>();
            CarAi carAI = carPrefab.GetComponent<CarAi>();
            carAI.carPath = newPath;
            carAI.StartAI();

            spawnedCarLap.Add(carPhysics, -1);
        }

        GameObject carPathObjForPlayer = new GameObject();
        playerPath = carPathObjForPlayer.AddComponent<CarPath>();
        playerPath.SetUpPath(path);

        
        GameObject playerCarObj = Instantiate(playerCarPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
        PlayerWaypointSystem playerWaypointSystem = playerCarObj.GetComponent<PlayerWaypointSystem>();
        
        playerWaypointSystem.SetUpPath(playerPath);
        
        playerCarPhysics = playerCarObj.GetComponent<CarPhysics>();
        spawnedCarLap.Add(playerCarPhysics, -1);

        foreach (var car in spawnedCarLap)
        {
            car.Key.canRide = false;
        }
    }

    public void StartGame()
    {
        foreach (var car in spawnedCarLap)
        {
            car.Key.canRide = true;
        }
    }

    public void AddLoop(CarPhysics carPhysics)
    {
        if (spawnedCarLap.ContainsKey(carPhysics))
        {
            
            if (carPhysics.controlAI)
            {
                if (spawnedCarLap[carPhysics] == loopCount)
                {
                    carPhysics.canRide = false;
                    carPhysics.gameObject.GetComponent<Collider>().isTrigger = true;
                }
                else
                {
                    spawnedCarLap[carPhysics]++;
                }
            }
            else
            {
                Debug.Log($"end lopop {carPhysics.gameObject.name} ");

                if (carPhysics.gameObject.GetComponent<PlayerWaypointSystem>().currAllWaypointsindex == 0)
                {
                    Debug.Log("end lopop");
                    if (spawnedCarLap[carPhysics] == loopCount)
                    {
                        EndRace();
                    }
                    else
                    {
                        spawnedCarLap[carPhysics]++;
                        loopCounter.text = $"{spawnedCarLap[carPhysics]}/{loopCount}";
                    }
                }
                else
                {
                    Debug.Log("no end lopop " + carPhysics.gameObject.GetComponent<PlayerWaypointSystem>().currAllWaypointsindex);

                }
            }
        }
    }

    private void EndRace()
    {
        endRaceUI.SetActive(true);
    }

    public void ResetRaceButton()
    {
        SceneManager.LoadScene("EndScene");
    }

    public void RestPositionButton()
    {
        if (playerPath == null || playerCarPhysics == null)
        {
            Debug.Log("NO PLAYER ON SCENE");
            return;
        }
        Vector3 lastWaypoint = playerPath.GetPreviousWaypointPosition();
        Vector3 nextWaypoint = playerPath.GetCurrWaypointPosition();

        // Przesunięcie w górę o 2 jednostki, by uniknąć kolizji
        playerCarPhysics.transform.position = lastWaypoint + Vector3.up * 2f;

        // Obrót w kierunku następnego waypointa
        Vector3 directionToNext = (nextWaypoint - lastWaypoint).normalized;
        playerCarPhysics.transform.rotation = Quaternion.LookRotation(directionToNext, Vector3.up);

        playerCarPhysics.ResetVelocity(); // Opcjonalnie, jeśli CarPhysics ma taką metodę
    }
}