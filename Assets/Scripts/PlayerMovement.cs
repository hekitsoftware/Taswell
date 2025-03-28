using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References <3")]
    public PlayerMoveStats MoveStats;
    [SerializeField] private Collider2D _feetColl;
    [SerializeField] private Collider2D _bodyColl;

    [Header("Animation")]
    [SerializeField] Animator animator;
    private string currentState;

    private Rigidbody2D _rb;

    // Movement variables
    public float _moveVelocity { get; private set; }
    public bool _isFacingRight;

    // Collision check variables
    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;

    // Jump variables
    public float VerticalVelocity { get; private set; }
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private int _numberOfJumpsUsed;

    // Apex variables
    private float _apexPoint;
    private bool _isPastApexThreshold;

    // Jump buffer and coyote time
    private float _jumpBufferTimer;
    private bool _jumpReleasedDuringBuffer;
    private float _coyoteTimer;

    //------------------------------------------
    // Animation States:
    const string PLAYER_IDLE = "T_idle";
    const string PLAYER_RUN = "T_Run";
    const string PLAYER_JUMP = "T_Jump";

    private void Awake()
    {
        _isFacingRight = true;
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        ChangeAnimationState(PLAYER_IDLE);
    }

    private void Update()
    {
        JumpChecks();
        LandCheck();
        CountTimers();

        if (!_isGrounded) { ChangeAnimationState(PLAYER_JUMP); }
    }

    private void FixedUpdate()
    {
        Jump();
        CollisionChecks();

        if (_isGrounded)
        {
            Move(MoveStats.GroundAcc, MoveStats.GroundDec, InputManager.Movement);
        }
        else
        {
            Move(MoveStats.AirAcc, MoveStats.AirDec, InputManager.Movement);
        }

        // Emergency downward force
        if (!_isGrounded && _rb.linearVelocity.y == 0)
        {
            VerticalVelocity = VerticalVelocity - 1;
        }
    }

    #region Movement

    private void Move(float acc, float dec, Vector2 moveInput)
    {
        TurnCheck(moveInput);

        if (moveInput != Vector2.zero)
        {
            float targetVelocity = moveInput.x * MoveStats.MaxMoveSpeed;
            _moveVelocity = Mathf.Lerp(_moveVelocity, targetVelocity, acc * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(_moveVelocity, _rb.linearVelocity.y);
            if (_isGrounded) { ChangeAnimationState(PLAYER_RUN); }
        }
        else
        {
            _moveVelocity = Mathf.Lerp(_moveVelocity, 0f, dec * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(_moveVelocity, _rb.linearVelocity.y);
            if (_isGrounded) { ChangeAnimationState(PLAYER_IDLE); }
        }
    }

    private void TurnCheck(Vector2 moveInput)
    {
        if (_isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        }
        else if (!_isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        if (turnRight)
        {
            _isFacingRight = true;
            transform.Rotate(0f, 180f, 0f);
        }
        else
        {
            _isFacingRight = false;
            transform.Rotate(0f, -180f, 0f);
        }
    }

    #endregion

    #region Jumping

    private void JumpChecks()
    {
        if (InputManager.jumpWasPressed)
        {
            _jumpBufferTimer = MoveStats.JumpBufferTime;
            _jumpReleasedDuringBuffer = false;
        }

        if (InputManager.jumpWasReleased)
        {
            if (_jumpBufferTimer > 0)
            {
                _jumpReleasedDuringBuffer = true;
            }

            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = true;
                    _isFastFalling = true;
                    _fastFallTime = MoveStats.TimeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }

        if (_jumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {
            InitJump(1);
            if (_jumpReleasedDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticalVelocity;
            }
        }
        else if (_jumpBufferTimer > 0f && _isJumping && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed)
        {
            _isFastFalling = false;
            InitJump(1);
        }
        else if (_jumpBufferTimer > 0f && _isFalling && _numberOfJumpsUsed < MoveStats.NumberOfJumpsAllowed - 1)
        {
            InitJump(2);
            _isFastFalling = false;
        }
    }

    private void InitJump(int numberOfJumpsUsed)
    {
        if (!_isJumping)
        {
            _isJumping = true;
        }
        _jumpBufferTimer = 0f;
        _numberOfJumpsUsed += numberOfJumpsUsed;
        VerticalVelocity = MoveStats.InitialJumpVelocity;
    }

    private void Jump()
    {
        if (_isJumping)
        {
            if (VerticalVelocity >= 0f)
            {
                _apexPoint = Mathf.InverseLerp(MoveStats.InitialJumpVelocity, 0f, VerticalVelocity);

                if (_apexPoint > MoveStats.ApexThreshold)
                {
                    if (!_isPastApexThreshold)
                    {
                        _isPastApexThreshold = true;
                    }
                    if (_isPastApexThreshold)
                    {
                        VerticalVelocity = 0f;
                    }
                }
                else if (!_isFastFalling)
                {
                    VerticalVelocity += MoveStats.Gravity * Time.deltaTime;
                }
            }
            else if (!_isFastFalling)
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMulti * Time.deltaTime;
            }

            if (_isFastFalling)
            {
                if (_fastFallTime >= MoveStats.TimeForUpwardsCancel)
                {
                    VerticalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMulti * Time.deltaTime;
                }
                else
                {
                    VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / MoveStats.TimeForUpwardsCancel));
                }
                _fastFallTime += Time.fixedDeltaTime;
            }
        }

        if (!_isGrounded && !_isJumping)
        {
            if (!_isFalling)
            {
                _isFalling = true;
            }
            VerticalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
        }

        VerticalVelocity = Mathf.Clamp(VerticalVelocity, -MoveStats.MaxFallSpeed, 50f);
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, VerticalVelocity);
    }

    #endregion

    #region LandCheck
    private void LandCheck()
    {
        if ((_isJumping || _isFalling) && _isGrounded && VerticalVelocity <= 0f)
        {
            // Reset all jumping-related states
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;

            // Reset jump count after landing
            _numberOfJumpsUsed = 0;

            // Reset vertical velocity to a small negative number (to stick the player to the ground)
            VerticalVelocity = Physics2D.gravity.y;

            // Ensure that landing puts the character in idle state
            if (_moveVelocity == 0)
            {
                ChangeAnimationState(PLAYER_IDLE);
            }
        }
    }

    #endregion

    #region Collision Checks

    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x, MoveStats.GroundDetectRayLength);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectRayLength, MoveStats.groundLayer);
        _isGrounded = _groundHit.collider != null;
    }

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _bodyColl.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x * MoveStats.HeadWidth, MoveStats.HeadDetectRayLength);

        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, MoveStats.HeadDetectRayLength, MoveStats.groundLayer);
        _bumpedHead = _headHit.collider != null;
    }

    private void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
    }

    #endregion

    #region Timers

    private void CountTimers()
    {
        if (_jumpBufferTimer > 0)
        {
            _jumpBufferTimer -= Time.fixedDeltaTime;
        }
        if (_coyoteTimer > 0)
        {
            _coyoteTimer -= Time.fixedDeltaTime;
        }
    }

    #endregion

    #region Animation

    void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;

        animator.Play(newState);

        currentState = newState;
    }

    #endregion
}
