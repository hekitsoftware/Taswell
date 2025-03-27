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

    // Dash
    private Vector2 _dashDirection;
    public bool _isAirDashing;
    public bool _isDashing;
    private bool _isDashFastFalling;

    private float _dashOnGroundTimer;
    private float _dashOffGroundTimer;
    private float _dashFastFallTime;
    private float _dashFallReleaseSpeed;

    private int _numberOfDashesUsed;

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
    }

    private void Update()
    {
        DashCheck();
        JumpChecks();
        LandCheck();
        CountTimers();

        if (!_isGrounded) { ChangeAnimationState(PLAYER_JUMP); }
    }

    private void FixedUpdate()
    {
        Dash();
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
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;
            _numberOfJumpsUsed = 0;
            VerticalVelocity = Physics2D.gravity.y;
        }
    }

    #endregion

    #region Dash

    private void DashCheck()
    {
        if (InputManager.DashWasPressed)
        {
            // Dash when grounded
            if (_isGrounded && _dashOnGroundTimer <= 0 && !_isDashing)
            {
                InitDash();
            }
            // Air dash: only if dashes are available and not already dashing
            else if (!_isGrounded && !_isDashing && _numberOfDashesUsed < MoveStats.NumberOfDashes)
            {
                _isAirDashing = true;
                InitDash();
            }
        }
    }

    private void InitDash()
    {
        _dashDirection = InputManager.Movement;

        // Determine closest dash direction based on input
        Vector2 closestDirection = Vector2.zero;
        float minDistance = Vector2.Distance(_dashDirection, MoveStats.DashDirections[0]);

        for (int i = 0; i < MoveStats.DashDirections.Length; i++)
        {
            if (_dashDirection == MoveStats.DashDirections[i])
            {
                closestDirection = _dashDirection;
                break;
            }

            float distance = Vector2.Distance(_dashDirection, MoveStats.DashDirections[i]);

            // Handle diagonal bias
            bool isDiagonal = (Mathf.Abs(MoveStats.DashDirections[i].x) == 1 && Mathf.Abs(MoveStats.DashDirections[i].y) == 1);
            if (isDiagonal)
            {
                distance -= MoveStats.DashDiagonallyBias;
            }

            if (distance < minDistance)
            {
                minDistance = distance;
                closestDirection = MoveStats.DashDirections[i];
            }
        }

        // Default dash direction if no input
        if (closestDirection == Vector2.zero)
        {
            closestDirection = _isFacingRight ? Vector2.right : Vector2.left;
        }

        _dashDirection = closestDirection;
        _numberOfDashesUsed++;

        // Start dashing
        _isDashing = true;
        _dashOffGroundTimer = 0f;
        _dashOnGroundTimer = MoveStats.GroundDashTime;

        ResetDash();
    }

    private void Dash()
    {
        if (_isDashing)
        {
            _dashOffGroundTimer += Time.fixedDeltaTime;

            if (_dashOffGroundTimer >= MoveStats.DashTime)
            {
                if (_isGrounded)
                {
                    ResetDashes();
                }

                _isAirDashing = false;
                _isDashing = false;

                if (!_isJumping)
                {
                    _dashFastFallTime = 0f;
                    _dashFallReleaseSpeed = VerticalVelocity;

                    if (!_isGrounded)
                    {
                        _isDashFastFalling = true;
                    }
                }

                return;
            }

            _moveVelocity = MoveStats.DashSpeed * _dashDirection.x;

            if (_dashDirection.y != 0f || _isAirDashing)
            {
                VerticalVelocity = MoveStats.DashSpeed * _dashDirection.y;
            }
        }

        else if (_isDashFastFalling)
        {
            if (VerticalVelocity > 0f)
            {
                if (_dashFastFallTime < MoveStats.DashTimeForUpwardsCancel)
                {
                    VerticalVelocity = Mathf.Lerp(_dashFallReleaseSpeed, 0f, (_dashFastFallTime / MoveStats.DashTimeForUpwardsCancel));
                }
                else if (_dashFastFallTime >= MoveStats.DashTimeForUpwardsCancel)
                {
                    VerticalVelocity += MoveStats.Gravity * MoveStats.DashGravityOnReleaseMulti * Time.fixedDeltaTime;
                }

                _dashFastFallTime += Time.fixedDeltaTime;
            }
            else
            {
                VerticalVelocity += MoveStats.Gravity * MoveStats.DashGravityOnReleaseMulti * Time.fixedDeltaTime;
            }
        }
    }

    private void ResetDash()
    {
        _isDashFastFalling = false;
        _dashOnGroundTimer = -0.01f;
    }

    private void ResetDashes()
    {
        _numberOfDashesUsed = 0;
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

        if (_dashOnGroundTimer > 0)
        {
            _dashOnGroundTimer -= Time.fixedDeltaTime;
        }

        if (_dashOffGroundTimer > 0)
        {
            _dashOffGroundTimer -= Time.fixedDeltaTime;
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
