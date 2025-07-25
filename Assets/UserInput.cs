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
    public static bool WasCrouchPressed;
    public static bool WasCrouchReleased;
    public static bool WasAttackPressed;
    public static bool WasDashPressed;
    public static bool WasInteractPressed;
    public static bool WasParryPressed;
    public static bool WasEscapePressed;
    public static bool WasSpell1Pressed;
    public static bool WasSpell2Pressed;
    public static bool WasSpell3Pressed;
    public static bool WasSpell4Pressed;
    public static bool WasSpell5Pressed;
    public static bool WasSpell6Pressed;
    public static bool WasSpell7Pressed;
    public static bool WasVoidInputPressed;
    public static bool WasInventoryPressed;
    public static bool WasBoomerangPressed;
    public static bool WasTabLeftPressed;
    public static bool WasTabRightPressed;
    

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _attackAction;
    private InputAction _dashAction;
    private InputAction _interactAction;
    private InputAction _escapeAction;
    private InputAction _parryAction;
    private InputAction _spell1Action;
    private InputAction _spell2Action;
    private InputAction _spell3Action;
    private InputAction _spell4Action;
    private InputAction _spell5Action;
    private InputAction _spell6Action;
    private InputAction _spell7Action;
    private InputAction _voidAction;
    private InputAction _inventoryAction;
    private InputAction _boomerangAction;
    private InputAction _tabLeftAction;
    private InputAction _tabRightAction;
    private InputAction _crouchAction;
    

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();

        _moveAction = PlayerInput.actions["Movement"];
        _jumpAction = PlayerInput.actions["Jump"];
        _attackAction = PlayerInput.actions["Attack"];
        _dashAction = PlayerInput.actions["Dash"];
        _interactAction = PlayerInput.actions["Interact"];
        _escapeAction = PlayerInput.actions["Escape"];
        _parryAction = PlayerInput.actions["Parry"];
        _crouchAction = PlayerInput.actions["Crouch"];
        _spell1Action = PlayerInput.actions["Spell1"];
        _spell2Action = PlayerInput.actions["Spell2"];
        _spell3Action = PlayerInput.actions["Spell3"];
        _spell4Action = PlayerInput.actions["Spell4"];
        _spell5Action = PlayerInput.actions["Spell5"];
        _spell6Action = PlayerInput.actions["Spell6"];
        _spell7Action = PlayerInput.actions["Spell7"];
        _voidAction = PlayerInput.actions["VoidSpell"];
        _inventoryAction = PlayerInput.actions["Inventory"];
        _boomerangAction = PlayerInput.actions["BoomerangThrow"];
        _tabLeftAction = PlayerInput.actions["TabLeft"];
        _tabRightAction = PlayerInput.actions["TabRight"];
        
    }

    private void Update()
    {
        MoveInput = _moveAction.ReadValue<Vector2>();

        WasJumpPressed = _jumpAction.WasPressedThisFrame();
        IsJumpBeingPressed = _jumpAction.IsPressed();
        WasJumpReleased = _jumpAction.WasReleasedThisFrame();
        WasAttackPressed = _attackAction.WasPressedThisFrame();
        WasDashPressed = _dashAction.WasPressedThisFrame();
        WasCrouchPressed =  _crouchAction.WasPressedThisFrame();
        WasCrouchReleased = _crouchAction.WasReleasedThisFrame();
        WasInteractPressed = _interactAction.WasPressedThisFrame();
        WasParryPressed = _parryAction.WasPressedThisFrame();
        WasEscapePressed = _escapeAction.WasPressedThisFrame();
        WasSpell1Pressed = _spell1Action.WasPressedThisFrame();
        WasSpell2Pressed = _spell2Action.WasPressedThisFrame();
        WasSpell3Pressed = _spell3Action.WasPressedThisFrame();
        WasSpell4Pressed = _spell4Action.WasPressedThisFrame();
        WasSpell5Pressed = _spell5Action.WasPressedThisFrame();
        WasSpell6Pressed = _spell6Action.WasPressedThisFrame();
        WasSpell7Pressed = _spell7Action.WasPressedThisFrame();
        WasVoidInputPressed = _voidAction.WasPressedThisFrame();
        WasInventoryPressed = _inventoryAction.WasPressedThisFrame();
        WasBoomerangPressed = _boomerangAction.WasPressedThisFrame();
        WasTabLeftPressed = _tabLeftAction.WasPressedThisFrame();
        WasTabRightPressed = _tabRightAction.WasPressedThisFrame();
        
    }
}