﻿using UnityEngine;
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
    public float cnt = 4;
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
    Vector2 mouse;
    Vector2 mouseVelocity;
    float velocityDistance;
    float baseVelocityDistance;
    float relativeVelocityDistance;
    float penalty;
    public float minSpeed = -1;
    float maxSpeed = 1;
    float coloringMultiplier = 2f;
    float recentSpeed;
    public float startDifficulty = 1.1f;
    public float difficultySpeed = 1f;
    public float hideCursorTime = 1f;

    Vector4 lastMouse;
    Vector4 secondLastMouse;

    public Queue<Vector4> sliderValues = new Queue<Vector4>();

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
            Cursor.visible = true;
        } else {
            if (Time.time > lastMouse.w + hideCursorTime) {
                Cursor.visible = false;
            }
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
        float speed = Mathf.Clamp((basePenalty - penalty) / basePenalty, minSpeed, maxSpeed);
        slider.value += speed * Time.deltaTime / follower.period / minPeriodsRequired;
        sliderValues.Enqueue(new Vector4(slider.value, 0, 0, Time.time));
        while (sliderValues.Peek().w < Time.time - 0.1f) {
            sliderValues.Dequeue();
        }
        if (sliderValues.Peek().w < Time.time) {
            recentSpeed = (slider.value - sliderValues.Peek().x) / (Time.time - sliderValues.Peek().w);
        }
        sphereRenderer.material.SetColor("_EmissionColor", Color.Lerp(bad, good, (recentSpeed * follower.period * minPeriodsRequired + 1) / 2));
        scoreText.text = score.ToString();

        if (slider.value == 1) {
            NextLevel();
        }
    }

    void Start() {
        distanceSphereRenderer = distanceSphere.GetComponentInChildren<MeshRenderer>();
        sphereRenderer = target.GetComponentInChildren<MeshRenderer>();
        NextLevel();
    }

    void Save() {
    }

    public void NextLevel() {
        score++;
        cnt = startDifficulty + difficultySpeed * Mathf.Pow(score, 0.5f);
        slider.value = 0;
        follower.speed = targetSpeed; 
        RandomTrajectory();
    }

    [ContextMenu("Random trajectory")]
    public void RandomTrajectory() {
        follower.RandomTrajectory(cnt);
    }

}
