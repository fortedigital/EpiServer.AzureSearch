namespace Forte.EpiServer.AzureSearch.Plugin
{
    public class UpdateOrRecreateResult
    {
        public UpdateOrRecreateResult(UpdateOrRecreateResultEnum type, string recreationReason)
        {
            Type = type;
            RecreationReason = recreationReason;
        }

        public UpdateOrRecreateResultEnum Type { get; }
        public string RecreationReason { get; }
    }
}