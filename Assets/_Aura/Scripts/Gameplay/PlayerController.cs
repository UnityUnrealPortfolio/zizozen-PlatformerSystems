using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This component is responsible for getting player input and
/// passes it as a movement value to the Character Controller Class
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Player Properties")]
    public float walkSpeed = 10f;
    public float gravity = 20f;
    public float jumpSpeed = 15f;

    //player state
    public bool isJumping;

    //input flags
    [SerializeField]private bool _startJump;
    [SerializeField]private bool _releaseJump;

    //Input received from the controller device
    [SerializeField] private Vector2 _input;

    //a direction calculated from the received input
    private Vector2 _moveDirection;

    //CharacterController2D that has the Move() function that will receive the calculated
    //input from the Device
    private CharacterController2D _characterController;

    private void Start()
    {
        _characterController = GetComponent<CharacterController2D>();
    }
    private void Update()
    {
        _moveDirection.x = _input.x;
        _moveDirection.x *= walkSpeed;


        //only calculate gravity if we are not grounded
        if (_characterController.isSomethingBelow)
        {
            isJumping = false;

            if (_startJump)
            {
                _startJump = false;
                _moveDirection.y = jumpSpeed;
                isJumping = true;
                _characterController.DisableGroundCheck();
            }
        }
        else
        {
            _startJump = false;
            if (_releaseJump)
            {
                _releaseJump = false;
                if(_moveDirection.y > 0)
                {
                    _moveDirection.y *= 0.5f;
                }
            }
            _moveDirection.y -= gravity * Time.deltaTime;
        }

        _characterController.Move(_moveDirection * Time.deltaTime);

    }

    #region Input Methods
    public void OnMoveInputAction(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }

    public void OnJumpInputAction(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _startJump = true;
        }
        else if (context.canceled)
        {
            _releaseJump = true;
        }
    }
    #endregion
}
