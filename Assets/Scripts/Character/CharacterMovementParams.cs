using UnityEngine;

[CreateAssetMenu(fileName = "CharacterMovementParams", menuName = "Data Assets/CharacterMovementParams", order = 0)]
public class CharacterMovementParams : ScriptableObject
{
    [SerializeField] private float walkingSpeed;
    [SerializeField] private float runningSpeed;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float radius;
    [SerializeField] private float distToReachWaypoint;
    [SerializeField] private AnimationCurve accelerationCurve;
    [SerializeField] private AnimationCurve decelerationCurve;
    [SerializeField] private float avoidanceTimeHorizon;
    [SerializeField] private float avoidanceForceStrength;
    [SerializeField] private float avoidanceForceStrengthMax;

    public float WalkingSpeed { get { return walkingSpeed; } }
    public float RunningSpeed { get { return runningSpeed; } }
    public float TurnSpeed { get { return turnSpeed; } }
    public float Radius { get { return radius; } }
    public float DistToReachWaypoint { get { return distToReachWaypoint; } }
    public AnimationCurve AccelerationCurve { get { return accelerationCurve; } }
    public AnimationCurve DecelerationCurve { get { return decelerationCurve; } }
    public float AvoidanceTimeHorizon { get { return avoidanceTimeHorizon; } }
    public float AvoidanceForceStrength { get { return avoidanceForceStrength; } }
    public float AvoidanceForceStrengthMax { get { return avoidanceForceStrengthMax; } }
}