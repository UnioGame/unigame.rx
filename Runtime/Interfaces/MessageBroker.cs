namespace UniGame.Runtime.Rx
{
    using System;
    using System.Collections.Generic;
    using R3;

    public class MessageBroker : IMessageBroker
    {
        public static readonly MessageBroker Default = new MessageBroker();
        
        public Dictionary<Type,object> _subjects = new();
        
        public void Publish<T>(T message)
        {
            var type = typeof(T);
            if (!_subjects.TryGetValue(type, out var value))
            {
                var subject = new Subject<T>();
                value = subject;
                _subjects[type] = value;
            }
            
            var subjectValue = value as Subject<T>;
            subjectValue.OnNext(message);
        }

        public Observable<T> Receive<T>()
        {
            var type = typeof(T);
            if (!_subjects.TryGetValue(type, out var value))
            {
                var subject = new Subject<T>();
                value = subject;
                _subjects[type] = value;
            }

            return value as Observable<T>;
        }
    }
}