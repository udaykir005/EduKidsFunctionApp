using EduKidsFunctionApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Twilio.TwiML;


namespace EduKidsFunctionApp
{
    public class EduKidsMainFunction
    {
        private readonly ILogger<EduKidsMainFunction> _logger;
        private readonly AppDbContext _dbContext;
        // Initialize Twilio
        string? accountSid = Environment.GetEnvironmentVariable("TwilioAccountSid");
        string? authToken = Environment.GetEnvironmentVariable("TwilioAuthToken");
        string? twilioNumber = Environment.GetEnvironmentVariable("TwilioWhatsAppNumber");
        string? contentSid = Environment.GetEnvironmentVariable("TwilioContentSid_edukids_words_q1");
        string? getConsentcontentSid = Environment.GetEnvironmentVariable("TwilioContentSid_getconsent");

        public EduKidsMainFunction(ILogger<EduKidsMainFunction> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;

        }

        [Function("EduKidsMainFunction")]
        public async Task<String> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            _logger.LogInformation($"Function triggered at: {DateTime.UtcNow}");
            string jsonContentVariables="Success2";
            try
            {

                TwilioClient.Init(accountSid, authToken);

                // Fetch phone numbers from DB
                var contacts = await _dbContext.CustomerContacts
                                               // .Where(c => c.ContactId == 1)
                                               .ToListAsync();
                //return contacts.Count.ToString();
                // Fetch 3 random words from DB
                var words = await _dbContext.WordsBanks
                    .OrderBy(r => Guid.NewGuid())  // Random order
                    .Take(3)
                    .ToListAsync();

                if (words.Count < 3)
                {
                    _logger.LogError("Not enough words in the database.");
                    return "Fail";
                }

                // Format date
                var formattedDate = DateTime.UtcNow.ToString("MMM dd, yyyy");

                // Prepare messages
                var tasks = new List<Task<MessageResource>>();

                foreach (var contact in contacts)
                {


                    var contentVariables = new 
                    {
                        _1 = contact.UserName,
                        _2 = formattedDate,
                        _3 = $"*{char.ToUpper(words[0].Word[0]) + words[0].Word.Substring(1).ToLower()}*",
                        _4 = $"  _{words[0].Grammar}_",

                        _5 = words[0].Meaning,
                        _6 = words[0].ExampleUsage,
                        _7 = $"*{char.ToUpper(words[1].Word[0]) + words[1].Word.Substring(1).ToLower()}*",
                        _8 = $"  _{words[1].Grammar}_",
                        _9 = words[1].Meaning,
                        _10 = words[1].ExampleUsage,

                        _11 = $"*{char.ToUpper(words[2].Word[0]) + words[2].Word.Substring(1).ToLower()}*",
                        _12 = $"  _{words[2].Grammar}_",
                        _13 = words[2].Meaning,
                        _14 = words[2].ExampleUsage


                    };
                     jsonContentVariables = System.Text.Json.JsonSerializer.Serialize(contentVariables).Replace("_","");

                    _logger.LogInformation($"jsonContentVariables!:{jsonContentVariables}");

                    tasks.Add(MessageResource.CreateAsync(
                        contentSid: contentSid,
                        from: new Twilio.Types.PhoneNumber($"whatsapp:{twilioNumber}"),
                        to: new Twilio.Types.PhoneNumber($"whatsapp:{contact.Phone}"),
                        contentVariables: jsonContentVariables
                    ));
                }

               
                
                await Task.WhenAll(tasks);
                _logger.LogInformation("WhatsApp messages sent successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending WhatsApp messages: {ex.Message}");
            }
            return jsonContentVariables;

        }


        [Function("GetUserContinuationConsent")]
        public async Task<String> UserContinuationConsent(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            _logger.LogInformation($"Function triggered at: {DateTime.UtcNow}");
            string jsonContentVariables = "Success2";
            try
            {
             
                TwilioClient.Init(accountSid, authToken);

                // Fetch phone numbers from DB
                var contacts = await _dbContext.CustomerContacts
                                              //  .Where(c => c.ContactId == 1)
                                               .ToListAsync();
                //return contacts.Count.ToString();
               

        

                // Prepare messages
                var tasks = new List<Task<MessageResource>>();

                foreach (var contact in contacts)
                {


                    
                    tasks.Add(MessageResource.CreateAsync(
                        contentSid: getConsentcontentSid,
                        from: new Twilio.Types.PhoneNumber($"whatsapp:{twilioNumber}"),
                        to: new Twilio.Types.PhoneNumber($"whatsapp:{contact.Phone}")
                        
                    ));
                }



                await Task.WhenAll(tasks);
                _logger.LogInformation("WhatsApp messages sent successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending WhatsApp messages: {ex.Message}");
            }
            return jsonContentVariables;

        }


        [Function("ReceiveWhatsAppMessage")]
        public async Task<IActionResult> NewUserOnboarding(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("WhatsApp message received");
            var formData = await req.ReadFormAsync();
            string fromNumber = "33"; // formData["From"];
            string messageBody = "tset";//formData["Body"]?.ToLower().Trim();

            /***/

            try
            {

                TwilioClient.Init(accountSid, authToken);

                // Fetch phone numbers from DB
                var contacts = await _dbContext.CustomerContacts
                                                .Where(c => c.ContactId == 1)
                                               .ToListAsync();
                //return contacts.Count.ToString();




                // Prepare messages
                var tasks = new List<Task<MessageResource>>();

                foreach (var contact in contacts)
                {



                    tasks.Add(MessageResource.CreateAsync(
                        contentSid: getConsentcontentSid,
                        from: new Twilio.Types.PhoneNumber($"whatsapp:{twilioNumber}"),
                        to: new Twilio.Types.PhoneNumber($"whatsapp:{contact.Phone}")

                    ));
                }



                await Task.WhenAll(tasks);
                _logger.LogInformation("WhatsApp messages sent successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending WhatsApp messages: {ex.Message}");
            }



            /***/
            var user = await _dbContext.CustomerContacts.FirstOrDefaultAsync(u => u.Phone == fromNumber);

            if (user == null)
            {
                user = new CustomerContact { Phone = fromNumber, RegistrationStep = 1 };
                _dbContext.CustomerContacts.Add(user);
                await _dbContext.SaveChangesAsync();
                return TwilioResponse("Welcome to EduKids! What's your child's name?");
            }

            return await HandleUserResponse(user, messageBody);
        }

        private async Task<IActionResult> HandleUserResponse(CustomerContact user, string message)
        {
            string reply = "test";

            switch (user.RegistrationStep)
            {
                case 1:
                    user.UserName = message;
                    user.RegistrationStep = 2;
                    reply = "Great! How old is your child?";
                    break;

                case 2:
                    //  user.DateOfBirth = message;
                    user.RegistrationStep = 3;
                    reply = "Got it! What's your name as the parent?";
                    break;

                case 3:
                    user.FatherName = message;
                    user.RegistrationStep = 4;
                    user.Subscribed = true;
                    reply = "Thanks! You're now subscribed to EduKids daily learning messages.";
                    break;

                default:
                    reply = "You're already subscribed! Stay tuned for daily words.";
                    break;
            }

            await _dbContext.SaveChangesAsync();
            return TwilioResponse(reply);
        }

        private static IActionResult TwilioResponse(string message)
        {
            var messagingResponse = new MessagingResponse();
            messagingResponse.Message(message);
            return new ContentResult
            {
                Content = messagingResponse.ToString(),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

    }
    
}
