using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class CharacterPathfinder : Singleton<CharacterPathfinder>
{
    private struct CharacterPathData
    {
        public Vector3 start;
        public Vector3 destination;
        public UnityAction<NavMeshPath> onComplete;
    }

    [SerializeField] private long yieldThresholdMilliseconds;

    private List<CharacterPathData> pathData;


    protected override void Awake()
    {
        pathData = new();

        StartCoroutine(calculateCharacterPaths());
    }

    private IEnumerator calculateCharacterPaths()
    {
        Stopwatch stopwatch = new();

        while (true)
        {
            yield return new WaitUntil(() =>
            {
                return pathData.Count > 0;
            });

            stopwatch.Start();

            for(int i = pathData.Count - 1; i >= 0; i--)
            {
                CharacterPathData data = pathData[i];

                NavMeshPath navMeshPath = new();
                NavMesh.CalculatePath(data.start, data.destination, NavMesh.AllAreas, navMeshPath);

                data.onComplete(navMeshPath);

                pathData.Remove(data);

                if (stopwatch.ElapsedMilliseconds >= yieldThresholdMilliseconds)
                {
                    //Debug.LogFormat("Yielded to the next frame after {0}ms", stopwatch.ElapsedMilliseconds);
                    stopwatch.Reset();
                    yield return null;
                }
            }
        }
    }

    public void CalculatePath(Vector3 start, Vector3 destination, UnityAction<NavMeshPath> onComplete)
    {
        pathData.Add(new CharacterPathData()
        {
            start = start,
            destination = destination,
            onComplete = onComplete
        });
    }
}
