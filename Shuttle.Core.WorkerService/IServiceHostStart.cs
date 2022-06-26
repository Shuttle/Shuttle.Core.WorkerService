using System;

namespace Shuttle.Core.WorkerService
{
    public interface IServiceHostStart
    {
        void Start(IServiceProvider services);
    }
}