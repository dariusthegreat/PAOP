using System;

namespace Daria.Aop
{
    [Serializable]
    public class OnMethodBoundaryAspect : MethodLevelAspect
    {
        public virtual void OnEntry(MethodExecutionArgs args)
        {
            
        }
        
        public virtual void OnSuccess(MethodExecutionArgs args)
        {
            
        }
        
        public virtual void OnException(MethodExecutionArgs args)
        {
            
        }
        
        public virtual void OnExit(MethodExecutionArgs args)
        {
            
        }
    }
}