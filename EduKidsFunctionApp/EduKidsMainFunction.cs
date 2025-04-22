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
using Microsoft.Azure.WebJobs.Host;
using Twilio.TwiML;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Web;
using Twilio.TwiML.Messaging;
using System.Net.Mail;


//using Twilio.TwiML.Voice;

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
        string? CompanyName = Environment.GetEnvironmentVariable("CompanyName");

        public EduKidsMainFunction(ILogger<EduKidsMainFunction> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
            TwilioClient.Init(accountSid, authToken);


        }
     //   public void Run([TimerTrigger("0 */15 * * * *")] TimerInfo myTimer, ILogger log)

        [Function("KeepWarms")]
        public async Task<String> KeepWarms([Microsoft.Azure.Functions.Worker.TimerTrigger("0 */15 * * * *")] Microsoft.Azure.Functions.Worker.TimerInfo myTimer)
   //     [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            _logger.LogInformation($"Warming function at: {DateTime.Now}");

            try
            {


                // Fetch phone numbers from DB
                var contacts = await _dbContext.CustomerContacts
                                                .Where(c => c.Subscribed == true)
                                               .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending WhatsApp messages: {ex.Message}");
                return "Fail";

            }
            _logger.LogInformation($"Warming Success ");

            return "Success";
        }
        [Function("EduKidsMainFunction")]
        public async Task<String> Run([Microsoft.Azure.Functions.Worker.TimerTrigger("0 0 2 * * *")] Microsoft.Azure.Functions.Worker.TimerInfo myTimer)

        //public async Task<String> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            _logger.LogInformation($"Function triggered at: {DateTime.UtcNow}");

            /*executing above query two times to bring cold db to warm stage so that connection wont fail*/
            string jsonContentVariables="Success2";
            try
            {


                // Fetch phone numbers from DB
                var contacts = await _dbContext.CustomerContacts
                                                .Where(c => c.Subscribed == true)
                                               .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending WhatsApp messages: {ex.Message}");
            }

            try
            {


                // Fetch phone numbers from DB --test gg
                var contacts = await _dbContext.CustomerContacts
                                                .Where(c => c.Subscribed == true)
                                               .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending WhatsApp messages: {ex.Message}");
            }

            try
            {


                // Fetch phone numbers from DB
                var contacts = await _dbContext.CustomerContacts
                                                .Where(c => c.Subscribed == true)
                                               .ToListAsync();
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
                await _dbContext.Database.ExecuteSqlRawAsync("UPDATE Master.customerContacts SET RegistrationStep = 7 WHERE Subscribed = 1");
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
        public async Task<string> NewUserOnboarding(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req, FunctionContext executionContext)
        {
            _logger.LogInformation("WhatsApp message received");


            var content = await new StreamReader(req.Body).ReadToEndAsync();

            var parsed = HttpUtility.ParseQueryString(content);

            string? from = parsed["From"];
            string? messageBody = parsed["Body"];
            string fromNumber = from.Replace("whatsapp:", "");

            var user = await _dbContext.CustomerContacts.FirstOrDefaultAsync(u => u.Phone == fromNumber);

            if (user == null)
            {
                user = new CustomerContact { Phone = fromNumber, RegistrationStep = 1 };
                _dbContext.CustomerContacts.Add(user);
                await _dbContext.SaveChangesAsync();
                return TwilioResponse($"Welcome to {CompanyName}, bite sized english learning program! To enroll, Please enter your learner/child's name?");
            }

            return await HandleUserResponse(user, messageBody);

        }

        private async Task<string> HandleUserResponse(CustomerContact user, string message)
        {
            string reply = "test";
            if (message.ToLower() == "stop")
            {
                user.Subscribed = false;
                reply = $"You have been unsubscribed from {CompanyName}. Reply 'start' to resubscribe.";
            }
            else if (message.ToLower() == "start" && user.Subscribed == false)
            {
                user.Subscribed = true;
                reply = $"Welcome back to {CompanyName}!";
            }
            else
            {
                switch (user.RegistrationStep)
                {
                    case 1:
                        user.UserName = message;
                        user.RegistrationStep = 2;
                        reply = "Great! How old is your child in years?";
                        break;

                    case 2:
                        if(int.TryParse(message, out _) == false) //check if the message is a number
                        {
                            reply = "Please enter a valid age in years. Ex: 9";
                            break;
                        }
                        int age = int.Parse(message);
                        user.DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-age));
                        user.RegistrationStep = 3;
                        reply = "Got it! What's your name as the parent?";
                        break;

                    case 3:
                        user.FatherName = message;
                        user.RegistrationStep = 4;
                        reply = "Great! Whats your email id for communication?";
                        break;

                    case 4:
                        if (!IsValidEmail(message))
                        {
                            reply = "Please enter a valid email address!";
                            break;
                        }
                        user.Email = message;
                        user.RegistrationStep = 5;
                        reply = "Which city you are from?";
                        break;

                    case 5:
                        user.RegistrationStep =  6;
                        user.City = message;
                        reply = "Which state you are from?";
                        break;

                    case 6:
                        user.RegistrationStep = 7;
                        user.States = message;
                        user.Subscribed = true;
                        reply = $"Thanks! You're now subscribed to {CompanyName} daily learning messages. Please feel free to send any suggestions here. Send STOP to stop receiving words!";
                        break;

                    /* gts
                    case 7:
                        user.RegistrationStep = 8;
                        reply = "Thank you for Message!";
                        break;
                    */
                    default:
                        var newMessage = new CustomerMessage
                        {
                            Phone = user.Phone,
                            Message = message
                        };
                        _dbContext.CustomerMessages.Add(newMessage);
                        reply = "Thank you for Message!";
                        break;
                }
            }
            await _dbContext.SaveChangesAsync();
            return TwilioResponse( reply);
        }

        private string TwilioResponse(string message)
        {
            return message;
            
        }

        static bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);  // Try to create a MailAddress object
                return true;  // If it succeeds, the email is valid
            }
            catch (FormatException)
            {
                return false;  // If it throws a FormatException, the email is not valid
            }
        }

    }
    
}
