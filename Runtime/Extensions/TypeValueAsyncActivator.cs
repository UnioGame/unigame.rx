namespace UniGame.Runtime.Utils
{
    using Cysharp.Threading.Tasks;
    using System;


    public static class TypeValueActivator
    {

        public static async UniTask<TValue> CreateObjectAsync<TValue>(this ITypeValueProvider valueProvider)
            where TValue : class
        {
            var value = await valueProvider.CreateObjectAsync(typeof(TValue));
            return value as TValue;
        }

        public static async UniTask<object> CreateObjectAsync(this ITypeValueProvider valueProvider, Type type)
        {
            var parameters = type.GetParameters();
            foreach (var parameter in parameters)
            {
                var value = await valueProvider.ReceiveValueAsync(parameter);
            }

            return null;
        }


    }

    public interface ITypeValueProvider
    {
        public object GetValue(Type type);

        public UniTask<object> ReceiveValueAsync(Type type);
    }
}