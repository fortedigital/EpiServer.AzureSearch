using System;

namespace Forte.EpiServer.AzureSearch.Exceptions
{
    public class ScheduledJobFailedException : Exception
    {
        public ScheduledJobFailedException(string message) : base(message)
        {
        }
    }
}
