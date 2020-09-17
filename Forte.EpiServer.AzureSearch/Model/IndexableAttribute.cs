using System;

namespace Forte.EpiServer.AzureSearch.Model
{
  [AttributeUsage(AttributeTargets.Property)]
  public class IndexableAttribute : Attribute
  {
    public bool IsIndexable { get; private set; }

    public IndexableAttribute() => this.IsIndexable = true;

    public IndexableAttribute(bool isIndexable) => this.IsIndexable = isIndexable;
  }
}
