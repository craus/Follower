using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mathematics;
using SparseCollections;
using System;
using System.Linq;
using UnityEngine.UI;

public class Checker : MonoBehaviour {

    public Follower follower;
    public Transform target;
    public Transform distanceSphere;
    MeshRenderer distanceSphereRenderer;
    MeshRenderer sphereRenderer;
    public Slider slider;
    public Text scoreText;
    public float baseDistance = 8;
    public int cnt = 4;
    public float targetSpeed = 3;
    public float minPeriodsRequired = 2;
    public Color bad;
    public Color normal;
    public Color good;
    public int score;
    public float minValuableVelocity = 1;
    public float sqrNormalizedDistancePenalty = 1;
    public float sqrNormalizedVelocityDistancePenalty = 1;
    public float basePenalty = 1;
    public Vector2 mouse;
    public Vector2 mouseVelocity;
    public float velocityDistance;
    public float baseVelocityDistance;
    public float relativeVelocityDistance;
    public float penalty;

    public Vector4 lastMouse;
    public Vector4 secondLastMouse;

    public void Update() {
        mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Time.time > lastMouse.w) {
            mouseVelocity = (mouse - secondLastMouse.xy()) / (Time.time - secondLastMouse.w);
        } else {
            mouseVelocity = Vector2.zero;
        }
        if ((mouse - lastMouse.xy()) != Vector2.zero) {
            secondLastMouse = lastMouse;
            lastMouse = new Vector4(mouse.x, mouse.y, 0, Time.time);
        }
        Vector2 targetVelocity = follower.Velocity(Time.time);
        velocityDistance = (targetVelocity - mouseVelocity).magnitude;
        baseVelocityDistance = Mathf.Max(targetVelocity.magnitude, minValuableVelocity);
        float relativeVelocityDistance = velocityDistance / Mathf.Max(targetVelocity.magnitude, minValuableVelocity);
        distanceSphere.transform.position = mouse;
        float distance = (mouse - target.position.xy()).magnitude;
        distanceSphere.localScale = Vector2.one * distance * 2;
        float relativeDistance = distance / baseDistance;
        penalty = sqrNormalizedDistancePenalty * Mathf.Pow(relativeDistance, 2) + sqrNormalizedVelocityDistancePenalty * Mathf.Pow(relativeVelocityDistance, 2);
        float speed = (basePenalty - penalty) / basePenalty;
        distanceSphereRenderer.material.SetColor("_EmissionColor", Color.Lerp(bad, good, (speed + 1) / 2));
        sphereRenderer.material.SetColor("_EmissionColor", Color.Lerp(bad, good, slider.value));
        slider.value += speed * Time.deltaTime / follower.period / minPeriodsRequired;
        scoreText.text = score.ToString();

        if (slider.value == 1) {
            score++;
            cnt = 2 + (int)(Math.Log(score, 2));
            NextLevel();
            slider.value = 0;
        }
    }

    void Start() {
        distanceSphereRenderer = distanceSphere.GetComponentInChildren<MeshRenderer>();
        sphereRenderer = target.GetComponentInChildren<MeshRenderer>();
        NextLevel();
    }

    private void NextLevel() {
        follower.speed = targetSpeed;
        follower.RandomTrajectory(cnt);
    }

}
