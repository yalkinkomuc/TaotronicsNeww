using System;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private Vector2 parallaxEffectMultiplier;

    private Transform _transform;
    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    private float textureUnitSizeX;

    private void Start()
    {
        _transform = transform;
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;

        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        Texture2D texture = sprite.texture;

        textureUnitSizeX = ((texture.width * transform.localScale.x) / sprite.pixelsPerUnit);
    }

    private void FixedUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        _transform.position += new Vector3(deltaMovement.x * parallaxEffectMultiplier.x *.1f, deltaMovement.y * parallaxEffectMultiplier.y);
        lastCameraPosition = cameraTransform.position;

        if(Math.Abs(cameraTransform.position.x - _transform.position.x) >= textureUnitSizeX)
        {
            float offsetPositionX = (cameraTransform.position.x - _transform.position.x) % textureUnitSizeX;
            _transform.localPosition = new Vector3(cameraTransform.position.x + offsetPositionX, _transform.localPosition.y);
        }
    }
}


