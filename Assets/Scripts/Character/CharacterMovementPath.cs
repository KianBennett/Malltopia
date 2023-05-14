using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class CharacterMovementPath
{
    private Vector3[] nodes;

    private float decelerationDist;
    private float totalPathLength;
    private int targetNode;

    private AnimationCurve accelerationCurve, decelerationCurve;

    private UnityAction<Vector3> onSetTargetNode;
    private UnityAction onReachDestination;

    public Vector3 TargetNode { get { return nodes[targetNode]; } }
    public Vector3 Destination { get { return nodes.Last(); } }

    public CharacterMovementPath(Vector3[] nodes, AnimationCurve accelerationCurve, AnimationCurve decelerationCurve, UnityAction<Vector3> onSetTargetNode, UnityAction onReachDestination)
    {
        this.nodes = nodes;
        this.accelerationCurve = accelerationCurve;
        this.decelerationCurve = decelerationCurve;
        this.onSetTargetNode = onSetTargetNode;
        this.onReachDestination = onReachDestination;

        decelerationDist = decelerationCurve.keys.Last().time - 0.2f;
        calculateTotalPathLength();
    }

    public float EvaluateSpeed(Vector3 position, float maxSpeed)
    {
        float distTravelled = DistFromStart(position);

        if (distTravelled < totalPathLength - decelerationDist)
        {
            return accelerationCurve.Evaluate(distTravelled) * maxSpeed;
        }
        else
        {
            return decelerationCurve.Evaluate(distTravelled - (totalPathLength - decelerationDist)) * maxSpeed;
        }
    }

    public void SetTargetNode(int node)
    {
        //Debug.LogFormat("Set node: {0}/{1}", node, nodes.Count);
        if (node >= nodes.Length)
        {
            onReachDestination();
            return;
        }

        targetNode = node;
        onSetTargetNode(nodes[targetNode]);
    }

    public void NextTargetNode()
    {
        SetTargetNode(targetNode + 1);
    }

    // Distance between character and the most recent node passed 
    public float DistFromLastWaypoint(Vector3 position)
    {
        if (targetNode > 0)
        {
            return Vector3.Distance(position, nodes[targetNode - 1]);
        }
        else
        {
            return nodes[targetNode].magnitude;
        }
    }

    // Distance between character and the target node
    public float DistToNextWaypoint(Vector3 position)
    {
        return Vector3.Distance(position, nodes[targetNode]);
    }

    public float DistToDestination(Vector3 position)
    {
        float dist = DistToNextWaypoint(position);

        if(targetNode == nodes.Length - 1)
        {
            return dist;
        }

        for(int i = targetNode; i < nodes.Length - 1; i++)
        {
            dist += Vector3.Distance(nodes[i], nodes[i + 1]);
        }

        return dist;
    }

    public float DistFromStart(Vector3 position)
    {
        if(targetNode <= 1)
        {
            return DistFromLastWaypoint(position);
        }

        float dist = 0;

        for(int i = 0; i < targetNode - 1; i++)
        {
            dist += Vector3.Distance(nodes[i], nodes[i + 1]);
        }

        dist += DistFromLastWaypoint(position);

        return dist;
    }

    private void calculateTotalPathLength()
    {
        // Add up square magnitudes so we only need to do one square root operation at the end
        float sqrPathLength = 0;

        foreach(Vector3 node in nodes)
        {
            sqrPathLength += node.sqrMagnitude;
        }

        totalPathLength = Mathf.Sqrt(sqrPathLength);
    }

    public void DrawDebugLines()
    {
        for (int i = targetNode - 1; i < nodes.Length - 1; i++)
        {
            Debug.DrawLine(nodes[i], nodes[i + 1], new Color(1, 1, 1, 0.25f));
        }
    }

    public void DrawDebugGizmos()
    {
        for (int i = targetNode - 1; i < nodes.Length; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(nodes[i], 0.05f);
        }
    }
}
