using System;

namespace Purchase.Base.Server
{
    public class PurchaseProcess<T> : IPurchaseProcess<T>
    {
        event Action<T> CompleteGeneric;
        event Action<T>  IPurchaseProcess<T>.Complete
        {
            add
            {
                if (_done)
                {
                    value?.Invoke(_content);
                    return;
                }

                CompleteGeneric += value;
            }
            remove
            {
                if (!_done)
                {
                    CompleteGeneric -= value;
                }
            }
        }

        bool IPurchaseProcess<T>.Done => _done;
        T IPurchaseProcess<T>.Result => _content;

        private bool _done = false;
        private T _content = default;

        public void Resolve(T content)
        {
            _done = true;
            _content = content;
            
            CompleteGeneric?.Invoke(_content);
            CompleteGeneric = null;
        }
    }
}