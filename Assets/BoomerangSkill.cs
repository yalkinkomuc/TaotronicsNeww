using System;
using UnityEngine;

public class BoomerangSkill : MonoBehaviour
{

    [SerializeField] private float launchSpeed;
    private void Update()
    {
        transform.Translate(Vector3.right * (launchSpeed * Time.deltaTime));
    }
}
