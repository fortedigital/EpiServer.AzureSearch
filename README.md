# Forte EpiServer Azure Search

This code ads search functionality to your EpiServer project based on Azure Cognitive Search service

## Configuration

Once you create Azure Cognitive Search service you'll get service name and admin keys. 
You need to define those two in your appSettings:

**web.config**

```xml
 <appSettings>
    <add key="AzureSearchService:Name" value="yourservicename" />
    <add key="AzureSearchService:ApiKey" value="YOURADMINKEY" />
  </appSettings>
``` 

NOTE: service name is without _search.windows.net_ suffix.

## Initialization

For basic search initialization it's enough to create class like this:

```c#
using System.Configuration;
using EPiServer.Framework;
using Forte.EpiServer.AzureSearch.Configuration;

namespace AlloyDemoKit.ForteSearch
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class ForteSearchInitializationModule : DefaultAzureSearchServiceInitializationModule
    {
        protected override AzureSearchServiceConfiguration GetSearchServiceConfiguration()
        {
            var serviceName = ConfigurationManager.AppSettings["AzureSearchService:Name"];
            var apiKey = ConfigurationManager.AppSettings["AzureSearchService:ApiKey"];
            
            return new AzureSearchServiceConfiguration(serviceName,apiKey);
        }
    }
}
```

This will add scheduled job to your EpiServer installation which is responsible for full index of content 
and will register event handlers so content is updated on publish events. 

## Search providers

 This package allows to configure EpiServer built in search features (in admin panel) to make use of Azure Search Index. 
 
 Basic usage of this is to add class in your code which you will mark with `[SearchProvider]` attribute and extend `ContentSearchProviderBase` class.
 
 ```c#
[SearchProvider]
public class PageSearchProvider : ContentSearchProviderBase<ContentDocument>
{
    public PageSearchProvider(IAzureSearchService azureSearchService, IContentLanguageAccessor contentLanguageAccessor) : base(azureSearchService, contentLanguageAccessor)
    {
    }

    public override string Area => "CMS/pages";

    public override string Category => "Find pages";
}
```

You can adjust `Area` and `Category` values to your needs - depending which search (pages/blocks/media) it should be attached to 
## Custom document model

It is highly probable that you'll want to extend built in `ContentDocument` class. You'll have to do it in case when, for example, you want to add new field to be indexed. This is how you do it:

Let's define new model. Only restriction here is that it inherits from `SearchDocument` but in this example we'll extend built in `ContentDocument`:

```c#

public class MyCustomDocument : ContentDocument
{
    public int[] Categories { get; set; }
}
```

Now, we have to create logic which will be responsible for converting your EpiServer content do your new document type `MyCustomDocument`. 

In order to do so, we'll create `MyCustomDocumentBuilder` class:

```c#
public class MyCustomDocumentBuilder : DefaultDocumentBuilder<MyCustomDocument>
{
    public MyCustomeDocumentBuilder(IUrlResolver urlResolver, 
        IContentLoader contentLoader, 
        IContentExtractorController contentExtractorController,
        IContentTypeRepository contentTypeRepository)
        : base(urlResolver, contentLoader, contentExtractorController, contentTypeRepository)
    {
    }

    public override MyCustomContentDocument Build(IContent content)
    {
        var document = base.Build(content);

        if (content is PageData pageData)
        {
            document.Categories = pageData.Category.ToArray();
        }

        return document;
    }

    public override MyCustomDocument Build(PageData pageData)
    {
        return Build(pageData as IContent);
    }
}
``` 

Now, we have to setup search to make use of new document and document builder. In order to do so, our `ForteSearchInitializationModule` won't inherit from `DefaultAzureSearchServiceInitializationModule` but rather:

```c#
[InitializableModule]
[ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
public class ForteSearchInitializationModule : AzureSearchServiceInitializationModule<MyCustomDocument, MyCustomDocumentBuilder>
{
    /// AzureSearchServiceConfiguration GetSearchServiceConfiguration() goes here as before
 
}
```

## Extracting content

Content document has this field:

```c#
[IsSearchable]
public string[] ContentBody { get; set; }
```

It is meant to store all content which is supposed to be searchable - article body, intro, section page summary etc.

By default, if you don't do anything, no content will be extracted there. There are two options you can do it:

### IndexableAttribute

You can mark any field with this attribute and its content will be added as a content to `ContentBody` field.

```c#
public class ArticlePage : PageData
{
    [Forte.EpiServer.AzureSearch.Model.Indexable]
    public virtual XhtmlString Body { get; set; }

    [Forte.EpiServer.AzureSearch.Model.Indexable]
    public virtual string Summary { get; set; }
}
```

This attribute does further extraction for properties of types `XhtmlString`, 'ContentArea', any block type (derivative of BlockData) and 'ContentReference'(as long as it's referencing block and not page). On all other field types, `ToString()` will be called. If you want to do something more sophisticated, like order content extraction texts results, use `IContentExtractor` instead.

### IContentExtractor

You can create class which will implement `IContentExtractor` and decide what and when should end up in `ContentBody`. Inject and use 'Forte.EpiServer.AzureSearch.ContentExtractor.XhtmlStringExtractor' for extracting plain text from Xhtml based properties types (XHtmlString and ContentArea):

```c#
public class ArticlePageContentExtractor : IContentExtractor
{
    private readonly XhtmlStringExtractor _xhtmlStringExtractor;
    public ArticlePageContentExtractor(XhtmlStringExtractor xhtmlStringExtractor)
    {
        _xhtmlStringExtractor = xhtmlStringExtractor;
    }

    public bool CanExtract(IContentData content)
    {
        return content is ArticlePageBase;
    }

    public ContentExtractionResult Extract(IContent content, ContentExtractorController extractor)
    {
        var article = (ArticlePageBase) content;

        var articleBody = _xhtmlStringExtractor.GetPlainTextContent(article.Body, extractor);
        var articleIntro = _xhtmlStringExtractor.GetPlainTextContent(article.Intro, extractor);
        var articleHeading = article.Heading;
        
        return new ContentExtractionResult(new[] {articleBody, articleIntro, articleHeading}, null);
    }
}
```

Note, that once created, such class has to be registered in your StructureMap, for example here:

```c#
[InitializableModule]
[ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
public class ForteSearchInitializationModule : AzureSearchServiceInitializationModule<MyCustomDocument, MyCustomDocumentBuilder>
{
    /// AzureSearchServiceConfiguration GetSearchServiceConfiguration() goes here as before
    public override void ConfigureContainer(ServiceConfigurationContext context)
    {
        base.ConfigureContainer(context);
         
        context.StructureMap().Configure(c =>
        {
            c.Scan(a =>
            {
                a.TheCallingAssembly();
                a.AddAllTypesOf<IContentExtractor>();
            });
        });

    }
}
```
