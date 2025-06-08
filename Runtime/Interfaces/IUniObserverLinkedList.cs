namespace UniGame.Runtime.Rx
{
    public interface IUniObserverLinkedList<T>
    {
        void UnsubscribeNode(UniObserverNode<T> node);
    }
}