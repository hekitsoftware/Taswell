using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[CreateAssetMenu(menuName = "Assets/Create/MyFramework/MoveStats")]
public class PlayerMoveStats : ScriptableObject
{
    [Header("Walking")]
    [Range(1f, 100f)] public float MaxMoveSpeed = 20.5f;
    [Range(0.25f, 50f)] public float GroundAcc = 5f;
    [Range(0.25f, 50f)] public float GroundDec = 20f;
    [Range(0.25f, 50f)] public float AirAcc = 5f;
    [Range(0.25f, 50f)] public float AirDec = 5f;

    [Header("Ground/Collision Check")]
    public LayerMask groundLayer;
    public float GroundDetectRayLength = 0.02f;
    public float HeadDetectRayLength = 0.02f;
    [Range(0f, 1f)] public float HeadWidth = 0.75f;

    [Header("Jump")]
    public float JumpHeight = 6.0f;  // Slightly reduced jump height for a snappier feel
    [Range(1f, 1.1f)] public float JumpHeightCompensationFactor = 1.05f; // Adjusted factor for more precise jumps
    public float TimeTillJumpApex = 0.3f;  // Quicker jump arc
    [Range(0.01f, 5)] public float GravityOnReleaseMulti = 1.1f;  // Slightly faster fall after releasing jump
    public float MaxFallSpeed = 26f;  // Reasonable max fall speed
    [Range(1f, 5)] public int NumberOfJumpsAllowed = 2; // 2 jumps allowed, as per the default

    [Header("Jump Cut")]
    [Range(0.02f, 0.3f)] public float TimeForUpwardsCancel = 0.027f;

    [Header("Jump Apex")]
    [Range(0.5f, 1f)] public float ApexThreshold = 0.97f;  // Remains the same, could adjust based on desired feel
    [Range(0.01f, 1f)] public float ApexHangTime = 0.075f;  // Small hang time to prevent "floating"

    [Header("Jump Cut")]
    [Range(0f, 1f)] public float JumpBufferTime = 0.15f;  // Buffer time for jump input

    [Header("Jump Cut")]
    [Range(0f, 1f)] public float JumpCoyoteTime = 0.15f;  // Extended coyote time to ensure jumps are more forgiving

    [Header("Jump Visualisation Tool")]
    public bool ShowJumpArc = false;
    public bool StopOnCollision = true;
    public bool DrawRight = true;
    [Range(5f, 100f)] public float ArcResolution = 20;
    [Range(0f, 500f)] public float VisualizationSteps = 90;

    [Header("Dash")]
    [Range(0f, 1f)] public float DashTime = 0.11f;
    [Range(1f, 200f)] public float DashSpeed = 40f;
    [Range(0f, 1f)] public float GroundDashTime = 0.225f;
    [Range(0, 5)] public float NumberOfDashes = 2f;
    [Range(0f, 0.5f)] public float DashDiagonallyBias = 0.04f;

    [Header("Dash Cancel Time")]
    [Range(0.01f, 5f)] public float DashGravityOnReleaseMulti = 1f;
    [Range(0.02f, 0.3f)] public float DashTimeForUpwardsCancel = 0.027f;

    public readonly Vector2[] DashDirections = new Vector2[]
    {
        new Vector2(0, 0), //Nothing
        new Vector2(1, 0), //Right
        new Vector2(1, 1).normalized, //T-Right
        new Vector2(0, 1), //Up
        new Vector2(-1, 1).normalized, //T-Left
        new Vector2(-1, 0), //Left
        new Vector2(-1, -1).normalized, //B-Left
        new Vector2(0, -1), //Down
        new Vector2(1, -1).normalized //B-Right
    };

    public float Gravity { get; private set; }
    public float InitialJumpVelocity { get; private set; }
    public float AdjustedJumpHeight { get; private set; }

    private void OnValidate()
    {
        CalculateV();
    }

    private void OnEnable()
    {
        CalculateV();
    }

    private void CalculateV()
    {
        AdjustedJumpHeight = JumpHeight * JumpHeightCompensationFactor;
        Gravity = -(2f * AdjustedJumpHeight) / MathF.Pow(TimeTillJumpApex, 2f);
        InitialJumpVelocity = MathF.Abs(Gravity) * TimeTillJumpApex;
    }
}
