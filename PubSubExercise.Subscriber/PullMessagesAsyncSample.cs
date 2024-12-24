using System.Text.Json;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Cloud.PubSub.V1;
using PubSubExercise.Authentication;

namespace PubSubExercise.PubSubSubscriber;
public class PullMessagesAsyncSample
{
    private Task<GmailService> _gmailServiceTask;
    
    public PullMessagesAsyncSample()
    {
        _gmailServiceTask = Task.Run(async () =>
        {
            var credential = await CredentialsProvider.GetUserCredentialsAsync();
            return new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "PubSubSubscriber",
            });
        });
    }
    
    public async Task<int> PullMessagesAsync(string projectId, string subscriptionId, bool acknowledge)
    {
        SubscriptionName subscriptionName = SubscriptionName.FromProjectSubscription(projectId, subscriptionId);
        SubscriberClient subscriber = await SubscriberClient.CreateAsync(subscriptionName);
        // SubscriberClient runs your message handle function on multiple
        // threads to maximize throughput.
        int messageCount = 0;
        Task startTask = subscriber.StartAsync(async (PubsubMessage message, CancellationToken cancel) =>
        {
            string text = message.Data.ToStringUtf8();
            Console.WriteLine($"Message {message.MessageId}: {text}");
            Interlocked.Increment(ref messageCount);
            
            // Extract historyId from the JSON object
            var jsonDoc = JsonDocument.Parse(text);
            if (jsonDoc.RootElement.TryGetProperty("historyId", out JsonElement historyIdElement))
            {
                string historyId = historyIdElement.GetInt32().ToString();
                // Call the separate method to fetch email content
                await FetchEmailAsync(historyId);
            }
            else
            {
                Console.WriteLine("historyId not found in the message data.");
            }
            
            await FetchEmailAsync(text);
            
            return acknowledge ? SubscriberClient.Reply.Ack : SubscriberClient.Reply.Nack;
        });
        // Run for 5 seconds.
        await Task.Delay(5000);
        await subscriber.StopAsync(CancellationToken.None);
        // Lets make sure that the start task finished successfully after the call to stop.
        await startTask;
        return messageCount;
    }
    
    public async Task FetchEmailAsync(string historyId)
    {
        try
        {
            var gmailService = await _gmailServiceTask;
        
            // Fetch the history of changes using the history ID
            var request = gmailService.Users.History.List("me");
            request.StartHistoryId = ulong.Parse(historyId);
            request.LabelId = "INBOX"; // Optional: filter by label
        
            var response = await request.ExecuteAsync();
            if (response.History == null)
            {
                Console.WriteLine("No new emails found.");
                return;
            }
        
            foreach (var history in response.History)
            {
                foreach (var addedMessage in history.MessagesAdded)
                {
                    var message = await gmailService.Users.Messages.Get("me", addedMessage.Message.Id).ExecuteAsync();
                    var subject = message.Payload?.Headers?.FirstOrDefault(h => h.Name == "Subject")?.Value;
                    Console.WriteLine($"New Email: {subject}");
                    
                    // Extract the email content
                    string emailContent = GetEmailContent(message.Payload);
                    Console.WriteLine($"Email Content: {emailContent}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while fetching email: {ex.Message}");
        }
    }
    
    private string GetEmailContent(MessagePart payload)
    {
        if (payload == null) return string.Empty;

        if (!string.IsNullOrEmpty(payload.Body?.Data))
        {
            return Base64UrlDecode(payload.Body.Data);
        }

        if (payload.Parts != null && payload.Parts.Count > 0)
        {
            foreach (var part in payload.Parts)
            {
                string result = GetEmailContent(part);
                if (!string.IsNullOrEmpty(result))
                    return result;
            }
        }
    
        return string.Empty;
    }

    private string Base64UrlDecode(string input)
    {
        string s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        var bytes = Convert.FromBase64String(s);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}