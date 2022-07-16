using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieRotator : MonoBehaviour {
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

        this.localUp = this.current * Vector3.up;
        this.localForward = this.current * Vector3.forward;
        this.localRight = this.current * Vector3.right;
    }

    private void Rotate(Quaternion rotation, int count) {
        if (count < 0) {
            rotation = Quaternion.Inverse(rotation);
        }

        for (int i = 0; i < Math.Abs(count); ++i) {
            target *= rotation;
        }

        this.current = this.transform.rotation;
        this.startTime = Time.fixedTime;
    }

    public void RotateX(int count) {
        var targetInverse = Quaternion.Inverse(target);
        var rotation = Quaternion.FromToRotation(targetInverse * localForward, targetInverse * localRight);

        Rotate(rotation, count);
    }

    public void RotateY(int count) {
        var targetInverse = Quaternion.Inverse(target);
        var rotation = Quaternion.FromToRotation(targetInverse * localForward, targetInverse * localUp);

        Rotate(rotation, count);
    }

    public void Update() {
        if (Time.fixedTime < startTime + Globals.MOVEMENT_TIME) {
            this.transform.rotation = Quaternion.Slerp(current, target, (Time.fixedTime - startTime) / Globals.MOVEMENT_TIME);
        } else {
            this.transform.rotation = target;
        }
    }
}
