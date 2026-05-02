using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ResetToPlayer : MonoBehaviour {

	// Use this for initialization
	void Start () {
        InvokeRepeating("GoBack", 1, 1);
        Invoke("ForceBack", 10);
	}

    float oldEmission;
    void GoBack() {
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        if ((player.position - transform.position).sqrMagnitude > 1000) {
            ForceBack();
        }
    }

    private void ForceBack() {
        name = "trail";
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        Instantiate(gameObject, player.position, player.rotation).GetComponent<NavMeshAgent>().SetDestination(GetComponent<NavMeshAgent>().destination);
        GetComponent<TrailRenderer>().autodestruct = true;
        Destroy(GetComponent<NavMeshAgent>());
        Destroy(this);
    }
}
