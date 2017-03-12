using UnityEngine;

namespace com.Gemfile.Merger
{
    public interface IBaseView
    {
        void Init();
        void Reset();
        void ChangeOrientation();
        Transform Transform { get; }
        GameObject GameObject { get; }
    }
    
    public class BaseView: MonoBehaviour, IBaseView
    {
        public virtual void Init()
        {
        }

        public virtual void Reset()
        {

        }

        public virtual void ChangeOrientation()
        {

        }

        public Transform Transform
        {
            get { return transform; }
        }

        public GameObject GameObject
        {
            get { return gameObject; }
        }
    }
}