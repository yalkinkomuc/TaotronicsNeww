using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    public static PlayerInput PlayerInput;
    public static Vector2 MoveInput;
    public static bool WasJumpPressed;
    public static bool IsJumpBeingPressed;
    public static bool WasJumpReleased;
    public static bool WasAttackPressed;
    public static bool WasDashPressed;
    public static bool WasInteractPressed;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _attackAction;
    private InputAction _dashAction;
    private InputAction _interactAction;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        _moveAction = PlayerInput.actions["Movement"];
        _jumpAction = PlayerInput.actions["Jump"];
        _attackAction = PlayerInput.actions["Attack"];
        _dashAction = PlayerInput.actions["Dash"];
        _interactAction = PlayerInput.actions["Interact"];
    }

    private void Update()
    {
        MoveInput = _moveAction.ReadValue<Vector2>();

        WasJumpPressed = _jumpAction.WasPressedThisFrame();
        IsJumpBeingPressed = _jumpAction.IsPressed();
        WasJumpReleased = _jumpAction.WasReleasedThisFrame();
        WasAttackPressed = _attackAction.WasPressedThisFrame();
        WasDashPressed = _dashAction.WasPressedThisFrame();
        WasInteractPressed = _interactAction.WasPressedThisFrame();
    }
}