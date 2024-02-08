using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LatokenMauiClient
{
    public class SelectedProfileChangedMessage : ValueChangedMessage<string>
    {
        public SelectedProfileChangedMessage(string selectedProfile) : base(selectedProfile)
        {
        }
    }

}