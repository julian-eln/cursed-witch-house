using UnityEngine;
using UnityEngine.AI;

public class pathFinding : MonoBehaviour
{
    public Transform Target;
    public NavMeshAgent Agent;
    public float range; //radius of sphere
    public bool pointOfInt;
    public detectionradius Detectionradius;
    public bool chase;
    public Transform centrePoint; //centre of the area the agent wants to move around in
    //instead of centrePoint you can set it as the transform of the agent if you don't care about a specific area

    // Update is called once per frame
    void Update()
    {
        if(chase==true)
        {
            Agent.SetDestination(Target.position);
            Target = Detectionradius.Target;
        }
        else if (pointOfInt == true)
        {
            Agent.SetDestination(Target.position);
            Target = Detectionradius.Target;
        }
        else if (Agent.remainingDistance <= Agent.stoppingDistance) //done with path
        {
            Vector3 point;
            if (RandomPoint(centrePoint.position, range, out point)) //pass in our centre point and radius of area
            {
                Debug.DrawRay(point, Vector3.up, Color.blue, 1.0f); //so you can see with gizmos
                Agent.SetDestination(point);
            }
        }
        
    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {

        Vector3 randomPoint = center + Random.insideUnitSphere * range; //random point in a sphere 
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas)) //documentation: https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html
        {
            //the 1.0f is the max distance from the random point to a point on the navmesh, might want to increase if range is big
            //or add a for loop like in the documentation
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }
 

}

