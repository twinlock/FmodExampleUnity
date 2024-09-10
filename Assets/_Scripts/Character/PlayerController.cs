using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _speed = 5.0f;
    [SerializeField] private CharacterController _controller;
    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        // please don't do this, this is to simulate animation events from the animator
        
    }
    private void FixedUpdate() {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontalInput, 0, verticalInput);
        Vector3 velocity = direction * _speed;

        _controller.Move(velocity * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
