using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LatokenMauiClient
{
    [Serializable]
    public partial class Profile : ObservableObject
    {
        [ObservableProperty]
        private string profileName;

        [ObservableProperty]
        private string apiSecret;

        [ObservableProperty]
        private string apiKey;
    }
}
