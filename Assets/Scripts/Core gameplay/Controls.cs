using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using CoreGameplay.Audio; // добавлено

namespace CoreGameplay
{
    public class Controls : MonoBehaviour
    {
        private Vector2Int localPos = Vector2Int.zero;
        public float blockSize = 3f;
        public float smoothing = 5f;
        public GameObject Car;
        public Vector2 tiltAngle = Vector2.one * 20f;
        public float tiltSpeed = 1f;
        public float tiltTime = 1f;
        private Vector3 fp;
        private Vector3 lp;
        private bool isTouch = false;
        private bool dragDo = false;
        [SerializeField] private float dragDistance = 50f;
        int i = 0;
        private float timer;
        [SerializeField] private float tapTime = 1f;
        private Vector2 targetTilt;
        private static Controls Instance { get; set; }
        private Vector3 TargetPosition => new(transform.position.x, localPos.y * blockSize, -localPos.x * blockSize);

        private void Awake()
        {
            EnhancedTouchSupport.Enable();
            Instance = this;
        }

        void Update()
        {
            KeyboardInput();
            TouchInput();
            localPos = new Vector2Int(Mathf.Clamp(localPos.x, -1, 1), Mathf.Clamp(localPos.y, -1, 1));
            transform.position = Vector3.Lerp(transform.position, TargetPosition, smoothing * Time.deltaTime);
            Vector3 rot = Car.transform.eulerAngles;
            rot.x = Mathf.LerpAngle(rot.x, -targetTilt.y, tiltSpeed * Time.deltaTime);
            rot.z = Mathf.LerpAngle(rot.z, -targetTilt.x, tiltSpeed * Time.deltaTime);
            Car.transform.eulerAngles = rot;
        }

        void KeyboardInput()
        {
            if (Keyboard.current.wKey.wasPressedThisFrame && Move(Vector2Int.up)) 
            {
                StartCoroutine(DoTilt(Vector2.up));
                AudioManager.Instance?.PlayLaneChange(); // добавлено
            }
            if (Keyboard.current.aKey.wasPressedThisFrame && Move(Vector2Int.left)) 
            {
                StartCoroutine(DoTilt(Vector2.left));
                AudioManager.Instance?.PlayLaneChange();
            }
            if (Keyboard.current.sKey.wasPressedThisFrame && Move(Vector2Int.down)) 
            {
                StartCoroutine(DoTilt(Vector2.down));
                AudioManager.Instance?.PlayLaneChange();
            }
            if (Keyboard.current.dKey.wasPressedThisFrame && Move(Vector2Int.right)) 
            {
                StartCoroutine(DoTilt(Vector2.right));
                AudioManager.Instance?.PlayLaneChange();
            }
        }

        private void TouchInput()
        {
            if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
            {
                UnityEngine.InputSystem.EnhancedTouch.Touch touch = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0];
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    fp = touch.screenPosition;
                    lp = touch.screenPosition;
                    isTouch = true;
                    dragDo = false;
                }
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
                {
                    lp = touch.screenPosition;
                    if (isTouch && (Mathf.Abs(lp.x - fp.x) > dragDistance || Mathf.Abs(lp.y - fp.y) > dragDistance))
                    {
                        dragDo = true;
                        isTouch = false;
                        if (Mathf.Abs(lp.x - fp.x) > Mathf.Abs(lp.y - fp.y))
                        {
                            if ((lp.x > fp.x))
                            {
                                Debug.Log("Right Swipe");
                                if (Move(Vector2Int.right))
                                {
                                    StartCoroutine(DoTilt(Vector2.right));
                                    AudioManager.Instance?.PlayLaneChange();
                                }
                            }
                            else
                            {
                                Debug.Log("Left Swipe");
                                if (Move(Vector2Int.left))
                                {
                                    StartCoroutine(DoTilt(Vector2.left));
                                    AudioManager.Instance?.PlayLaneChange();
                                }
                            }
                        }
                        else
                        {
                            if (lp.y > fp.y)
                            {
                                Debug.Log("Up Swipe");
                                if (Move(Vector2Int.up))
                                {
                                    StartCoroutine(DoTilt(Vector2.up));
                                    AudioManager.Instance?.PlayLaneChange();
                                }
                            }
                            else
                            {
                                Debug.Log("Down Swipe");
                                if (Move(Vector2Int.down))
                                {
                                    StartCoroutine(DoTilt(Vector2.down));
                                    AudioManager.Instance?.PlayLaneChange();
                                }
                            }
                        }
                    }
                }
                else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended && !dragDo)
                {
                    i += 1;
                    timer = tapTime;
                    if (timer > 0)
                    {
                        if (i == 2)
                        {
                            Debug.Log("DoubleTap");
                            i = 0;
                        }
                    }
                }
            }
        }

        private bool Move(Vector2Int vector)
        {
            localPos += vector;
            return true;
        }

        private IEnumerator DoTilt(Vector2 side)
        {
            targetTilt = tiltAngle * side;
            yield return new WaitForSeconds(tiltTime);
            targetTilt = Vector2.zero;
        }
    }
}