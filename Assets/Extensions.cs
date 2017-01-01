using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Linq;

public static class Extensions {

    public static float Modulo(float x, float y) {
        return (x % y + y) % y;
    }

    public static float CyclicDiff(float x, float y, float period) {
        var result = Mathf.Min(Modulo(x - y, period), Modulo(y - x, period));
        return result;
    }

    public static int modulo(this int x, int y) {
        return (x % y + y) % y;
    }

    public static T cyclic<T>(this List<T> list, int index) {
        return list[modulo(index, list.Count)];
    }

    public static T cyclicNext<T>(this List<T> list, T element, int delta = 1) {
        return list.cyclic(list.IndexOf(element) + delta);
    }

    public static string Path(this GameObject obj) {
        string path = "/" + obj.name;
        while (obj.transform.parent != null) {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
    }

    public static Vector2 xy(this Vector3 v) {
        return new Vector2(v.x, v.y);
    }

    public static Vector2 xy(this Vector4 v) {
        return new Vector2(v.x, v.y);
    }

    public static int ExtMin<T>(this IEnumerable<T> collection, Func<T, int> criteria) {
        if (collection.Count() == 0) {
            return int.MaxValue;
        }
        return collection.Min(criteria);
    }

    public static int ExtMax<T>(this IEnumerable<T> collection, Func<T, int> criteria) {
        if (collection.Count() == 0) {
            return int.MinValue;
        }
        return collection.Max(criteria);
    }

    public static float ExtMin<T>(this IEnumerable<T> collection, Func<T, float> criteria) {
        if (collection.Count() == 0) {
            return float.PositiveInfinity;
        }
        return collection.Min(criteria);
    }

    public static float ExtMax<T>(this IEnumerable<T> collection, Func<T, float> criteria) {
        if (collection.Count() == 0) {
            return float.NegativeInfinity;
        }
        return collection.Max(criteria);
    }

    public static string ExtToString<T>(this IEnumerable<T> collection, string delimiter = ", ", string format = "[{0}]", Func<T, string> elementToString = null) {
        elementToString = elementToString ?? (obj => obj.ToString());
        return String.Format(format, String.Join(delimiter, collection.Select(obj => obj != null ? elementToString(obj) : "null").ToArray()));
    }

    public static T CyclicNext<T>(this List<T> list, T obj, int delta = 1) {
        return list[((list.IndexOf(obj) + delta) % list.Count + list.Count) % list.Count];
    }

    public static void ChangeAlpha(this Material material, float alpha) {
        Color c = material.color;
        c.a = alpha;
        material.color = c;
    }

    public static List<T> GetComponentsInDirectChildren<T>(this Component component) {
        List<T> result = new List<T>();
        foreach (Transform t in component.transform) {
            result = result.Concat(t.GetComponents<T>().ToList()).ToList();
        }
        return result;
    }

    public static Vector3 Change(this Vector3 v, float x = float.NaN, float y = float.NaN, float z = float.NaN) {
        return new Vector3(
            float.IsNaN(x) ? v.x : x,
            float.IsNaN(y) ? v.y : y,
            float.IsNaN(z) ? v.z : z
        );
    }

    public static float NormalizeAngle(float angle) {
        while (angle < -180) {
            angle += 360;
        }
        while (angle > 180) {
            angle -= 360;
        }
        return angle;
    }

    public static Vector3 NormalizeAngles(Vector3 angles) {
        return new Vector3(NormalizeAngle(angles.x), NormalizeAngle(angles.y), NormalizeAngle(angles.z));
    }

    public static string ExtToString(this Vector3 v) {
        return String.Format("({0:0.####}, {1:0.####}, {2:0.####})", v.x, v.y, v.z);
    }

    public static string ExtToString(this Vector2 v) {
        return String.Format("({0:0.####}, {1:0.####})", v.x, v.y);
    }

    public static List<T> ShallowClone<T>(this List<T> listToClone) {
        if (listToClone == null) {
            return null;
        }
        return listToClone.Select(item => item).ToList();
    }

    public static string Path(this Transform transform) {
        string path = transform.name;
        while (transform.parent != null) {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }

    public static void Reset(this Transform transform) {
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
    }

    public static T Rand<T>(this T[,] matrix) {
        return matrix[UnityEngine.Random.Range(0, matrix.GetLength(0)), UnityEngine.Random.Range(0, matrix.GetLength(1))];
    }

    public static string ExtToString(this Transform t) {
        return String.Format("{{position = {0}, rotation = {1}, lossyScale = {2}}}", t.position.ExtToString(), t.rotation, t.lossyScale.ExtToString());
    }

    public static string ExtToString(this Ray ray) {
        return String.Format("{{origin = {0}, direction = {1}}}", ray.origin.ExtToString(), ray.direction.ExtToString());
    }

    static Matrix4x4 x;
    static Matrix4x4 y;


    static bool CloseMatrix(Transform a, Transform b) {
        x = a.localToWorldMatrix;
        y = b.localToWorldMatrix;
        for (int i = 0; i < 16; i++) {
            if (Mathf.Abs(x[i] - y[i]) > 0.01f) {
                return false;
            }
        }
        return true;
        //return a.localToWorldMatrix == b.localToWorldMatrix;
    }

    static bool CloseFourVectors(Transform a, Transform b) {
        return ClosePosition(a, b) && CloseUp(a, b) && CloseForward(a, b) && CloseScale(a, b);
    }

    public static bool Close(Transform a, Transform b) {
        return CloseFourVectors(a, b);
    }

    public static bool ClosePosition(Transform a, Transform b) {
        return Close(a.position, b.position);
    }

    public static bool CloseUp(Transform a, Transform b) {
        return Close(a.up, b.up);
    }

    public static bool CloseForward(Transform a, Transform b) {
        return Close(a.forward, b.forward);
    }

    public static bool CloseScale(Transform a, Transform b) {
        return Close(a.lossyScale, b.lossyScale);
    }

    public static bool Close(Vector3 a, Vector3 b) {
        return Vector3.SqrMagnitude(a - b) < 0.01f;
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
        foreach (T element in source) {
            action(element);
        }
    }

    public static List<T> GetComponentsInMyChildren<T>(this MonoBehaviour mb) where T : MonoBehaviour {
        List<T> res = new List<T>();
        foreach (Transform c in mb.transform) {
            T component = c.GetComponent<T>();
            if (component != null) {
                res.Add(component);
            }
        }
        return res;
    }

    public static List<Transform> Children(this Transform t) {
        List<Transform> res = new List<Transform>();
        foreach (Transform c in t) {
            res.Add(c);
        }
        return res;
    }

    public static int Modulo(int x, int y) {
        return (x % y + y) % y;
    }

    public static float Direction(this Vector2 vector) {
        return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
    }

    public static T Rnd<T>(this List<T> collection) {
        return collection[UnityEngine.Random.Range(0, collection.Count)];
    }

    public static T Rnd<T>(this List<T> collection, int from, int to) {
        return collection[UnityEngine.Random.Range(from, to)];
    }

    public static List<T> RndSelection<T>(this List<T> collection, int cnt) {
        if (cnt > collection.Count) {
            return collection;
        }
        List<T> result = new List<T>();
        int trash = collection.Count - cnt;
        collection.ForEach(x => {
            if (UnityEngine.Random.Range(0, cnt + trash) < cnt) {
                result.Add(x);
                --cnt;
            } else {
                --trash;
            }
        });
        return result;
    }

    public static List<T> Range<T>(this List<T> collection, int from, int to) {
        if (from < 0) {
            from = 0;
        }
        if (to >= collection.Count) {
            to = collection.Count - 1;
        }
        return collection.GetRange(from, to - from + 1);
    }

    public static Vector2 Scaled(this Vector2 v, Vector2 scale) {
        v.Scale(scale);
        return v;
    }

    public static Vector2 Inverse(this Vector2 v) {
        return new Vector2(1 / v.x, 1 / v.y);
    }

    public static Vector2 Clamp(this Vector2 v, Vector2 min, Vector2 max) {
        return new Vector2(Mathf.Clamp(v.x, min.x, max.x), Mathf.Clamp(v.y, min.y, max.y));
    }

    public static Vector2 Clamp(this Vector2 v, Vector2 max) {
        return Clamp(v, Vector2.zero, max);
    }

    public static double Rnd(double min, double max) {
        return (double)(UnityEngine.Random.Range((float)min, (float)max));
    }

    public static float GaussianRnd() {
        float sum = 0;
        for (int i = 0; i < 12; i++) {
            sum += UnityEngine.Random.Range(-1f, 1f);
        }
        return sum;
    }

    public static bool Editor() {
#if UNITY_EDITOR
        return !UnityEditor.EditorApplication.isPlaying;
#else 
        return false;
#endif
    }

    public static void Destroy(this Component c) {
        if (Editor()) {
            UnityEngine.GameObject.DestroyImmediate(c.gameObject);
        } else {
            c.gameObject.SetActive(false);
            UnityEngine.GameObject.Destroy(c.gameObject);
        }
    }
}
