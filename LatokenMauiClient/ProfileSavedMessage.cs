using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LatokenMauiClient
{
    public class ProfileSavedMessage : ValueChangedMessage<string>
    {
        public ProfileSavedMessage(string selectedProfile) : base(selectedProfile)
        {
        }
    }

}