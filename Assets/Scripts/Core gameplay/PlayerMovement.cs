using UnityEngine;
using TMPro;
using System.Collections;

namespace CoreGameplay
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Speed Settings")]
        [SerializeField] private float startSpeed = 5f;
        [SerializeField] private float accelerationPerSecond = 0.5f;
        [SerializeField] private float maxSpeed = 15f;

        [Header("UI Settings")]
        [SerializeField] private TMP_Text speedText;

        [Header("Lane Settings")]
        [SerializeField] private Transform laneTransform;

        private float currentSpeed;
        private float speedMultiplier = 1f;
        private Coroutine multiplierCoroutine;

        public float CurrentSpeed => currentSpeed * speedMultiplier;

        void Start()
        {
            currentSpeed = startSpeed;
        }

        void Update()
        {
            Vector3 direction = new Vector3(1,0,0);
            currentSpeed = Mathf.Min(currentSpeed + accelerationPerSecond * Time.deltaTime, maxSpeed);
            transform.Translate(direction * CurrentSpeed * Time.deltaTime);
            speedText.text = $"Speed: {CurrentSpeed}";

            laneTransform.position = new Vector3(transform.position.x+50,0,0);
        }

        public void ApplySpeedMultiplier(float multiplier, float duration)
        {
            if (multiplierCoroutine != null)
                StopCoroutine(multiplierCoroutine);
            multiplierCoroutine = StartCoroutine(SpeedMultiplierRoutine(multiplier, duration));
        }

        public void SetBaseStats(float startSpeed, float maxSpeed, float acceleration)
        {
            this.startSpeed = startSpeed;
            this.maxSpeed = maxSpeed;
            this.accelerationPerSecond = acceleration;
            
            currentSpeed = startSpeed;
        }

        private System.Collections.IEnumerator SpeedMultiplierRoutine(float multiplier, float duration)
        {
            speedMultiplier = multiplier;
            Debug.Log($"Ускорение началось. Скорость {CurrentSpeed}");
            yield return new WaitForSeconds(duration);
            speedMultiplier = 1f;
            Debug.Log("Ускорение закончилось");
            multiplierCoroutine = null;
        }
    }
}