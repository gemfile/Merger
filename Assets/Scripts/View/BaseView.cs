using UnityEngine;

namespace com.Gemfile.Merger
{
    public interface IBaseView
    {
        void Init();
        Transform Transform { get; }
        GameObject GameObject { get; }
    }
    
    public class BaseView: MonoBehaviour, IBaseView
    {
        public virtual void Init()
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