using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LatokenMauiClient
{
    public class ProfilePageNavigatedToMessage : ValueChangedMessage<string>
    {
        public ProfilePageNavigatedToMessage(string selectedProfile) : base(selectedProfile)
        {
        }
    }

}