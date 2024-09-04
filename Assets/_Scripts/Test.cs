using _Scripts._Game.Grid.Pathfinders;

using Sirenix.OdinInspector;

using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

using Zenject;

namespace _Scripts
{
    public class Test : MonoBehaviour
    {
        [Inject]
        private Pathfinder _pathfinder;
        
        private float timer;

        private void Start()
        {
            //timer = 0f;
        }

        [Button]
        public void FindPath()
        {
            //_pathfinder.FindPath(new int2(10, 4), new int2(1, 1), new int2(14, 14), 15);
            
            
        }

        private void Update()
        {
            // timer += Time.deltaTime;
            //
            // if (timer > 1f) 
            // {
            //     var startTime = Time.realtimeSinceStartup;
            //     timer = 0f; // reset timer
            //     
            //     var findPathJobCount = 500;
            //     var jobHandleArray = new NativeArray<JobHandle>(findPathJobCount, Allocator.TempJob);
            //
            //     for (var i = 0; i < findPathJobCount; i++)
            //     {
            //         var findPathJob = new FindPathJob
            //         {
            //             StartPosition = new int2(0, 0), EndPosition = new int2(95, 89), GridSize = new int2(100, 100)
            //         };
            //
            //         jobHandleArray[i] = findPathJob.Schedule();
            //     }
            //     
            //     JobHandle.CompleteAll(jobHandleArray);
            //     jobHandleArray.Dispose();
            //
            //     Debug.Log("Time passed since start: " + (Time.realtimeSinceStartup - startTime) * 1000f);
            // }
        }
    }
}