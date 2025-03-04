using EPiServer.Core;
using EPiServer.DataAbstraction;
using Forte.EpiServer.AzureSearch.Events;
using Forte.EpiServer.AzureSearch.Model;

namespace Forte.EpiServer.AzureSearch
{
    public class EventsRegistry<TDocument>
        where TDocument : ContentDocument
    {
        private readonly IContentEvents _contentEvents;
        private readonly IContentSecurityEvents _contentSecurityEvents;
        private readonly SearchEventHandler<TDocument> _searchEventHandler;

        public EventsRegistry(
            IContentEvents contentEvents,
            IContentSecurityEvents contentSecurityEvents,
            SearchEventHandler<TDocument> searchEventHandler)
        {
            _contentEvents = contentEvents;
            _contentSecurityEvents = contentSecurityEvents;
            _searchEventHandler = searchEventHandler;
        }

        public void RegisterEvents()
        {
            RegisterContentEvents();
            RegisterContentSecurityEvents();
        }

        private void RegisterContentSecurityEvents()
        {
            _contentSecurityEvents.ContentSecuritySaved += _searchEventHandler.OnContentSecuritySaved;
        }

        private void RegisterContentEvents()
        {
            _contentEvents.PublishingContent += _searchEventHandler.OnPublishingContent;
            _contentEvents.PublishedContent += _searchEventHandler.OnPublishedContent;
            _contentEvents.MovingContent += _searchEventHandler.OnMovingContent;
            _contentEvents.MovedContent += _searchEventHandler.OnMovedContent;
            _contentEvents.SavingContent += _searchEventHandler.OnSavingContent;
            _contentEvents.DeletingContentLanguage += _searchEventHandler.OnDeletingContentLanguage;
        }
    }
}
