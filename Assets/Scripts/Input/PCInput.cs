using UnityEngine;

public class PCInput : IPlayerInput
{
    // Tüm input sistemini devre dışı bırakır (UI dahil)
    private bool inputEnabled = true;
    
    // SADECE oyun içi inputlarını devre dışı bırakır (UI hariç)
    private bool gameplayInputEnabled = true;
   
    // Hareket ve aksiyon inputları - bunlar gameplayInputEnabled'dan etkilenir
    public float xInput => inputEnabled && gameplayInputEnabled ? Input.GetAxisRaw("Horizontal") : 0f;
    public float yInput => inputEnabled && gameplayInputEnabled ? Input.GetAxisRaw("Vertical") : 0f;
    public bool jumpInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.Space);
    public bool dashInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.LeftShift);
    public bool crouchInput => inputEnabled && gameplayInputEnabled && Input.GetKey(KeyCode.S);
    public bool crouchInputReleased => inputEnabled && gameplayInputEnabled && Input.GetKeyUp(KeyCode.S);
    public bool attackInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.Mouse0);
    public bool interactionInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.E);
    
    public bool parryInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.Q);
    public bool spell1Input => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.R);
    public bool spell2Input => inputEnabled && gameplayInputEnabled && Input.GetKey(KeyCode.T);
    public bool boomerangInput => inputEnabled && gameplayInputEnabled && Input.GetKeyDown(KeyCode.Mouse1);
    
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
