using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Util.Store;

namespace PubSubExercise.Authentication;
public static class CredentialsProvider
{
	private const string credentialsEnvVar = "PUBSUB_EXERSICE_CREDENTIALS";
	
	public static async Task<UserCredential> GetUserCredentialsAsync()
	{
		var credentialsFilePath = Environment.GetEnvironmentVariable(credentialsEnvVar)
			?? throw new InvalidOperationException($"{credentialsEnvVar} environment variable is not set.");
	
		UserCredential credential;
        using var stream = new FileStream(credentialsFilePath, FileMode.Open, FileAccess.Read);
        string credPath = "token.json";
        credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            clientSecrets: GoogleClientSecrets.FromStream(stream).Secrets,
            scopes: [GmailService.Scope.GmailReadonly],
            user: "user",
            taskCancellationToken: CancellationToken.None,
            dataStore: new FileDataStore(credPath, true));
			
		return credential;
    }
}
