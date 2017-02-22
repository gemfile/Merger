namespace com.Gemfile.Merger
{
    public interface IBaseController<M, V>
    {
        void Init(V view);
        M Model { get; }
        void Clear();
    }
    
    public class BaseController<M, V>
        where M: BaseModel, new()
        where V: BaseView
    {
        public M Model {
            get { return model; }
        }
        protected M model;
        protected V View {
            get { return view; }
        }
        protected V view;

        public virtual void Init(V view) {
            this.model = new M();
            this.view = view;
            
            model.Init();
            view.Init();
        }

        public virtual void Clear() {
            
        }
    }
}