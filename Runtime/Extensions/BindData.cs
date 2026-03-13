namespace UniGame.Runtime.Rx.Runtime.Extensions
{
    public struct BindData<TSender,TValue>
    {
        public TSender Source;
        public TValue Value;
    }
    
    public struct BindData<TSender,TData,TValue>
    {
        public TSender Source;
        public TData Data;
        public TValue Value;
    }
}