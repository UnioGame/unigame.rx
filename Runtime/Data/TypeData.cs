namespace UniGame.Runtime.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using global::UniGame.Core.Runtime;
    using global::UniGame.Runtime.Rx;
    using R3;

    [Serializable]
    public class TypeData : ITypeData
    {
        private IValueContainerStatus cachedValue;
        private Type cachedType;
        
        /// <summary>
        /// registered components
        /// </summary>
        private Dictionary<Type, IValueContainerStatus> contextValues = new(32);

        public bool HasValue => contextValues.Any(value => value.Value.HasValue);

        #region writer methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove<TData>()
        {           
            var type = typeof(TData);
            return Remove(type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Release();
        }

        public bool Remove(Type type)
        {
            if (!contextValues.TryGetValue(type, out var value)) return false;
            
            var removed = contextValues.Remove(type);
            if (cachedType == type)
                ResetCache();
            
            return removed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Publish<TData>(TData value)
        {
            var data = GetData<TData>();
            data.Value = value;           
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PublishForce<TData>(TData value)
        {
            var data = GetData<TData>();
            data.SetValueForce(value);        
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Observable<TData> Receive<TData>() =>  GetData<TData>();
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TData Get<TData>()
        {
            var data = GetData<TData>();
            return data == null ? default(TData) : data.Value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object Get(Type type)
        {
            var data = GetData(type);
            return data;
        }

        public bool Contains<TData>()
        {
            var type = typeof(TData);
            return Contains(type);
        }

        public bool Contains(Type type) => contextValues.TryGetValue(type, out var value) && 
                                           value.HasValue;

        public void Release()
        {
            ResetCache();
            
            foreach (var contextValue in contextValues)
            {
                if(contextValue.Value is IDisposable disposable)
                    disposable.Dispose();
            }
            
            contextValues.Clear();
        }

        private void ResetCache()
        {
            cachedType  = null;
            cachedValue = null;
        }

        private ReactiveValue<TValue> CreateContextValue<TValue>() => new();

        private ReactiveValue<TValue> GetData<TValue>()
        {
            if (cachedValue is ReactiveValue<TValue> data)
                return data;

            var type = typeof(TValue);
            
            if (!contextValues.TryGetValue(type, out var value)) {
                value = CreateContextValue<TValue>();
                contextValues[type] = value;
            }
            
            data = value as ReactiveValue<TValue>;
            cachedType = type;
            cachedValue = data;
            
            return data;
        }
        
        public object GetValue(Type type)
        {
            if (!contextValues.TryGetValue(type, out var value))
            {
                return null;
            }
            
            var readonlyValue = value as IReadonlyObjectValue;
            return readonlyValue?.ObjectValue;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object GetData(Type valueType)
        {
            var type = valueType;
            contextValues.TryGetValue(type, out var value);
            
            var readonlyValue = value as IReadonlyObjectValue;
            return readonlyValue?.ObjectValue;
        }
        

        //Editor Only API
#if UNITY_EDITOR

        public IReadOnlyDictionary<Type, IValueContainerStatus> EditorValues => contextValues;

#endif

    }
}
