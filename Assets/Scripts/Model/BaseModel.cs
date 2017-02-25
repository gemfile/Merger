namespace com.Gemfile.Merger
{
    public interface IBaseModel
    {
        void Init();
    }
    
    [System.Serializable]
    public class BaseModel: IBaseModel
    {
        public virtual void Init()
        {
        }
    }
}