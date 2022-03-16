using Forte.EpiServer.AzureSearch;
using Forte.EpiServer.AzureSearch.Configuration;
using Forte.EpiServer.AzureSearch.ContentExtractor;
using Forte.EpiServer.AzureSearch.Events;
using Forte.EpiServer.AzureSearch.Indexes;
using Forte.EpiServer.AzureSearch.Model;
using Forte.EpiServer.AzureSearch.Plugin;
using Microsoft.AspNetCore.Builder;

// ReSharper disable CheckNamespace
// Having this namespace, the client of this code doesn't have to reference this namespace in startup file.
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEpiServerAzureSearch<TDocument, TDocumentBuilder>(this IServiceCollection services, string serviceName, string apiKey)
            where TDocument : ContentDocument
            where TDocumentBuilder : class, IContentDocumentBuilder<TDocument>
        {
            services.AddSingleton(new AzureSearchServiceOptions(serviceName, apiKey));
            services.AddSingleton<IAzureSearchService, AzureSearchService>();

            services.AddTransient<IContentExtractor, IndexableContentExtractor>();
            services.AddTransient<XhtmlStringExtractor>();
            services.AddTransient<AzureSearchIndexBootstrapper>();
            services.AddTransient<BackgroundAzureSearchIndexBootstrapper>();
            services.AddTransient<IContentExtractorController, ContentExtractorController>();
            services.AddTransient<IIndexNamingConvention, PrefixedIndexNamingConvention>();
            services.AddTransient<DefaultDocumentBuilder>();
            services.AddTransient<IIndexSpecificationProvider, NullIndexSpecificationProvider>();

            RegisterDocumentSpecificServices<TDocument, TDocumentBuilder>(services);

            return services;
        }

        private static void RegisterDocumentSpecificServices<TDocument, TDocumentBuilder>(IServiceCollection services)
            where TDocument : ContentDocument where TDocumentBuilder : class, IContentDocumentBuilder<TDocument>
        {
            services.AddSingleton<SearchEventHandler<TDocument>>();
            services.AddSingleton<EventsRegistry<TDocument>>();
            services.AddTransient<PageDocumentsProvider<TDocument>>();
            services.AddTransient<BlockDocumentsProvider<TDocument>>();
            services.AddTransient<IContentIndexer, ContentIndexer<TDocument>>();
            services.AddTransient<IContentDocumentBuilder<TDocument>, TDocumentBuilder>();
            services.AddTransient<IIndexDefinitionHandler, IndexDefinitionHandler<TDocument>>();
            services.AddTransient<IIndexGarbageCollector, IndexGarbageCollector<TDocument>>();
        }

        public static void UseEpiServerAzureSearch<TDocument>(this IApplicationBuilder app) where TDocument : ContentDocument
        {
            app.ApplicationServices.GetRequiredService<EventsRegistry<TDocument>>().RegisterEvents();
            app.ApplicationServices.GetRequiredService<BackgroundAzureSearchIndexBootstrapper>().CreateOrUpdateIndexAsync<TDocument>();
        }
    }
}
