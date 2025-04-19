using UnityEngine;

public class GamepadInput : IPlayerInput
{
    // Tüm input sistemini devre dışı bırakır (UI dahil)
    private bool inputEnabled = true;
    
    // SADECE oyun içi inputlarını devre dışı bırakır (UI hariç)
    private bool gameplayInputEnabled = true;

    // Hareket ve aksiyon inputları - bunlar gameplayInputEnabled'dan etkilenir
    public float xInput => inputEnabled && gameplayInputEnabled ? Input.GetAxisRaw("Horizontal") : 0f;
    public float yInput => inputEnabled && gameplayInputEnabled ? Input.GetAxisRaw("Vertical") : 0f;
    public bool jumpInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.JoystickButton0);
    public bool dashInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.JoystickButton1);
    public bool crouchInput => inputEnabled && gameplayInputEnabled && yInput < -0.5f;
    public bool crouchInputReleased => inputEnabled && gameplayInputEnabled && yInput >= -0.5f;
    public bool attackInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.JoystickButton2);
    public bool interactionInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.JoystickButton3);
    public bool parryInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.JoystickButton4);
    public bool spell1Input => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.JoystickButton7);
    public bool spell2Input => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.JoystickButton8);
    public bool boomerangInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.JoystickButton9);
    
    // Tüm inputları devre dışı bırakır (UI inputları da dahil)
    public void DisableAllInput()
    {
        inputEnabled = false;
        gameplayInputEnabled = false;
    }
    
    // Tüm inputları etkinleştirir
    public void EnableAllInput()
    {
        inputEnabled = true;
        gameplayInputEnabled = true;
    }
    
    // SADECE oyun inputlarını devre dışı bırakır (UI inputları etkin kalır)
    public void DisableGameplayInput()
    {
        gameplayInputEnabled = false;
        inputEnabled = true; // Input sisteminin kendisi açık kalmalı, sadece oyun girdileri kapanmalı
    }
}
