using System;
using System.Collections.Generic;
using System.Linq;
using UI.Base.Manager;
using UI.Widget.Observer;

namespace UI.Widget
{
    public class UIWidgetManager : UIManagerBase<UIWidgetBase>, IWidgetManager
    {
        private readonly Dictionary<UIWidgetBase, HashSet<Action>> _releaseActions = new Dictionary<UIWidgetBase, HashSet<Action>>();

        protected override void OnInsert(UIWidgetBase element)
        {
            _releaseActions.Add(element, new HashSet<Action>());
        }

        protected override void OnExtract(UIWidgetBase element)
        {
            if (_releaseActions.TryGetValue(element, out HashSet<Action> actions))
            {
                actions.ToList().ForEach(x => x?.Invoke());
                actions.Clear();
                _releaseActions.Remove(element);
            }
        }

        IWidgetObserver<TElement> IWidgetManager.Get<TElement>(Predicate<TElement> predicate)
        {
            return CreateObserver<TElement>(predicate);
        }
        
        private IWidgetObserver<TElement> CreateObserver<TElement>(Predicate<TElement> predicate) where TElement : UIWidgetBase
        {
            if (!Find(out TElement element, predicate))
                throw new NullReferenceException($"Can't find {typeof(TElement)} element in {GetType().Name}");
            
            WidgetObserver<TElement> observer = new WidgetObserver<TElement>(element);

            _releaseActions[element].Add(ReleaseAction);
            observer.Release += releaseElement => _releaseActions[element].Remove(ReleaseAction);

            void ReleaseAction()
            {
                ((IWidgetObserver<TElement>) observer).Release();
            }
            
            return observer;
        }
    }
}