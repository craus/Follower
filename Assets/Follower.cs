using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mathematics;
using SparseCollections;
using System;
using System.Linq;

[ExecuteInEditMode]
public class Follower : MonoBehaviour {

    public TrajectoryPoint trajectoryPointPrefab;
    float radius = 0.5f;
    public Transform trajectoryTransform;
    public RectTransform trajectoryRect;
    public List<TrajectoryPoint> trajectory;
    public float time;
    public float period;
    public float speed;

    public SparseArray<int, double> xApproximation, yApproximation;

    public float ApproximationFunction(float t, int id, bool y) {
        //t = Extensions.Modulo(t, period);
        
        //return Mathf.Sin(2 * Mathf.PI * (id * 2 - 1) * t / period + id);

        if (id == 0) {
            return 1;
        }
        if (id % 2 == 0) {
            return Mathf.Sin(Mathf.PI * 2 * ((1 + (id - 1) / 2) * t / period));
        } else {
            return Mathf.Cos(Mathf.PI * 2 * ((1 + (id - 1) / 2) * t / period));
        }
        //return Mathf.Cos(Mathf.PI * 2 * (t / period * id));
    }

    public Vector2 Position(float t) {
        Vector2 result = Vector2.zero;
        for (int i = 0; i < trajectory.Count; i++) {
            result += new Vector2((float)(ApproximationFunction(t, i, false) * xApproximation[i]), (float)(ApproximationFunction(t, i, true) * yApproximation[i]));
        }
        return result;
    }

    public Vector2 Velocity(float t) {
        float dt = 1e-4f;
        return (Position(t + dt) - Position(t)) / dt;
    }

    public Vector2 SpookyPosition() {
        float[] prefixProducts = new float[trajectory.Count];
        prefixProducts[0] = Extensions.CyclicDiff(time, trajectory[0].time, period);
        for (int i = 1; i < trajectory.Count; i++) {
            prefixProducts[i] = prefixProducts[i - 1] * Extensions.CyclicDiff(time, trajectory[i].time, period);
        }

        float[] suffixProducts = new float[trajectory.Count];
        suffixProducts[trajectory.Count - 1] = Extensions.CyclicDiff(time, trajectory[trajectory.Count - 1].time, period);
        for (int i = trajectory.Count-2; i >= 0; i--) {
            suffixProducts[i] = suffixProducts[i + 1] * Extensions.CyclicDiff(time, trajectory[i].time, period);
        }

        float totalWeight = 0;
        Vector3 totalPosition = Vector2.zero;
        for (int i = 0; i < trajectory.Count; i++) {
            float weight = 1;
            if (i > 0) {
                weight *= prefixProducts[i-1];
            }
            if (i < trajectory.Count-1) {
                weight *= suffixProducts[i+1];
            }
            totalPosition += weight * trajectory[i].transform.position;
            totalWeight += weight;
        }
        return totalPosition / totalWeight;
    }

    void Update() {
        if (Extensions.Editor()) {
            RecalculateTrajectory();
            RecalculatePosition();
        } else {
            time = Time.time;
            RecalculatePosition();
        }
	}

    void Start() {
    }

    [ContextMenu("Recalculate Position")]
    void RecalculatePosition() {
        transform.position = Position(time);
    }

    [ContextMenu("Recalculate Trajectory")]
    void RecalculateTrajectory() {
        trajectory = trajectoryTransform.GetComponentsInChildren<TrajectoryPoint>().ToList();
        xApproximation = new SparseArray<int, double>();
        yApproximation = new SparseArray<int, double>();
        var mx = new SparseCollections.Sparse2DMatrix<int, int, double>();
        var my = new SparseCollections.Sparse2DMatrix<int, int, double>();
        var xa = new SparseCollections.SparseArray<int, double>();
        var ya = new SparseCollections.SparseArray<int, double>();
        for (int i = 0; i < trajectory.Count; i++) {
            for (int j = 0; j < trajectory.Count; j++) {
                mx[i, j] = ApproximationFunction(trajectory[i].time, j, false);
                my[i, j] = ApproximationFunction(trajectory[i].time, j, true);
            }
            xa[i] = trajectory[i].transform.position.x;
            ya[i] = trajectory[i].transform.position.y;
        }

        LinearEquationSolver.Solve(trajectory.Count, mx, xa, xApproximation);
        LinearEquationSolver.Solve(trajectory.Count, my, ya, yApproximation);

        //for (int i = 0; i < trajectory.Count; i++) {
        //    Debug.LogFormat("xApproximation[{0}] = {1}", i, xApproximation[i]);
        //}
        //for (int i = 0; i < trajectory.Count; i++) {
        //    Debug.LogFormat("yApproximation[{0}] = {1}", i, yApproximation[i]);
        //}
    }

    [ContextMenu("Test")]
    void Test() {
        var mx = new SparseCollections.Sparse2DMatrix<int,int,double>();
        mx[0,0] = 2;
        mx[0,1] = 1;
        mx[1,0] = 6;
        mx[1,1] = -1;
        var a = new SparseCollections.SparseArray<int, double>();
        a[0] = 11;
        a[1] = 13;
        var x = new SparseCollections.SparseArray<int, double>();
        LinearEquationSolver.Solve(2, mx, a, x);
        Debug.LogFormat("x[0] = {0}", x[0]);
        Debug.LogFormat("x[1] = {0}", x[1]);
    }

    [ContextMenu("Fit to screen")]
    void FitToScreen() {
        RecalculateTrajectory();
        Vector2 minPosition = Position(0);
        Vector2 maxPosition = Position(0);
        for (int i = 0; i < 1000; i++) {
            Vector2 position = Position(period * i / 1000);
            minPosition = Vector2.Min(minPosition, position);
            maxPosition = Vector2.Max(maxPosition, position);
        }
        var corners = new Vector3[4];
        Vector2 a = Camera.main.ScreenToWorldPoint(trajectoryRect.TransformPoint(trajectoryRect.rect.min));
        Vector2 b = Camera.main.ScreenToWorldPoint(trajectoryRect.TransformPoint(trajectoryRect.rect.max));
        Vector2 desiredMinPosition = Vector2.Min(a, b);
        Vector2 desiredMaxPosition = Vector2.Max(a, b);
        desiredMinPosition += Vector2.one * radius;
        desiredMaxPosition -= Vector2.one * radius;
        Vector2 desiredSize = desiredMaxPosition - desiredMinPosition;
        desiredSize = Vector2.one * Mathf.Min(desiredSize.x, desiredSize.y);
        Vector2 zoom = desiredSize.Scaled((maxPosition - minPosition).Inverse());
        zoom = Vector2.one * Mathf.Min(zoom.x, zoom.y);
        Vector2 shift = (desiredMinPosition+desiredMaxPosition) - (minPosition+maxPosition).Scaled(zoom);
        shift /= 2;
        trajectory.ForEach(tp => tp.transform.position = tp.transform.position.xy().Scaled(zoom) + shift);
        RecalculateTrajectory();
    }

    public void NormalizeSpeed() {
        RecalculateTrajectory();
        float distance = 0;
        for (int i = 0; i < 1000; i++) {
            Vector2 position1 = Position(period * i / 1000);
            Vector2 position2 = Position(period * (i+1) / 1000);
            distance += (position2 - position1).magnitude;
        }
        float speed = distance / period;
        float timeZoom = speed / this.speed;
        period *= timeZoom;
        for (int i = 0; i < trajectory.Count; i++) {
            trajectory[i].time *= timeZoom;
        }
    }

    [ContextMenu("Random trajectory")]
    void RandomTrajectory7() {
        RandomTrajectory(7);
    }

    public void RandomTrajectory(float cnt) {
        int lowCnt = (int)cnt;
        int highCnt = lowCnt + 1;
        float upgradePart = cnt - lowCnt;
        upgradePart = 0;
        period = lowCnt;
        trajectoryTransform.Children().ForEach(c => c.Destroy());
        for (int i = 0; i < lowCnt; i++) {
            var tp = Instantiate(trajectoryPointPrefab);
            tp.transform.SetParent(trajectoryTransform);
            tp.transform.position = UnityEngine.Random.insideUnitCircle * 100;
            tp.time = period * i / lowCnt + period / lowCnt * 0.2f * UnityEngine.Random.Range(-1f, 1f);
        }
        RecalculateTrajectory();
        ////trajectoryTransform.Children().ForEach(c => c.Destroy());
        ////for (int i = 0; i < highCnt; i++) {
        ////    var tp = Instantiate(trajectoryPointPrefab);
        ////    tp.transform.SetParent(trajectoryTransform);
        ////    tp.time = period * i / highCnt + period / highCnt * 0.0f * UnityEngine.Random.Range(-1f, 1f);
        ////    tp.transform.position = Vector2.Lerp(Position(tp.time), UnityEngine.Random.insideUnitCircle * 100, upgradePart);
        ////    tp.transform.position = Position(tp.time);
        ////    Debug.LogFormat("position = {0}", tp.transform.position);
        ////}
        //period = highCnt;

        FitToScreen();
        NormalizeSpeed();
    }
}
