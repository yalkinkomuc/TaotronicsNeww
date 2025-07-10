using UnityEngine;
using UnityEngine.InputSystem;

public class NewInputSystem : IPlayerInput
{
    private PlayerInputActions playerInputActions;
    
    public NewInputSystem()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
    }
    
    // Movement inputs
    public float xInput => playerInputActions.Gameplay.Move.ReadValue<Vector2>().x;
    public float yInput => playerInputActions.Gameplay.Move.ReadValue<Vector2>().y;
    
    // Basic action inputs
    public bool jumpInput => playerInputActions.Gameplay.Jump.WasPressedThisFrame();
    
    //Spell Section
    public bool spell1Input => Mouse.current.rightButton.isPressed && playerInputActions.Gameplay.Spell1.WasPressedThisFrame();
    public bool spell2Input => Mouse.current.rightButton.isPressed && playerInputActions.Gameplay.Spell2.IsPressed();
    public bool spell3Input => Mouse.current.rightButton.isPressed && playerInputActions.Gameplay.Spell3.WasPressedThisFrame();
    public bool spell4Input => Mouse.current.rightButton.isPressed && playerInputActions.Gameplay.Spell4.WasPressedThisFrame();
    public bool spell5Input =>  playerInputActions.Gameplay.Spell5.WasPressedThisFrame();
    public bool spell6Input => Mouse.current.rightButton.isPressed && playerInputActions.Gameplay.Spell6.WasPressedThisFrame();
    public bool spell7Input => Mouse.current.rightButton.isPressed && playerInputActions.Gameplay.Spell7.WasPressedThisFrame();
    
    //Spell Section
    public bool dashInput => playerInputActions.Gameplay.Dash.WasPressedThisFrame(); // Sprint action'ını kullanıyor
    public bool crouchInput => playerInputActions.Gameplay.Crouch.IsPressed();
    public bool crouchInputReleased => playerInputActions.Gameplay.Crouch.WasReleasedThisFrame();
    public bool attackInput => playerInputActions.Gameplay.Attack.WasPressedThisFrame();
    public bool interactionInput => playerInputActions.Gameplay.Interact.WasPressedThisFrame();
    
    public bool parryInput =>playerInputActions.Gameplay.Parry.WasPressedThisFrame();
    
    public bool boomerangInput => playerInputActions.Gameplay.Boomerang.WasPressedThisFrame();
    public bool voidSkillInput => playerInputActions.Gameplay.VoidSkill.WasPressedThisFrame();
    public bool inventoryInput =>playerInputActions.Gameplay.Inventory.WasPressedThisFrame();
    public bool tabLeftInput => playerInputActions.UI.LeftPressed.WasPressedThisFrame();
    public bool tabRightInput => playerInputActions.UI.RightPressed.WasPressedThisFrame();
    public bool escapeInput => playerInputActions.UI.EscapePressed.WasPressedThisFrame();

    
   
   

   
}
