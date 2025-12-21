using FirebaseAdmin;

namespace PushSender.Services;

public interface IFirebaseAppProvider
{
    FirebaseApp? TryGetApp();
}