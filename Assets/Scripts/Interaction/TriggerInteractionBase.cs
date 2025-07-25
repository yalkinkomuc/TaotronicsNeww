using System;
using UnityEngine;

public class TriggerInteractionBase :MonoBehaviour, IInteractable
{
    public NewInputSystem playerInput {get;private set;}
    
    public GameObject Player { get; set; }
    public bool CanInteract { get; set; }


    private void Awake()
    {
        playerInput = new NewInputSystem();
    }

    private void Start()
    {
        Player =  GameObject.FindGameObjectWithTag("Player");
    }


    private void Update()
    {
        if (CanInteract)
        {
            if (playerInput.interactionInput)
            {
                Interact();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject == Player)
        {
            CanInteract = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject == Player)
        {
            CanInteract = false;
        }
    }


    public virtual void Interact()
    {
        
        
        
    }

    public virtual void ShowInteractionPrompt()
    {
        
    }

    public virtual void HideInteractionPrompt()
    {
        
    }
}
