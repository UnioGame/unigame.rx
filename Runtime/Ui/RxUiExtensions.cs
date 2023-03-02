namespace UniGame.Rx.Runtime.Extensions
{
    using Extensions;
    using UnityEngine;
    using UnityEngine.UI;
    using System;
    using TMPro;
    using UniGame.Core.Runtime;
    using UniGame.Runtime.Common;
    using UniModules.UniCore.Runtime.Utils;
    using UniModules.UniUiSystem.Runtime.Utils;
    using UniRx;
    
    public static class RxUiExtensions
    {
        
        #region ugui

        public static TView Bind<TView,TValue>(this TView sender, IObservable<TValue> source, Button command)
            where TView : ILifeTimeContext
        {
            if (command == null) return sender;
            return sender.Bind(source,x => command.onClick?.Invoke());
        }
        
        public static TView Bind<TView>(this TView sender, IObservable<float> source, Slider slider)
            where TView : ILifeTimeContext
        {
            return source == null || slider == null 
                ? sender 
                : sender.Bind(source, x => slider.value = x);
        }

        public static TView Bind<TView>(this TView sender, Button source, Action<Unit> command, int throttleTime = 0)
            where TView : ILifeTimeContext
        {
            return source == null ? sender : sender.Bind(source, () => command(Unit.Default), throttleTime);
        }
        
        public static TView Bind<TView>(this TView sender, Button source, 
            ISignaleValueProperty<bool> value, int throttleTime = 0)
            where TView : ILifeTimeContext
        {
            return source == null ? sender 
                : sender.Bind(source, () => value.SetValue(true) , throttleTime);
        }
        
        public static TView Bind<TView>(this TView sender, Button source, Action<Unit> command, TimeSpan throttleTime)
            where TView : ILifeTimeContext
        {
            return source == null ? sender : sender.Bind(source, () => command(Unit.Default), throttleTime);
        }

        public static TView Bind<TView>(this TView sender, Button source, Action command,TimeSpan throttleTime)
            where TView : ILifeTimeContext
        {
            if (!source) return sender;

            var clickObservable = throttleTime.TotalMilliseconds <= 0
                ? source.OnClickAsObservable()
                : source.OnClickAsObservable().ThrottleFirst(throttleTime);

            return sender.Bind(clickObservable, command);
        }

        public static TView Bind<TView>(this TView view, IObservable<bool> source, Button button)
            where TView : ILifeTimeContext
        {
            return !button ? view : view.Bind(source, x => button.interactable = x);
        }
        
        public static TView Bind<TView>(this TView view, Button source, IReactiveCommand<Unit> command, int throttleInMilliseconds = 0)
            where TView : ILifeTimeContext
        {
            return !source ? view : Bind(view, source, () => command.Execute(Unit.Default), TimeSpan.FromMilliseconds(throttleInMilliseconds));
        }
        
        public static TView Bind<TView>(this TView view, IObservable<Unit> source, IReactiveCommand<Unit> command)
                    where TView : ILifeTimeContext
        {
            return view.Bind(source, x => command.Execute(Unit.Default));
        }
        
        public static TView Bind<TView>(this TView sender, Button source, Action command,int throttleInMilliseconds = 0)
            where TView : ILifeTimeContext
        {
            return Bind(sender, source, command,TimeSpan.FromMilliseconds(throttleInMilliseconds));
        }

                        
        public static TView Bind<TView>(this TView view, IObservable<Sprite> source, Button button, int frameThrottle = 0)
            where TView : ILifeTimeContext
        {
            if (!button || !button.image)
                return view;
            
            return view.Bind(source, x => button.image.SetValue(x));
        }
        
        
        public static TView Bind<TView>(this TView view, IObservable<bool> source, Toggle toggle)
            where TView : ILifeTimeContext
        {
            return !toggle ? view : view.Bind(source, x => toggle.isOn = x);
        }
        
        public static TSource Bind<TSource>(this TSource view, Toggle source, IReactiveProperty<bool> value)
            where TSource : ILifeTimeContext
        {
            return !source ? view : view.Bind(source.OnValueChangedAsObservable(), value);
        }
        
        public static TSource Bind<TSource>(this TSource view, Toggle source, IReactiveCommand<bool> value)
            where TSource : ILifeTimeContext
        {
            if (source == null) return view;
            var observable = source.OnValueChangedAsObservable();
            return view.Bind(observable, value, view.LifeTime);
        }

        public static TView Bind<TView>(this TView view, Toggle source, Action<bool> value)
            where TView : ILifeTimeContext
        {
            return !source ? view : view.Bind(source.OnValueChangedAsObservable(), value);
        }

        public static TView Bind<TView>(this TView view, IObservable<bool> source, CanvasGroup group)
            where TView : ILifeTimeContext
        {
            if (!group) return view;
            return view.Bind(source,x => group.interactable = x);
        }
        
        public static TView Bind<TView>(this TView view, IObservable<Sprite> source, Image image)
            where TView : ILifeTimeContext
        {
            return !image 
                ? view 
                : view.Bind(source.Where(x => x!=null), x => image.SetValue(x) );
        }

        public static TView Bind<TView>(this TView view, IObservable<Texture> source, RawImage image)
            where TView : ILifeTimeContext
        {
            return !image 
                ? view 
                : view.Bind(source.Where(x => x!=null), x => image.texture = x );
        }

        #endregion
        
        public static TView Bind<TView>(this TView view, IObservable<string> source, TextMeshProUGUI text)
            where TView : ILifeTimeContext
        {
            return view.Bind(source,x => text.SetValue(x));
        }
        
        public static TView Bind<TView>(this TView view, IObservable<int> source, TextMeshProUGUI text)
            where TView : ILifeTimeContext
        {
            return view.Bind(source, x => text.SetValue(x.ToStringFromCache()));
        }
        
        public static TView Bind<TView,TValue>(this TView view,
            IObservable<TValue> source,
            Func<TValue,string> format, TextMeshProUGUI text)
            where           TView : ILifeTimeContext
        {
            var stringObservable = source.Select(format);
            return view.Bind(stringObservable, text);
        }
        
        public static TView Bind<TView>(this TView view, IObservable<string> source, TMP_Text text)
            where TView : ILifeTimeContext
        {
            return !text ? view : view.Bind(source,x => text.SetValue(x));
        }

        public static TView Bind<TView>(this TView view, IObservable<string> source, TextMeshPro text)
            where TView : ILifeTimeContext
        {
            if (!text) return view;
            return view.Bind(source, x => text.text = x);
        }

        public static TView Bind<TView>(this TView view, IObservable<int> source, TextMeshPro text)
            where TView : ILifeTimeContext
        {
            return view.Bind(source, x => text.text = x.ToStringFromCache());
        }
        
        
    }
}