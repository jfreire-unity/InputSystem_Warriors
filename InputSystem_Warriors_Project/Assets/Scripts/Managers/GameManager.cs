using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameMode
{
    SinglePlayer,
    LocalMultiplayer
}

public class GameManager : Singleton<GameManager>
{   

    //Game Mode
    public GameMode currentGameMode;

    //Single Player
    public GameObject inScenePlayer;
    public SinglePlayerCameraMode singlePlayerCameraMode;

    //Local Multiplayer
    public GameObject playerPrefab;
    public int numberOfPlayers;
    static int s_CurrentPlayerId;
    PlayerInputManager playerInputManager;

    public Transform spawnRingCenter;
    public float spawnRingRadius;

    //Spawned Players
    private List<PlayerController> activePlayerControllers;
    private bool isPaused;
    private PlayerController focusedPlayerController;

    void Start()
    {

        isPaused = false;

        playerInputManager = GetComponent<PlayerInputManager>();
        SetupBasedOnGameState();
        SetupUI();
    }

    void SetupBasedOnGameState()
    {
        switch(currentGameMode)
        {
            case GameMode.SinglePlayer:
                SetupSinglePlayer();
                break;

            case GameMode.LocalMultiplayer:
                SetupLocalMultiplayer();
                break;
        }
    }

    void SetupSinglePlayer()
    {
        activePlayerControllers = new List<PlayerController>();

        if(inScenePlayer == true)
        {
            AddPlayerToActivePlayerList(inScenePlayer.GetComponent<PlayerController>());
        }

        SetupActivePlayers();
        SetupSinglePlayerCamera();
    }

    void SetupLocalMultiplayer()
    {   

        if(inScenePlayer == true)
        {
            Destroy(inScenePlayer);
            PlayerController.s_GlobalPlayerCounter = 0;
        }
        
        playerInputManager.enabled = true;
        
        activePlayerControllers = new List<PlayerController>();
        
        // Wait for players to join when new devices are detected and a button is pressed.
    }

    void SpawnPlayers()
    {

        activePlayerControllers = new List<PlayerController>();

        for(int i = 0; i < numberOfPlayers; i++)
        {
            Vector3 spawnPosition = CalculatePositionInRing();
            Quaternion spawnRotation = CalculateRotation();
        
            GameObject spawnedPlayer = Instantiate(playerPrefab, spawnPosition, spawnRotation) as GameObject;
            
            AddPlayerToActivePlayerList(spawnedPlayer.GetComponent<PlayerController>());
        }
    }

    public void AddPlayerToActivePlayerList(PlayerController newPlayer)
    {
        if(activePlayerControllers != null)
            activePlayerControllers.Add(newPlayer);
    }

    void SetupActivePlayers()
    {
        for(int i = 0; i < activePlayerControllers.Count; i++)
        {
            activePlayerControllers[i].SetupPlayer(i);
        }
    }

    void SetupUI()
    {
        UIManager.Instance.SetupManager();
    }

    void SetupSinglePlayerCamera()
    {
        CameraManager.Instance.SetupSinglePlayerCamera(singlePlayerCameraMode);
    }

    public void TogglePauseState(PlayerController newFocusedPlayerController)
    {
        focusedPlayerController = newFocusedPlayerController;

        isPaused = !isPaused;

        ToggleTimeScale();

        UpdateActivePlayerInputs();

        SwitchFocusedPlayerControlScheme();

        UpdateUIMenu();

    }

    void UpdateActivePlayerInputs()
    {
        for(int i = 0; i < activePlayerControllers.Count; i++)
        {
            if(activePlayerControllers[i] != focusedPlayerController)
            {
                 activePlayerControllers[i].SetInputActiveState(isPaused);
            }

        }
    }

    void SwitchFocusedPlayerControlScheme()
    {
        switch(isPaused)
        {
            case true:
                focusedPlayerController.EnablePauseMenuControls();
                break;

            case false:
                focusedPlayerController.EnableGameplayControls();
                break;
        }
    }

    void UpdateUIMenu()
    {
        UIManager.Instance.UpdateUIMenuState(isPaused);
    }

    //Get Data ----

    public List<PlayerController> GetActivePlayerControllers()
    {
        return activePlayerControllers;
    }

    public PlayerController GetFocusedPlayerController()
    {
        return focusedPlayerController;
    }

    public int NumberOfConnectedDevices()
    {
        return InputSystem.devices.Count;
    }
    

    //Pause Utilities ----

    void ToggleTimeScale()
    {
        float newTimeScale = 0f;

        switch(isPaused)
        {
            case true:
                newTimeScale = 0f;
                break;

            case false:
                newTimeScale = 1f;
                break;
        }

        Time.timeScale = newTimeScale;
    }


    //Spawn Utilities

    public Vector3 CalculatePositionInRing()
    {
        if(numberOfPlayers == 1)
            return spawnRingCenter.position;

        float angle = (s_CurrentPlayerId++) * Mathf.PI * 2 / numberOfPlayers;
        float x = Mathf.Cos(angle) * spawnRingRadius;
        float z = Mathf.Sin(angle) * spawnRingRadius;
        
        return spawnRingCenter.position +  new Vector3(x, 0, z);
    }

    public Quaternion CalculateRotation()
    {
        return Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
    }

}
