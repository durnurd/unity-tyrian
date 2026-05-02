using UnityEngine;

public class RotateOverTime : MonoBehaviour {
    public Vector3 Axis;
	void Update () {
        transform.Rotate(Axis * Time.deltaTime);
	}
}
