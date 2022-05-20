using UnityEngine;
using System.Collections;
using GlobalTypes;

public class CharacterController2D : MonoBehaviour
{
    [Header("Ground detection properties")]
    public float raycastDistance = 0.4f;
    public LayerMask levelGeomLayerMask;
    public bool isSomethingBelow;
    public float groundCheckTimer = 0.1f;
    public PlatformType platformType;
    private bool _disableGroundCheck;

    //raycast positions (0 - to the left, 1 - dead center, 2 - to the right)
    private Vector2[] _raycastPositions = new Vector2[3];
    RaycastHit2D[] _raycastHits = new RaycastHit2D[3];

    private Rigidbody2D _rigidBody;
    private CapsuleCollider2D _capsuleCollider;

    private Vector2 _moveAmount;
    private Vector2 _currentPosition;
    private Vector2 _lastPosition;

    //temporary ground check disabling flag

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _capsuleCollider = GetComponent<CapsuleCollider2D>();
    }
    private void FixedUpdate()
    {
        //where is the player at the beginning of the frame
        _lastPosition = _rigidBody.position;

        //The position we want to be in this frame
        _currentPosition = _lastPosition + _moveAmount;

        //move the player to the position we want to be in this frame
        _rigidBody.MovePosition(_currentPosition);

        //zero out the _moveAmount ready for new input next frame
        _moveAmount = Vector2.zero;

        //Only check for ground if the disabled ground check flag is not on.
        if (_disableGroundCheck == false)
        {
            CheckGrounded();
        }
    }

    /// <summary>
    /// Method is called from Update callback.
    /// Because Update() may fire manytimes more than FixedUpdate(),
    /// we use += to accumulate the result of input received during those frames
    /// prior to passing it into the fixed update loop for use by the Rigidbody2D in
    /// the MovePosition() method.
    /// </summary>
    /// <param name="movementInput"></param>
    public void Move(Vector2 movementInput)
    {
        _moveAmount += movementInput;
    }

    private void CheckGrounded()
    {
        //first get the origin position of the middle ray(dead center)
        Vector2 raycastOrigin = _rigidBody.position - new Vector2(0f, _capsuleCollider.size.y * 0.5f);

        //calculate the left and right ray origin positions off the dead center one
        _raycastPositions[0] = raycastOrigin + (Vector2.left * _capsuleCollider.size.x * 0.25f + Vector2.up * 0.1f);
        _raycastPositions[2] = raycastOrigin + (Vector2.right * _capsuleCollider.size.x * 0.25f + Vector2.up * 0.1f);

        //assign the dead center one
        _raycastPositions[1] = raycastOrigin;

        DrawDebugRays(Vector2.down, Color.green);

        //cast three rays, each ray cast tells us if it's hit the ground
        //if so, it adds to a local count variable
        int numberOfGroundHits = 0;

        for (int i = 0; i < _raycastPositions.Length; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(_raycastPositions[i], Vector2.down, raycastDistance, levelGeomLayerMask);

            if (hit.collider)
            {
                //we actually store the hit info because we will later on write code to run dependent on
                //what ground type we hit.
                _raycastHits[i] = hit;

                //increment our counter
                numberOfGroundHits++;
            }
        }

        //at this point, if we have any number of hits, we must have something below us
        if (numberOfGroundHits > 0)
        {
            //most common scenario (90 - 95% of the time the middle ray is hitting a platform)
            if (_raycastHits[1].collider)
            {
                platformType = DetermineGroundType(_raycastHits[1].collider);
            }
            else
            {
                //run a for loop to check for if the left or right rays are hitting a platform
                for(int i = 0; i < _raycastHits.Length; i++)
                {
                    platformType = DetermineGroundType(_raycastHits[i].collider);
                }

            }
            isSomethingBelow = true;
        }
        else
        {
            platformType = PlatformType.None;
            isSomethingBelow = false;
        }

    }


    #region Ground type check
    private PlatformType DetermineGroundType(Collider2D collider)
    {
        if (collider.GetComponent<GroundEffector>())
        {
            return collider.GetComponent<GroundEffector>().platformType;
        }
        else
        {
            //means we are on default ground
            return PlatformType.DefaultPlatform;
        }
    }
    #endregion

    #region Temporary Ground check control
    public void DisableGroundCheck()
    {
        isSomethingBelow = false;
        _disableGroundCheck = true;
        StartCoroutine(EnableGroundCheck());
    }

    IEnumerator EnableGroundCheck()
    {
        yield return new WaitForSeconds(groundCheckTimer);
        _disableGroundCheck = false;
    }

    #endregion

    #region Debug Helper Methods
    private void DrawDebugRays(Vector2 direction, Color color)
    {
        for (int i = 0; i < _raycastPositions.Length; i++)
        {
            Debug.DrawRay(_raycastPositions[i], direction * raycastDistance, color);
        }
    }

    #endregion
}
