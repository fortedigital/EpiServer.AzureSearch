using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Forte.EpiServer.AzureSearch.ContentExtractor;
using Forte.EpiServer.AzureSearch.Events;
using Forte.EpiServer.AzureSearch.Model;
using Forte.EpiServer.AzureSearch.Plugin;

namespace Forte.EpiServer.AzureSearch.Configuration
{
    public abstract class AzureSearchServiceInitializationModule<TDocument, TDocumentBuilder> : IConfigurableModule where TDocument : ContentDocument where TDocumentBuilder : IContentDocumentBuilder<TDocument>
    {
        protected abstract AzureSearchServiceConfiguration GetSearchServiceConfiguration();
        
        public virtual void Initialize(InitializationEngine context)
        {
            var contentEvents = context.Locate.Advanced.GetInstance<IContentEvents>();
            var azureSearchService = context.Locate.Advanced.GetInstance<IAzureSearchService>();
            var searchEventHandler = context.Locate.Advanced.GetInstance<SearchEventHandler<TDocument>>();
            var indexSpecificationProvider = context.Locate.Advanced.GetInstance<IIndexSpecificationProvider>();
            
            context.InitComplete += (sender, args) =>
            {
                contentEvents.PublishingContent += searchEventHandler.OnPublishingContent;
                contentEvents.PublishedContent += searchEventHandler.OnPublishedContent;
                contentEvents.MovedContent += searchEventHandler.OnMovedContent;
                contentEvents.SavingContent += searchEventHandler.OnSavingContent;
                contentEvents.DeletingContentLanguage += searchEventHandler.OnDeletingContentLanguage;

                Task.Run(() => azureSearchService.CreateOrUpdateIndexAsync<TDocument>(indexSpecificationProvider.GetIndexSpecification()))
                    .ContinueWith(t =>
                    {
                        var logger = LogManager.GetLogger();
                        logger.Error("Error when creating search index", t.Exception);
                    }, TaskContinuationOptions.OnlyOnFaulted);
            };
        }

        public virtual void Uninitialize(InitializationEngine context)
        {
            var searchEventHandler = context.Locate.Advanced.GetInstance<SearchEventHandler<TDocument>>();
            var contentEvents = context.Locate.Advanced.GetInstance<IContentEvents>();
            contentEvents.PublishedContent -= searchEventHandler.OnPublishedContent;
            contentEvents.MovedContent -= searchEventHandler.OnMovedContent;
            contentEvents.SavingContent -= searchEventHandler.OnSavingContent;
        }
        
        public virtual void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.StructureMap().Configure(c =>
            {
                c.For<AzureSearchServiceConfiguration>().Singleton().Use(() => GetSearchServiceConfiguration());
                c.For<DefaultDocumentBuilder>().Use<DefaultDocumentBuilder>();
                c.For<IContentExtractorController>().Use<ContentExtractorController>();
                c.For<IIndexNamingConvention>().Use<PrefixedIndexNamingConvention>();
                c.For<IAzureSearchService>()
                    .Singleton()
                    .Use(loader => new AzureSearchService(loader.GetInstance<AzureSearchServiceConfiguration>(), LogManager.GetLogger(this.GetType()), loader.GetInstance<IIndexNamingConvention>())); 
                c.For<IContentIndexer>().Use<ContentIndexer<TDocument>>();
                c.For<SearchEventHandler<TDocument>>().Singleton().Use<SearchEventHandler<TDocument>>();
                c.For<IContentDocumentBuilder<TDocument>>().Use<TDocumentBuilder>();
                c.For<IIndexSpecificationProvider>().Use<NullIndexSpecificationProvider>();
                c.For<IIndexDefinitionHandler>().Use<IndexDefinitionHandler<TDocument>>();
                c.For<IIndexGarbageCollector>().Use<IndexGarbageCollector<TDocument>>();
            });
        }
    }
}
