using Signals;
using strange.extensions.pool.api;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core
{
    public class InputManager : MonoBehaviour, IInputManager
    {
        public TouchSignal OnPointerDown { get; private set; }
        public TouchSignal OnPointerUp { get; private set; }
        [Inject]
        public IPool<TouchData> TouchPool { get; private set; }
        [Inject]
        public ICoreLogger Logger { get; private set; }
        public float TouchSensetivity { get; set; }
        #region IInputManager
        public List<ITouchData> Touches { get; private set; }
        public bool MouseSupported { get; private set; }
        public Vector3 MousePosition { get { return ClampedMousePosition(); } }
        #endregion

        private Vector2 mousePosition = Vector2.zero;

        #region Unity
        // Use this for initialization after deserialization
        void Awake()
        {
            OnPointerDown = new TouchSignal();
            OnPointerUp = new TouchSignal();
            Touches = new List<ITouchData>();
            TouchSensetivity = 0.1f; // Same as default axis sensetivity for Unity's InputManager
            MouseSupported = Input.mousePresent &&
                (
                (Application.platform == RuntimePlatform.LinuxEditor) ||
                (Application.platform == RuntimePlatform.LinuxPlayer) ||
                (Application.platform == RuntimePlatform.OSXEditor) ||
                (Application.platform == RuntimePlatform.OSXPlayer) ||
                (Application.platform == RuntimePlatform.WindowsEditor) ||
                (Application.platform == RuntimePlatform.WindowsPlayer)
                );
        }

        // Use this for initialization after first OnEnable
        void Start()
        {
            TouchPool.inflationType = PoolInflationType.INCREMENT;
            TouchPool.overflowBehavior = PoolOverflowBehavior.IGNORE;
        }

        // Update is called once per frame
        void Update()
        {
            List<ITouchData> touches = GetTouches();
            if (touches != null)
            {
                UpdateTouches(touches);
            }
        }
        #endregion

        public bool IsPointerOverGameObject(ITouchData data)
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(data.Position.x, data.Position.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }

        #region Private
        private List<ITouchData> GetTouches()
        {
            List<ITouchData> touches = new List<ITouchData>();

            if (Input.touchSupported)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    TouchData touchData = TouchPool.GetInstance();
                    touchData.Init(touch, TouchSensetivity);
                    touches.Add(touchData);
                }
            }
            else
            {
                // Mouse emulation
                //Vector2 deltaMousePosition = new Vector2(Input.mousePosition.x - mousePosition.x, Input.mousePosition.y - mousePosition.y);
                Vector2 smoothDeltaMousePosition = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                //Logger.Log(deltaMousePosition);
                bool isStationary = !(Mathf.Abs(smoothDeltaMousePosition.x) > float.Epsilon || Mathf.Abs(smoothDeltaMousePosition.y) > float.Epsilon);
                mousePosition.x = Input.mousePosition.x;
                mousePosition.y = Input.mousePosition.y;
                float time = Time.time;
                if (Input.GetMouseButtonDown(0))
                {
                    TouchData touchData = TouchPool.GetInstance();
                    touchData.Init(-1, TouchPhase.Began, mousePosition, smoothDeltaMousePosition / TouchSensetivity, smoothDeltaMousePosition, 1.0f, 0.0f, time);
                    touches.Add(touchData);
                }
                else if (Input.GetMouseButton(0))
                {
                    TouchData touchData = TouchPool.GetInstance();
                    touchData.Init(-1, isStationary ? TouchPhase.Stationary : TouchPhase.Moved, mousePosition, smoothDeltaMousePosition / TouchSensetivity, smoothDeltaMousePosition, 1.0f, 0.0f, time);
                    touches.Add(touchData);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    TouchData touchData = TouchPool.GetInstance();
                    touchData.Init(-1, TouchPhase.Ended, mousePosition, smoothDeltaMousePosition / TouchSensetivity, smoothDeltaMousePosition, 1.0f, 0.0f, time);
                    touches.Add(touchData);
                }

                // Multi-touch emulation (second touch is located at screen center)
                if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
                {
                    TouchData touchData = TouchPool.GetInstance();
                    touchData.Init(1, TouchPhase.Began, Vector2.zero, Vector2.zero, Vector2.zero, 1.0f, 0.0f, time);
                    touches.Add(touchData);
                }
                else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    TouchData touchData = TouchPool.GetInstance();
                    touchData.Init(1, TouchPhase.Stationary, Vector2.zero, Vector2.zero, Vector2.zero, 1.0f, 0.0f, time);
                    touches.Add(touchData);
                }
                else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl))
                {
                    TouchData touchData = TouchPool.GetInstance();
                    touchData.Init(1, TouchPhase.Ended, Vector2.zero, Vector2.zero, Vector2.zero, 1.0f, 0.0f, time);
                    touches.Add(touchData);
                }
            }

            return touches;
        }

        private void UpdateTouches(List<ITouchData> touches)
        {
            for (int i = 0; i < touches.Count; i++)
            {
                ITouchData touch = touches[i];
                ITouchData storedTouch = FindTouch(Touches, touch.Id);
                switch (touch.Phase)
                {
                    case TouchPhase.Began:
                        {
                            if (storedTouch == null)
                            {
                                Touches.Add(touch);
                            }
                            else
                            {
                                int index = Touches.IndexOf(storedTouch);
                                Touches[index] = touch;
                             
                                //Logger.LogErrorFormat("Duplicated touch begin with Id = {0}", touch.Id);
                            }
                            OnPointerDown.Dispatch(touch);
                            break;
                        }
                    case TouchPhase.Canceled:
                    case TouchPhase.Ended:
                        {
                            if (storedTouch != null)
                            {
                                OnPointerUp.Dispatch(touch);
                                //Touches.Remove(storedTouch);
                                //TouchPool.ReturnInstance(storedTouch);
                            }
                            else
                            {
                                Logger.LogErrorFormat("Unable to find ended touch with Id = {0}", touch.Id);
                            }
                            break;
                        }
                }
                if (storedTouch != null)
                {
                    storedTouch.Update(touch, storedTouch.BeginTime);
                }
            }

            // Remove lost touches
            for (int i = Touches.Count; i > 0; i--)
            {
                if (FindTouch(touches, Touches[i - 1].Id) == null)
                {
                    TouchPool.ReturnInstance(Touches[i - 1]);
                    Touches.RemoveAt(i - 1);
                }
            }
        }

        private ITouchData FindTouch(List<ITouchData> touches, int id)
        {
            for (int i = 0; i < touches.Count; i++)
            {
                ITouchData touch = touches[i];
                if (touch.Id == id)
                {
                    return touch;
                }
            }
            return null;
        }

        private Vector3 ClampedMousePosition()
        {
            return new Vector3(Mathf.Clamp(Input.mousePosition.x, 0.0f, Screen.width - 1), Mathf.Clamp(Input.mousePosition.y, 0.0f, Screen.height - 1), Input.mousePosition.z);
        }
        #endregion
    }
}
