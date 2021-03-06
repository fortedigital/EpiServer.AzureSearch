﻿using AlloyDemoKit.Models.Pages;
using EPiServer.Personalization;

namespace AlloyDemoKit.Models.ViewModels
{
    public class ProfilePageViewModel : PageViewModel<ProfilePage>
    {
        public ProfilePageViewModel() : base()
        {
            Profile = EPiServerProfile.Current;
        }

        public ProfilePageViewModel(ProfilePage currentPage)
            : base(currentPage)
        {
            Profile = EPiServerProfile.Current;
        }

        public EPiServerProfile Profile { get; set; }

        //public Boolean Impersonating { get; set; }
    }
}
