using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;

public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private Character character;
    [SerializeField] private new Rigidbody rigidbody;
    [SerializeField] private CharacterMovementParams movementParams;
    [SerializeField] private bool drawDebugLines;

    //private NavMeshPath path;
    private CharacterMovementPath path;

    private bool isCalculatingPath;
    private bool hasReachedDestination;
    private Vector3 goalVelocity;
    private float scaledRadius;
    private Vector3 lookAtPosition;
    private bool useLookAtPosition;
    private bool isRunning;

    private float footstepTimer;

    public bool HasPath { get { return path != null; } }
    public bool IsCalculatingPath { get { return isCalculatingPath; } }
    public Vector3 Destination { get { return HasPath ? path.Destination : Vector3.zero; } }

    void Awake()
    {
        // Adjust radius depending on character size
        scaledRadius = movementParams.Radius * transform.localScale.x;
    }

    void Update()
    {
        if(!character.IsSitting)
        {
            float animSpeed = (rigidbody.velocity.magnitude / GetMaxSpeed()) * (isRunning ? 1.0f : 0.5f);
            character.ModelAnimator.SetFloat("MoveSpeed", animSpeed);
        }

        Vector3 rotationTarget = Vector3.zero;

        // If we want the character to look at a specific target even when they're moving (till, bench etc)
        if (useLookAtPosition)
        {
            rotationTarget = lookAtPosition;
        }
        else if(HasPath && !hasReachedDestination)
        {
            rotationTarget = path.TargetNode;
        }

        if (rotationTarget != Vector3.zero && !character.IsSitting)
        {
            Vector3 forward = rotationTarget - transform.position;
            forward.y = 0;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(forward, Vector3.up), Time.deltaTime * 10);
        }

        if (path != null && !hasReachedDestination)
        {
            if (drawDebugLines)
            {
                path.DrawDebugLines();
                Vector3 debugLineOrigin = transform.position + Vector3.up * 0.02f;
                Debug.DrawLine(debugLineOrigin, debugLineOrigin + rigidbody.velocity, Color.red);
                Debug.DrawLine(debugLineOrigin + rigidbody.velocity, debugLineOrigin + Quaternion.Euler(Vector3.up * 5) * (rigidbody.velocity * 0.9f), Color.red);
                Debug.DrawLine(debugLineOrigin + rigidbody.velocity, debugLineOrigin + Quaternion.Euler(Vector3.up * -5) * (rigidbody.velocity * 0.9f), Color.red);
                Debug.DrawCircle(debugLineOrigin, scaledRadius, 16, new Color(0.3f, 0.3f, 1.0f));
            }

            Vector3 velocity = Vector3.zero;

            if(!character.IsSitting)
            {
                // If we're looking at something while moving then the velocity is just towards the target node
                if (useLookAtPosition)
                {
                    velocity = (path.TargetNode - transform.position).normalized * path.EvaluateSpeed(transform.position, GetMaxSpeed());
                }
                // Otherwise the velocity is in the direction of rotation for more natural movement
                else
                {
                    velocity = transform.forward * path.EvaluateSpeed(transform.position, GetMaxSpeed());
                }
                // Make sure we're not going to float
                velocity.y = 0;
            }

            goalVelocity = velocity;
            rigidbody.velocity = goalVelocity;

            // This isn't working so remove it for now and come back to it
            //rigidbody.AddForce(calculateMovementForce() * Time.deltaTime, ForceMode.VelocityChange);

            // Advance to the next node in the path when reaching it
            if (path.DistToNextWaypoint(transform.position + Vector3.up * 0.03f) < movementParams.DistToReachWaypoint)
            {
                path.NextTargetNode();
            }
        }
        else
        {
            rigidbody.velocity = Vector3.MoveTowards(rigidbody.velocity, Vector3.zero, 2.0f * Time.deltaTime);
        }

        footstepTimer += Time.deltaTime;
        if (footstepTimer > 0.6f)
        {
            if(rigidbody.velocity.magnitude > 0.001f)
            {
                playFootstepSound();
            }
            footstepTimer = 0;
        }
    }

    public void SetDestination(Vector3 pos, bool isRunning = false)
    {
        path = null;
        hasReachedDestination = false;
        isCalculatingPath = true;
        this.isRunning = isRunning;

        // Get the nearest point on the navmesh
        NavMesh.SamplePosition(transform.position, out NavMeshHit hit, Mathf.Infinity, NavMesh.AllAreas);

        // Get rid of any elevation
        Vector3 hitPosition = hit.position;
        hitPosition.y = 0;

        // If the character is outside of the navmesh teleport them to the nearest point
        float distToNavMeshHit = Vector3.Distance(transform.position, hitPosition);
        if (distToNavMeshHit > 0.1f)
        {
            transform.position = hitPosition;
        }

        CharacterPathfinder.Instance.CalculatePath(hitPosition, pos, onCalculatePath);
    }

    public void RecalculateCurrentPath()
    {
        if(path == null)
        {
            return;
        }

        CharacterPathfinder.Instance.CalculatePath(transform.position, path.Destination, onCalculatePath);
    }

    private void onCalculatePath(NavMeshPath pathResult)
    {
        //Debug.Log("Calculated path: " + pathResult.status);
        if (pathResult.status != NavMeshPathStatus.PathInvalid)
        {
            path = new CharacterMovementPath(pathResult.corners, movementParams.AccelerationCurve, movementParams.DecelerationCurve,
                delegate
                {
                },
                delegate
                {
                    //rigidbody.velocity = Vector3.zero;
                    hasReachedDestination = true;
                    path = null;
                }
            );

            path.SetTargetNode(1);
        }
        //else
        //{
        //    HUD.Instance.Notifications.ShowNotification("Couldn't find path!", delegate { CameraController.Instance.SetPositionImmediate(transform.position); });
        //}

        isCalculatingPath = false;
    }

    public void StopMoving()
    {
        path = null;
        isCalculatingPath = false;
    }

    public void LookAt(Vector3 position)
    {
        lookAtPosition = position;
        useLookAtPosition = true;
    }

    public void StopLookingAt()
    {
        useLookAtPosition = false;
    }

    public float GetMaxSpeed()
    {
        return isRunning ? movementParams.RunningSpeed : movementParams.WalkingSpeed;
    }

    // public float RemainingDistance { get { return HasPath ? path.DistToDestination(transform.position) : 0; } }

    public float GetRemainingDistance()
    {
        if (HasPath)
        {
            return path.DistToDestination(transform.position);
        }
        else if(isCalculatingPath)
        {
            return float.MaxValue;
        }
        return 0;
    }

    private float calculateTimeToCollision(CharacterMovement i, CharacterMovement j)
    {
        // Taken from http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter19_Guide_to_Anticipatory_Collision_Avoidance.pdf (page 198)

        float r = i.scaledRadius + j.scaledRadius;
        Vector3 w = j.transform.position - i.transform.position;
        float c = Vector3.Dot(w, w) - r * r;

        if(c < 0)
        {
            // Agents are already colliding
            return 0;
        }

        Vector3 v = i.rigidbody.velocity - j.rigidbody.velocity;
        float a = Vector3.Dot(v, v);
        float b = Vector3.Dot(w, v);

        // The discriminant of the quadratic equation
        float discr = b * b - a * c;

        if(discr <= 0 || a <= 0)
        {
            return Mathf.Infinity;
        }

        float time = (b - Mathf.Sqrt(discr)) / a;

        if(time < 0)
        {
            return Mathf.Infinity;
        }

        return time;
    }

    private Vector3 calculateAvoidanceForce(CharacterMovement i, CharacterMovement j, float timeToCollision, float maxStrength)
    {
        // The furthest out point in time after which we stop considering collisions
        float timeHorizon = movementParams.AvoidanceTimeHorizon;

        if(timeToCollision >= timeHorizon)
        {
            return Vector3.zero;
        }

        Vector3 direction = (i.transform.position + i.rigidbody.velocity * timeToCollision) -
            (j.transform.position + j.rigidbody.velocity * timeToCollision);

        direction.Normalize();

        // A function that smoothly drops the magnitude to zero as the time approaches the time horizon
        float magnitude = (timeHorizon - timeToCollision) / (timeToCollision + 0.001f);

        if(magnitude > maxStrength)
        {
            magnitude = maxStrength;
        }

        return direction * magnitude;
    }

    private Vector3 calculateMovementForce()
    {
        // Find all characters within range
        float sqrRadius = 9.0f;
        List<CharacterMovement> charactersWithinRange = new();
        foreach(Character otherCharacter in Mall.CurrentFloor.Characters)
        {
            if(!otherCharacter || otherCharacter == character) continue;

            Vector3 dir = otherCharacter.transform.position - transform.position;
            if(dir.sqrMagnitude <= sqrRadius)
            {
                charactersWithinRange.Add(otherCharacter.Movement);
            }
        }

        float goalForceStrength = movementParams.AvoidanceForceStrength;
        float maxForceStrength = movementParams.AvoidanceForceStrengthMax;

        // Calculate goal force
        Vector3 force = Vector3.zero;

        foreach(CharacterMovement neighbour in charactersWithinRange)
        {
            float timeToCollision = calculateTimeToCollision(this, neighbour);
            force += calculateAvoidanceForce(this, neighbour, timeToCollision, maxForceStrength);
        }

        // We shouldn't be getting any vertical force, but just in case
        force.y = 0;

        return force;
    }

    void OnDrawGizmos()
    {
        if (HasPath && drawDebugLines)
        {
            path.DrawDebugGizmos();
        }
    }

    private void playFootstepSound()
    {
        AkSoundEngine.PostEvent("footsteps", this.gameObject);
    }
}
