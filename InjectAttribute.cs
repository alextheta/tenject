using System;

namespace Te.DI
{
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class InjectAttribute : Attribute
    {
        public static readonly Type Type = typeof(InjectAttribute);
    }
}