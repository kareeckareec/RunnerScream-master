using System.Collections.Generic;
using UnityEngine;

namespace CoreGameplay.Spawner
{
    public class PickupSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private List<GameObject> pickupPrefabs;
        [SerializeField] private float minSpawnInterval = 2f;
        [SerializeField] private float maxSpawnInterval = 8f;
        [SerializeField] private int maxActivePickups = 3;

        [Header("Lane Settings")]
        [SerializeField] private List<Transform> laneTransforms;
        [SerializeField] private bool randomLane = true;

        private float nextSpawnTime;
        private float currentInterval;
        private List<GameObject> activePickups = new List<GameObject>();

        void Start()
        {
            if (pickupPrefabs == null || pickupPrefabs.Count == 0)
            {
                Debug.LogWarning("PickupSpawner: нет префабов бонусов");
                enabled = false;
                return;
            }

            if (laneTransforms == null || laneTransforms.Count == 0)
            {
                float[] offsets = { -3f, 0f, 3f };
                laneTransforms = new List<Transform>();
                for (int i = 0; i < offsets.Length; i++)
                {
                    GameObject go = new GameObject($"PickupLane_{i}");
                    go.transform.SetParent(transform);
                    go.transform.localPosition = new Vector3(offsets[i], 0f, 0f);
                    laneTransforms.Add(go.transform);
                }
            }

            SetRandomInterval();
            nextSpawnTime = Time.time + currentInterval;
        }

        void Update()
        {
            activePickups.RemoveAll(p => p == null);

            if (Time.time >= nextSpawnTime && activePickups.Count < maxActivePickups)
            {
                SpawnPickup();
                SetRandomInterval();
                nextSpawnTime = Time.time + currentInterval;
            }
        }

        void SetRandomInterval()
        {
            currentInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
        }

        void SpawnPickup()
        {
            GameObject prefab = pickupPrefabs[Random.Range(0, pickupPrefabs.Count)];
            int laneIdx = randomLane ? Random.Range(0, laneTransforms.Count) : 0;
            Vector3 spawnPos = laneTransforms[laneIdx].position;

            GameObject pickup = Instantiate(prefab, spawnPos, Quaternion.identity);
            activePickups.Add(pickup);
        }
    }
}