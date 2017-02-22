using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace com.Gemfile.Merger
{
    public interface ISwipeInput
    {
        SwipeEvent OnSwipeMove { get; }
        SwipeEvent OnSwipeEnd { get; }
        SwipeEvent OnSwipeCancel { get; }
    }

    public class SwipeEvent: UnityEvent<SwipeInfo> {}
    
    public class SwipeInfo 
	{
		public Direction direction;
        public Vector2 touchDeltaFirst;
		public Vector2 touchDelta;
		public float timeDelta;
		public bool hasFreePass;
	}

	public enum Direction 
	{
		None,
		Up, 
		Down, 
		Left, 
		Right
	};

    public class SwipeInput: MonoBehaviour, ISwipeInput
	{
        public SwipeEvent OnSwipeEnd { get { return onSwipeEnd; } }
        readonly SwipeEvent onSwipeEnd = new SwipeEvent();
        public SwipeEvent OnSwipeMove { get { return onSwipeMove; } }
        readonly SwipeEvent onSwipeMove = new SwipeEvent();
        public SwipeEvent OnSwipeCancel { get { return onSwipeCancel; } }
        readonly SwipeEvent onSwipeCancel = new SwipeEvent();
    	
        float timeBegin = 0;
        float timeEnd = 0;
        Vector2 touchBegin = Vector2.zero;
        Vector2 touchEnd = Vector2.zero;
        Vector2 touchDeltaFirst = Vector2.zero;
        bool isTouchDown = false;

        void Update () 
		{
            KeyboardUpdate();
            TouchUpdate();
    	}

        void TouchUpdate() 
		{
            bool isPointerOverGui = EventSystem.current.IsPointerOverGameObject();
#if UNITY_IOS || UNITY_ANDROID
            ReadTouchInput(isPointerOverGui);
#endif
#if UNITY_EDITOR
            ReadMouseInput(isPointerOverGui);
#endif

            JudgeInputIsRight(isPointerOverGui);
        }

        void JudgeInputIsRight(bool isPointerOverGui)
        {
            if (timeBegin > 0 && timeEnd > 0 && !isPointerOverGui) 
			{
                float timeDelta = timeEnd - timeBegin;

                Vector2 touchDelta = touchEnd - touchBegin;
                Direction direction = GetDirection(ref touchDelta);
                if (direction != Direction.None) 
				{
                    if (touchDeltaFirst == Vector2.zero) {
                        touchDeltaFirst = touchDelta;
                    }

                    var swipeInfo = new SwipeInfo {
                        direction = direction, 
                        touchDeltaFirst = touchDeltaFirst,
                        touchDelta = touchDelta, 
                        timeDelta = timeDelta,
                        hasFreePass = false,
                    };

                    if (isTouchDown) {
                        onSwipeMove.Invoke(swipeInfo);
                    } else {
                        if (touchDeltaFirst.normalized != touchDelta.normalized) {
                            onSwipeCancel.Invoke(null);
                        } else {
                            onSwipeEnd.Invoke(swipeInfo);
                        }
                        Reset();
                    }
                }
            }
        }

        Direction GetDirection(ref Vector2 touchDelta)
        {
            float absoluteX = Math.Abs(touchDelta.x);
            float absoluteY = Math.Abs(touchDelta.y);
            Direction direction = Direction.None;

            if (absoluteX > absoluteY)
            {
                touchDelta.y = 0;
                if (touchDelta.x > 0)
                {
                    direction = Direction.Right;
                } 
                else 
                {
                    direction = Direction.Left;
                }
            } 
            else if (absoluteX < absoluteY)
            {
                touchDelta.x = 0;
                if (touchDelta.y > 0) 
                {
                    direction = Direction.Up;
                } 
                else 
                {
                    direction = Direction.Down;
                }
            }

            return direction;
        }

        void ReadTouchInput(bool isPointerOverGui)
        {
            if (Input.touchCount > 0 && !isPointerOverGui) 
			{
                var firstTouch = Input.GetTouch(0);
                var touchPhase = firstTouch.phase;
                var touchPosition = firstTouch.position;

                if (touchPhase == TouchPhase.Began)
				{
                    timeBegin = Time.time;
                    touchBegin = touchPosition;
                    isTouchDown = true;
                }

                if (touchPhase == TouchPhase.Moved)
                {
                    timeEnd = Time.time;
                    touchEnd = touchPosition;
                }

                if (touchPhase == TouchPhase.Ended)
                {
                    isTouchDown = false;
                }

                if (touchPhase == TouchPhase.Canceled)
                {
                    Reset();
                }
            }
        }

        void ReadMouseInput(bool isPointerOverGui)
        {
            if (Input.GetMouseButtonDown(0) && !isPointerOverGui) 
			{
                timeBegin = Time.time;
                touchBegin = Input.mousePosition;
                isTouchDown = true;
            }

            if (touchBegin != Vector2.zero) 
			{
                timeEnd = Time.time;
                touchEnd = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                isTouchDown = false;
            }
        }

        void Reset()
        {
            touchDeltaFirst = touchBegin = touchEnd = Vector2.zero;
            timeBegin = timeEnd = 0;
        }

        void KeyboardUpdate() 
		{
            Direction direction = Direction.None;
            if (Input.GetKeyDown(KeyCode.LeftArrow)) 
			{
                direction = Direction.Left;
            } 
			else if (Input.GetKeyDown(KeyCode.RightArrow)) 
			{
                direction = Direction.Right;
            } 
			else if (Input.GetKeyDown(KeyCode.UpArrow)) 
			{
                direction = Direction.Up;
            } 
			else if (Input.GetKeyDown(KeyCode.DownArrow)) 
			{
                direction = Direction.Down;
            }

            if (direction != Direction.None) 
			{
                onSwipeEnd.Invoke(new SwipeInfo(){
                    direction   = direction, 
                    hasFreePass  = true
                });
            }
        }

    }
}