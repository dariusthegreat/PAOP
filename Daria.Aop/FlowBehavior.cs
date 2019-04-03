namespace Daria.Aop
{
    public enum FlowBehavior
    {
        Default = 0,
        Continue = 1,
        RethrowException = 2,
        Return = 3,
        ThrowException = 4,
        Yield = 5
    }
}