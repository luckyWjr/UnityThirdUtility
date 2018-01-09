using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace Character
{
    public class AstarAI : MonoBehaviour
    {
        [SerializeField] private Transform target;

        Transform mineTrans;
        Seeker seeker;
        CharacterController cc;

        Path path;

        public float speed = 100;
        public float nextWayPointDistance = 3;

        int currentWayPoint = 0;//要前往的路径点下标

        void Start()
        {
            mineTrans = transform;
            seeker = GetComponent<Seeker>();
            cc = GetComponent<CharacterController>();

            //Start a new path to the targetPosition, return the result to the OnPathComplete function  
            seeker.StartPath(mineTrans.position, target.position, OnPathComplete);
        }

        public void OnPathComplete(Path p)
        {
            Debug.Log("Yay, we got a path back. Did it have an error? " + p.error);
            if (!p.error)
            {
                path = p;
                currentWayPoint = 0;
            }
            //1.Path.vectorPath是一个Vector3的list，保存着每一个路径点的位置。
            //2.Path.path是一个node的list，保存着每一个路径点的node。
        }

        void FixedUpdate()
        {
            if (path == null)
            {
                //We have no path to move after yet  
                return;
            }

            if (currentWayPoint >= path.vectorPath.Count)
            {
                Debug.Log("End Of Path Reached");
                return;
            }

            //Direction to the next waypoint  
            Vector3 dir = (path.vectorPath[currentWayPoint] - mineTrans.position).normalized;
            dir *= speed * Time.fixedDeltaTime;
            cc.SimpleMove(dir);

            //Check if we are close enough to the next waypoint  
            //If we are, proceed to follow the next waypoint  
            if (Vector3.Distance(mineTrans.position, path.vectorPath[currentWayPoint]) < nextWayPointDistance)
            {
                currentWayPoint++;
                return;
            }
        }
    }

}
