using System;

namespace Daria.Aop
{
    [Serializable]
    public abstract class Aspect : Attribute
    {
        public abstract bool CompileTimeValidate(object target);
    }
}