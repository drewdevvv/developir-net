using RestSharp;
using RestSharp.Authenticators;

namespace Developir.Web.Services;

public class EmailService
{
    private readonly string _apiKey;
    private readonly string _from;
    private readonly string _adminEmail;
    private readonly string _domain;
    private readonly RestClient _client;
    
    public EmailService(IConfiguration configuration)
    {
        _apiKey = configuration.GetValue<string>("EmailService:ApiKey") ?? 
            throw new ArgumentNullException("EmailService:ApiKey");
        _from = configuration.GetValue<string>("EmailService:From") ?? 
            throw new ArgumentNullException("EmailService:From");
        _adminEmail = configuration.GetValue<string>("EmailService:To") ?? 
            throw new ArgumentNullException("EmailService:To");
        _domain = configuration.GetValue<string>("EmailService:MailgunDomain") ?? 
            throw new ArgumentNullException("EmailService:MailgunDomain");
            
        var options = new RestClientOptions("https://api.mailgun.net/v3")
        {
            Authenticator = new HttpBasicAuthenticator("api", _apiKey)
        };
        _client = new RestClient(options);
    }
    
    public async Task SendContactEmail(string name, string email, string organization, string message)
    {
        try
        {
            // Send notification to admin
            var adminRequest = new RestRequest($"{_domain}/messages", Method.Post);
            adminRequest.AddParameter("from", $"{name} <{email}>");  // Use the form submitter's email
            adminRequest.AddParameter("to", _adminEmail);
            adminRequest.AddParameter("subject", $"New Contact Form Submission from {organization}");
            adminRequest.AddParameter("text", $"Name: {name}\nEmail: {email}\nOrganization: {organization}\nMessage: {message}");
            
            var adminResponse = await _client.ExecuteAsync(adminRequest);
            if (!adminResponse.IsSuccessful)
                throw new Exception($"Failed to send admin notification: {adminResponse.ErrorMessage}");
            
            // Send auto-response to the submitter
            var autoResponse = new RestRequest($"{_domain}/messages", Method.Post);
            autoResponse.AddParameter("from", $"Developir <{_from}>");
            autoResponse.AddParameter("to", email);  // Use the form submitter's email
            autoResponse.AddParameter("reply-to", _adminEmail);  // Set reply-to as admin email
            autoResponse.AddParameter("subject", "Thank you for contacting Developir");
            autoResponse.AddParameter("text", GetAutoResponseMessage(name));
            
            var userResponse = await _client.ExecuteAsync(autoResponse);
            if (!userResponse.IsSuccessful)
                throw new Exception($"Failed to send auto-response: {userResponse.ErrorMessage}");
        }
        catch (Exception ex)
        {
            // Log the exception details here
            throw new Exception("Failed to send email", ex);
        }
    }
    
    private string GetAutoResponseMessage(string name)
    {
        return $@"Dear {name},

Thank you for reaching out to Developir! We have received your message and appreciate you taking the time to contact us.

Our team will review your message and get back to you as soon as possible. Usually, we respond within 1-2 business days.

Note: This is an automated response. Please don't reply to this email directly.

Best regards,
The Developir Team";
    }
}