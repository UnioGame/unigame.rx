using System;

namespace UniModules.UniGame.Core.Runtime.Rx
{
    [Serializable]
    public class IntReactiveValue : ReactiveValue<int> {}

    [Serializable]
    public class FloatReactiveValue : ReactiveValue<float> {}

    [Serializable]
    public class BoolReactiveValue : ReactiveValue<bool>
    {
        public BoolReactiveValue() : base()
        {
        }

        public BoolReactiveValue(bool defaultValue) : base(defaultValue)
        {
            
        }
    }
    
    [Serializable]
    public class StringReactiveValue : ReactiveValue<string> {}
    
    [Serializable]
    public class DoubleReactiveValue : ReactiveValue<double> {}
    
    [Serializable]
    public class ByteReactiveValue : ReactiveValue<byte> {}
}

