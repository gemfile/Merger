using UnityEngine;
using System;
using UnityEngine.Events;

namespace Scripts.Util {
	[System.Serializable]
	public class SwipeEvent: UnityEvent<SwipeInfo> {}

	public class SwipeInfo {
		public Direction direction;
		public Vector2 touchDelta;
		public float timeDelta;
		public bool hasPass;
	}

	public enum Direction {
		None,
		Up, 
		Down, 
		Left, 
		Right
	};

    public class Swipe : MonoBehaviour {
        
        public SwipeEvent swipeEvent;
    	
        float timeBegin = 0;
        float timeEnd = 0;
        Vector2 touchBegin = Vector2.zero;
        Vector2 touchEnd = Vector2.zero;

		public Swipe() {
			swipeEvent = new SwipeEvent();
		}

        void Update () {
            KeyboardUpdate();
            TouchUpdate();
    	}

        void TouchUpdate() {
#if UNITY_IOS || UNITY_ANDROID
            if (Input.touchCount > 0) {
                if (Input.GetTouch(0).phase == TouchPhase.Began) {
                    timeBegin = Time.time;
                    touchBegin = Input.GetTouch(0).position;
                }

                if (Input.GetTouch(0).phase == TouchPhase.Ended) {
                    timeEnd = Time.time;
                    touchEnd = Input.GetTouch(0).position;
                }
            }
#endif

#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0)) {
                timeBegin = Time.time;
                touchBegin = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(0)) {
                timeEnd = Time.time;
                touchEnd = Input.mousePosition;
            }
#endif

            if (timeBegin > 0 && timeEnd > 0) {
                float timeDelta = timeEnd - timeBegin;
//                Debug.Log("timeDelta : " + timeDelta);

                Vector2 touchDelta = touchEnd - touchBegin;
                float absoluteX = Math.Abs(touchDelta.x);
                float absoluteY = Math.Abs(touchDelta.y);

                Direction direction = Direction.None;
                if (absoluteX > absoluteY) {
                    if (touchDelta.x > 0) {
                        direction = Direction.Right;
                    } else {
                        direction = Direction.Left;
                    }
                } else {
                    if (touchDelta.y > 0) {
                        direction = Direction.Up;
                    } else {
                        direction = Direction.Down;
                    }
                }

                if (direction != Direction.None) {
                    swipeEvent.Invoke(new SwipeInfo(){
                        direction   = direction, 
                        touchDelta  = touchDelta, 
                        timeDelta   = timeDelta,
                        hasPass  = false
                    });
                }

                timeBegin = timeEnd = 0;
            }
        }

        void KeyboardUpdate() {
            Direction direction = Direction.None;
            if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                direction = Direction.Left;
            } else if (Input.GetKeyDown(KeyCode.RightArrow)) {
                direction = Direction.Right;
            } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
                direction = Direction.Up;
            } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
                direction = Direction.Down;
            }

            if (direction != Direction.None) {
                swipeEvent.Invoke(new SwipeInfo(){
                    direction   = direction, 
                    hasPass  = true
                });
            }
        }
    }
}