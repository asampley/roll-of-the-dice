using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieRotator : MonoBehaviour {
    private float rotateTime = 1.0f;

    private Quaternion current;
    private Quaternion target;
    private float startTime;
    private float duration;

    private Vector3 localUp;
    private Vector3 localForward;
    private Vector3 localRight;

    void Start() {
        this.current = this.transform.rotation;
        this.target = this.transform.rotation;
        this.duration = 0.5f;

        this.localUp = this.current * Vector3.up;
        this.localForward = this.current * Vector3.forward;
        this.localRight = this.current * Vector3.right;
    }

    public void RotateX(int count) {
        Debug.Log("Rotate X");

        var targetInverse = Quaternion.Inverse(target);
        this.target *= Quaternion.FromToRotation(targetInverse * localRight, targetInverse * localForward);

        this.current = this.transform.rotation;
        this.startTime = Time.fixedTime;
    }

    public void RotateY(int count) {
        Debug.Log("Rotate Y");

        var targetInverse = Quaternion.Inverse(target);
        this.target *= Quaternion.FromToRotation(targetInverse * localUp, targetInverse * localForward);

        this.current = this.transform.rotation;
        this.startTime = Time.fixedTime;
    }

    public void Update() {
        rotateTime -= Time.deltaTime;

        if (rotateTime < 0f) {
            if (Random.value < 0.5f) {
                RotateX(1);
            } else {
                RotateY(1);
            }

            rotateTime += 1.0f;
        }

        if (Time.fixedTime < startTime + duration) {
            this.transform.rotation = Quaternion.Slerp(current, target, (Time.fixedTime - startTime) / duration);
        } else {
            this.transform.rotation = target;
        }
    }
}
