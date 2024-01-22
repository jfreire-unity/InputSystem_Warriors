using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    //Player ID
    public static int s_GlobalPlayerCounter;
    private int playerID;

    [Header("Sub Behaviours")]
    public PlayerMovementBehaviour playerMovementBehaviour;
    public PlayerAnimationBehaviour playerAnimationBehaviour;
    public PlayerVisualsBehaviour playerVisualsBehaviour;


    [Header("Input Settings")]
    public PlayerInput playerInput;
    public float movementSmoothingSpeed = 1f;
    private Vector3 rawInputMovement;
    private Vector3 smoothInputMovement;
    
    //Action Maps
    private string actionMapPlayerControls = "Player";
    private string actionMapMenuControls = "UI";

    //Current Control Scheme
    private string currentControlScheme;

    void Start()
    {
        if (GameManager.Instance.currentGameMode == GameMode.LocalMultiplayer)
        {
            // Set initial position 
            transform.position = GameManager.Instance.CalculatePositionInRing();
            transform.rotation = GameManager.Instance.CalculateRotation();
            GameManager.Instance.AddPlayerToActivePlayerList(this);
            SetupPlayer(s_GlobalPlayerCounter++);
        }
    }

    //This is called from the GameManager; when the game is being setup.
    public void SetupPlayer(int newPlayerID)
    {
        playerID = newPlayerID;

        currentControlScheme = playerInput.currentControlScheme;
        
        Debug.Log("currentControlScheme: " + currentControlScheme);

        playerMovementBehaviour.SetupBehaviour();
        playerAnimationBehaviour.SetupBehaviour();
        playerVisualsBehaviour.SetupBehaviour(playerID, playerInput);
    }


    //INPUT SYSTEM ACTION METHODS --------------

    //This is called from PlayerInput; when a joystick or arrow keys has been pushed.
    //It stores the input Vector as a Vector3 to then be used by the smoothing function.


    public void OnMovement(InputAction.CallbackContext value)
    {
        Vector2 inputMovement = value.ReadValue<Vector2>();
        rawInputMovement = new Vector3(inputMovement.x, 0, inputMovement.y);
    }

    //This is called from PlayerInput, when a button has been pushed, that corresponds with the 'Attack' action
    public void OnAttack(InputAction.CallbackContext value)
    {
        if(value.started && !playerAnimationBehaviour.attackInProgress)
        {
            playerAnimationBehaviour.PlayAttackAnimation();
        }
    }

    //This is called from Player Input, when a button has been pushed, that corresponsd with the 'TogglePause' action
    public void OnTogglePause(InputAction.CallbackContext value)
    {
        if(value.started)
        {
            GameManager.Instance.TogglePauseState(this);
        }
    }

    //INPUT SYSTEM AUTOMATIC CALLBACKS --------------

    //This is automatically called from PlayerInput, when the input device has changed
    //(IE: Keyboard -> Xbox Controller)
    public void OnControlsChanged()
    {
        Debug.Log($"player {playerID} input devices: " + playerInput.devices.Count );

        if(playerInput.currentControlScheme != currentControlScheme)
        {
            currentControlScheme = playerInput.currentControlScheme;

            playerVisualsBehaviour.UpdatePlayerVisuals();
            RemoveAllBindingOverrides();
        }
    }

    //This is automatically called from PlayerInput, when the input device has been disconnected and can not be identified
    //IE: Device unplugged or has run out of batteries
    public void OnDeviceLost()
    {
        playerVisualsBehaviour.SetDisconnectedDeviceVisuals();
    }


    public void OnDeviceRegained()
    {
        StartCoroutine(WaitForDeviceToBeRegained());
    }

    IEnumerator WaitForDeviceToBeRegained()
    {
        yield return new WaitForSeconds(0.1f);
        playerVisualsBehaviour.UpdatePlayerVisuals();
    }

    //Update Loop - Used for calculating frame-based data
    void Update()
    {
        CalculateMovementInputSmoothing();
        UpdatePlayerMovement();
        UpdatePlayerAnimationMovement();
    }

    //Input's Axes values are raw
    void CalculateMovementInputSmoothing()
    {
        // Break smoothly once we're attacking
        var rawInputMovementCached = rawInputMovement;
        if (playerAnimationBehaviour.attackInProgress)
        {
            rawInputMovementCached = Vector3.zero;
        }
        smoothInputMovement = Vector3.Lerp(smoothInputMovement, rawInputMovementCached, Time.deltaTime * movementSmoothingSpeed);
    }

    void UpdatePlayerMovement()
    {
        playerMovementBehaviour.UpdateMovementData(smoothInputMovement);
    }

    void UpdatePlayerAnimationMovement()
    {
        playerAnimationBehaviour.UpdateMovementAnimation(smoothInputMovement.magnitude);
    }


    public void SetInputActiveState(bool gameIsPaused)
    {
        switch (gameIsPaused)
        {
            case true:
                playerInput.DeactivateInput();
                break;

            case false:
                playerInput.ActivateInput();
                break;
        }
    }

    void RemoveAllBindingOverrides()
    {
        InputActionRebindingExtensions.RemoveAllBindingOverrides(playerInput.currentActionMap);
    }



    //Switching Action Maps ----


    
    public void EnableGameplayControls()
    {
        playerInput.SwitchCurrentActionMap(actionMapPlayerControls);  
    }

    public void EnablePauseMenuControls()
    {
        playerInput.SwitchCurrentActionMap(actionMapMenuControls);
    }


    //Get Data ----
    public int GetPlayerID()
    {
        return playerID;
    }

    public InputActionAsset GetActionAsset()
    {
        return playerInput.actions;
    }

    public PlayerInput GetPlayerInput()
    {
        return playerInput;
    }


}
