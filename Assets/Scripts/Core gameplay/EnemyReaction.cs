using UnityEngine;

namespace CoreGameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyReaction : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 20f;
        [SerializeField] private float knockbackForce = 25f;
        [SerializeField] private float upwardForce = 2f;
        [SerializeField] private float randomSpread = 0.1f;
        [SerializeField] private float destroyAfterSeconds = 3f;
        [SerializeField] private float destroyXThreshold = -20f;

        private Rigidbody rb;
        private bool isMoving = true;   // двигается ли враг (после удара перестаёт)
        private bool isHit = false;      // получил ли удар

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;   // пока не получил удар – управляем через MovePosition/Translate
            rb.useGravity = false;
        }

        void Update()
        {
            if (!isMoving) return;

            // Движение влево (навстречу игроку)
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime, Space.World);

            // Уничтожение при выходе за границу экрана
            if (transform.position.x < destroyXThreshold)
                Destroy(gameObject);
        }

        public void ReactToHit(Vector3 hitDirection, float worldSpeed)
        {
            if (isHit) return; // уже отреагировал
            isHit = true;
            isMoving = false;

            // Включаем физику для отбрасывания
            rb.isKinematic = false;
            rb.useGravity = true;

            // Небольшой разброс для разнообразия
            Vector3 random = new Vector3(0, Random.Range(0f, randomSpread), Random.Range(-randomSpread, randomSpread));
            Vector3 finalDir = (hitDirection + random).normalized;

            // Применяем импульс
            rb.AddForce(finalDir * knockbackForce + Vector3.up * upwardForce, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);

            // Уничтожаем через некоторое время
            Destroy(gameObject, destroyAfterSeconds);
        }
    }
}