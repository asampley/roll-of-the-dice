using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieRotator : MonoBehaviour {
    private Quaternion current;
    private Quaternion target;
    private float startTime;
    private float duration;

    private static Vector3 localUp;
    private static Vector3 localForward;
    private static Vector3 localRight;

    void Awake() {
        this.current = this.transform.localRotation;
        this.target = this.transform.localRotation;

        if (localUp == Vector3.zero) {
            localUp = this.current * Vector3.up;
            localForward = this.current * Vector3.forward;
            localRight = this.current * Vector3.right;
        }
    }

    private void Rotate(Quaternion rotation, int count) {
        if (count < 0) {
            rotation = Quaternion.Inverse(rotation);
        }

        for (int i = 0; i < Math.Abs(count); ++i) {
            target *= rotation;
        }

        this.current = this.transform.localRotation;
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
            this.transform.localRotation = Quaternion.Slerp(current, target, (Time.fixedTime - startTime) / Globals.MOVEMENT_TIME);
        } else {
            this.transform.localRotation = target;
        }
    }
}
