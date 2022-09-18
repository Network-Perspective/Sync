using System;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Application.Extensions
{
    public static class ExceptionExtensions
    {
        public static bool IndicatesTaskCanceled(this Exception ex)
        {
            if (ex is TaskCanceledException)
                return true;

            if (ex.InnerException is null)
                return false;

            if (ex.InnerException is TaskCanceledException)
                return true;

            return IndicatesTaskCanceled(ex.InnerException);
        }
    }
}