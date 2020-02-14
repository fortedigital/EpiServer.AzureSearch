using System.ComponentModel.DataAnnotations;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using TedGustaf.Episerver.GoogleMapsEditor.Shell;

namespace AlloyDemoKit.Models.Pages
{
    [ContentType(DisplayName = "Location Page", GUID = "894ae55c-880c-4e3b-a776-3db206eb86d2", Description = "A page displaying a location", GroupName = Global.GroupNames.Specialized)]
    [SiteImageUrl(Global.StaticGraphicsFolderPath + "page-type-thumbnail-standard.png")]
    public class LocationPage : StandardPage
    {
        [Display(GroupName = Global.GroupNames.Location, Name = "Location Name",
            Order =1)]
        public virtual string LocationName { get; set; }

        [Display(GroupName = Global.GroupNames.Location,
                    Order = 2
                    )]
        public virtual string Address { get; set; }

        [Display(
            GroupName = Global.GroupNames.Location,
            Order = 3)]
        public virtual string PostCode { get; set; }


        [Display(   Name = "Select Location",
                    GroupName = Global.GroupNames.Location,
                    Order = 5)]
        [UIHint(GoogleMapsEditorDescriptor.UIHint)]
        public virtual string GoogleLocation { get; set; }

        [Display(
            GroupName = Global.GroupNames.Location,
            Order = 4)]
        public virtual string Country { get; set; }
    }
}
