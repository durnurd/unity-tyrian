using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavTarget : MonoBehaviour {
    public NavMeshAgent Agent;
    // Use this for initialization
    void Start() {
        InvokeRepeating("SetTarget", 0, 1);
    }

    void SetTarget() {
        if (transform.hasChanged) {
            Agent.SetDestination(transform.position);
            transform.hasChanged = false;
        }
	}
	
}
