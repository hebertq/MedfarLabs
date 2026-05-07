using System;
using System.Reflection;
using MedfarLabs.Core.Domain.Interfaces.Repositories;

namespace MedFarLab.Pwa.Security
{
    public class PwaUnitOfWorkProxy : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod == null) return null;
            if (targetMethod.ReturnType == typeof(void)) return null;
            if (targetMethod.ReturnType.IsValueType) return Activator.CreateInstance(targetMethod.ReturnType);
            return null;
        }

        public static IUnitOfWork Create()
        {
            return DispatchProxy.Create<IUnitOfWork, PwaUnitOfWorkProxy>();
        }
    }
}
