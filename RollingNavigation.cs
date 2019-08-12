using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RollingNavigation : MonoBehaviour {

    public float objectRadius;
    [Tooltip("How much the object should be allowed to turn without rotating")]
    [Range(1, 20)]
    public float tiltBoundary = 5f;
    [Range(0, 90)]
    public float tiltAmount = 30f;
    [Tooltip("Number of degrees per second the object will tilt")]
    [Range(0, 50)]
    public float tiltSpeed = 5f;
    private NavMeshAgent nav;
    private Transform parentForm;
    private Vector3 prevVelocity;
    private float totalAngleRotated;
    private bool isTilting;

    void Start() {
        nav = GetComponentInParent<NavMeshAgent>();
        parentForm = transform.parent.GetComponent<Transform>();
        prevVelocity = Vector3.zero;
        totalAngleRotated = 0f;
        isTilting = false;
    }

    void Update() {
        transform.Rotate(Vector3.right * Time.deltaTime * Vector3.Magnitude(nav.velocity) * 360f / (2f * Mathf.PI * objectRadius)); //object rolls according to its navigation speed
        StartCoroutine(AutoTilt());
    }

    IEnumerator AutoTilt() {
        Vector3 currentVelocity = parentForm.transform.forward;
        if (!isTilting) {
            Vector3 rPrime = currentVelocity; //derivative of global position vector
            Vector3 rDoublePrime = (currentVelocity - prevVelocity) / Time.deltaTime; //second derivative of global position vector
            float curvature = Vector3.Magnitude(Vector3.Cross(rPrime, rDoublePrime)) / Mathf.Pow(Vector3.Magnitude(rPrime), 3); //radius of osculating circle
            if (curvature > Time.deltaTime * tiltBoundary) {
                if (Vector3.SignedAngle(currentVelocity, prevVelocity, parentForm.transform.up) > 0) {
                    StartCoroutine(RotateObj(tiltAmount - totalAngleRotated)); //object should tilt outward
                }
                else {
                    StartCoroutine(RotateObj((-1f * tiltAmount) - totalAngleRotated)); //object should tilt inward
                }
            }
            else {
                StartCoroutine(RotateObj(-1f * totalAngleRotated)); //object should tilt back to original position
            }
        }
        prevVelocity = currentVelocity;
        yield return new WaitForEndOfFrame();
    }
    IEnumerator RotateObj(float amountToRotate) {
        isTilting = true; //prevents further instances of this coroutine from starting
        float angleThisTime = amountToRotate * Time.deltaTime * tiltSpeed;
        transform.RotateAround(parentForm.transform.position, parentForm.transform.forward, angleThisTime);
        totalAngleRotated += angleThisTime;
        yield return new WaitForEndOfFrame();
        if (Mathf.Abs(amountToRotate) >= Time.deltaTime) { //should keep tilting
            StartCoroutine(RotateObj(amountToRotate - angleThisTime)); //recursive call
        }
        else { //finished tilting
            isTilting = false;
        }
    }

    public void StopTilting() {
        StopAllCoroutines();
        isTilting = true;
    }
}