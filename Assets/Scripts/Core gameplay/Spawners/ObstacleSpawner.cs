using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

namespace CoreGameplay
{
    public class ObstacleSpawner : MonoBehaviour
    {
        [Header("Difficulty Settings")]
        [SerializeField] private List<DifficultyLevel> difficultyLevels;

        [Header("Spawn Settings")]
        [SerializeField] private Vector3 spawnOffset = Vector3.zero;
        [SerializeField] private float obstacleLifetime = 10f;

        [Header("UI References")]
        [SerializeField] private TextMeshPro scoreText;

        [Header("Lane Settings")]
        [SerializeField] private List<Transform> laneTransforms;
        [SerializeField] private bool randomLane = true;
        [SerializeField] private bool avoidSameLane = true;
        [SerializeField] private int minDistanceBetweenSameLane = 2;

        private float lastSpawnTime;
        private List<GameObject> spawnedObstacles = new List<GameObject>();
        private int lastUsedLaneIndex = -1;
        private Queue<int> recentLaneHistory = new Queue<int>();

        private int currentDifficultyIndex = 0;
        private float currentDifficultyTimer = 0f;
        private List<int> recentSpawnedIndices = new List<int>();
        private const int MAX_HISTORY_SIZE = 3;

        public float Score { get; private set; }

        [System.Serializable]
        public class DifficultyLevel
        {
            public string name;
            public List<GameObject> obstacles;
            public float duration = 30f;
            public float obstacleSpawnDelay = 3f;
            public float scoreMultiplier=1f;
        }

        void Start()
        {
            if (difficultyLevels == null || difficultyLevels.Count == 0)
            {
                Debug.LogError("No difficulty levels configured!");
                enabled = false;
                return;
            }

            if (laneTransforms == null || laneTransforms.Count == 0)
            {
                float[] offsets = { -3f, 0f, 3f };
                laneTransforms = new List<Transform>();
                for (int i = 0; i < offsets.Length; i++)
                {
                    GameObject go = new GameObject($"Lane_{i}");
                    go.transform.SetParent(transform);
                    go.transform.localPosition = new Vector3(offsets[i], 0f, 0f);
                    laneTransforms.Add(go.transform);
                }
            }

            Score=0f;
            lastSpawnTime = Time.time;
        }

        void Update()
        {
            Score+=difficultyLevels[currentDifficultyIndex].scoreMultiplier;
            if(Score>0)
                scoreText.text = $"Current score: {Score}";

            currentDifficultyTimer += Time.deltaTime;
            UpdateDifficultyLevel();

            if (Time.time - lastSpawnTime >= difficultyLevels[currentDifficultyIndex].obstacleSpawnDelay)
            {
                SpawnObstacle();
                lastSpawnTime = Time.time;
            }

            spawnedObstacles.RemoveAll(obj => obj == null);
        }

        void UpdateDifficultyLevel()
        {
            float currentDuration = difficultyLevels[currentDifficultyIndex].duration;
            if (currentDifficultyTimer >= currentDuration)
            {
                currentDifficultyTimer = 0f;
                currentDifficultyIndex++;
                if (currentDifficultyIndex >= difficultyLevels.Count)
                    currentDifficultyIndex = 0;
                recentSpawnedIndices.Clear();
                Debug.Log($"Switched to difficulty: {difficultyLevels[currentDifficultyIndex].name}");
            }
        }

        void SpawnObstacle()
        {
            GameObject selectedPrefab = GetRandomObstacleForCurrentDifficulty();
            if (selectedPrefab == null) return;

            int laneIndex = ChooseLaneIndex();
            Vector3 spawnPosition = laneTransforms[laneIndex].position + spawnOffset;
            GameObject obstacle = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
            Destroy(obstacle, obstacleLifetime);
            spawnedObstacles.Add(obstacle);
        }

        private int ChooseLaneIndex()
        {
            if (!randomLane) return 0;
            int laneCount = laneTransforms.Count;
            if (laneCount == 1) return 0;

            List<int> availableLanes = Enumerable.Range(0, laneCount).ToList();

            if (avoidSameLane && lastUsedLaneIndex >= 0)
            {
                if (recentLaneHistory.Contains(lastUsedLaneIndex))
                    availableLanes.Remove(lastUsedLaneIndex);
            }

            if (availableLanes.Count == 0)
                availableLanes = Enumerable.Range(0, laneCount).ToList();

            int chosen = availableLanes[Random.Range(0, availableLanes.Count)];
            lastUsedLaneIndex = chosen;
            recentLaneHistory.Enqueue(chosen);
            if (recentLaneHistory.Count > minDistanceBetweenSameLane)
                recentLaneHistory.Dequeue();

            return chosen;
        }

        GameObject GetRandomObstacleForCurrentDifficulty()
        {
            var currentDifficulty = difficultyLevels[currentDifficultyIndex];
            if (currentDifficulty.obstacles == null || currentDifficulty.obstacles.Count == 0)
                return null;

            if (currentDifficulty.obstacles.Count == 1)
                return currentDifficulty.obstacles[0];

            List<int> availableIndices = new List<int>();
            for (int i = 0; i < currentDifficulty.obstacles.Count; i++)
                if (!recentSpawnedIndices.Contains(i))
                    availableIndices.Add(i);

            if (availableIndices.Count == 0)
            {
                availableIndices = Enumerable.Range(0, currentDifficulty.obstacles.Count).ToList();
                recentSpawnedIndices.Clear();
            }

            int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];
            recentSpawnedIndices.Add(randomIndex);
            if (recentSpawnedIndices.Count > MAX_HISTORY_SIZE)
                recentSpawnedIndices.RemoveAt(0);

            return currentDifficulty.obstacles[randomIndex];
        }
    }
}