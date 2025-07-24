using System;
using System.Collections;
using UnityEngine;

public class CameraFollowObject : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;

    [SerializeField] private float flipRotationTime = 0.5f;
    
    private Coroutine turnCoroutine;
    
    private Player player;

    private bool isFacingRight;

    private void Awake()
    {
        player = playerTransform.gameObject.GetComponent<Player>();
        isFacingRight = player.facingright;
    }

    private void Update()
    {
       transform.position = playerTransform.position;
    }

    public void CallTurn()
    {
        LeanTween.rotateY(gameObject,DetermineEndRotation(),flipRotationTime).setEaseOutSine();
    }

    private IEnumerator FlipYLerp()
    {
        float startRotation = transform.localEulerAngles.y;
        float endRotationAmount = DetermineEndRotation();
        float yRotation = 0f;
        
        float elapsedTime = 0f;
        while (elapsedTime < flipRotationTime)
        {
            elapsedTime += Time.deltaTime;
            
            yRotation = Mathf.Lerp(startRotation,endRotationAmount, (elapsedTime / flipRotationTime));
            transform.rotation = Quaternion.Euler(0, yRotation, 0);
            yield return null;
        }
    }

    private float DetermineEndRotation()
    {
        isFacingRight = !isFacingRight;
        if (isFacingRight)
        {
            return 180f;
        }
        
        else
        {
            return 0f;
        }
    }
}
