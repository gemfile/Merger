using UnityEngine;

namespace com.Gemfile.Merger
{
    public interface IBaseView
    {
        void Init();
    }
    
    public class BaseView: MonoBehaviour
    {
        public virtual void Init()
        {
        }
    }
}