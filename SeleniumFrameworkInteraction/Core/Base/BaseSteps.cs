using Core.DI;
using Microsoft.Extensions.Logging;

namespace Core.Base
{
    public abstract class BaseSteps
    {
        protected ILogger Logger { get; }
        protected BaseSteps()
        {
            Logger = ServiceLocator.GetService<ILoggerFactory>().CreateLogger(GetType().Name);
        }
    }
}
