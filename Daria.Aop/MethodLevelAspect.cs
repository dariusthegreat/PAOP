using System;
using System.Reflection;

namespace Daria.Aop
{
    [Serializable]
    public abstract class MethodLevelAspect : Aspect
    {
        public override bool CompileTimeValidate(object target) { return true; }
        
        public virtual void CompileTimeInitialize(MethodBase method) {}

        public virtual bool CompileTimeValidate(MethodBase method)    {return true;}
        
        public virtual void RuntimeInitialize(MethodBase method) {}
    }
}