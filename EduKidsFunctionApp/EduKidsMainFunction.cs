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


namespace EduKidsFunctionApp
{
    public class EduKidsMainFunction
    {
        private readonly ILogger<EduKidsMainFunction> _logger;
        private readonly AppDbContext _dbContext;

        public EduKidsMainFunction(ILogger<EduKidsMainFunction> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;


        }

        [Function("EduKidsMainFunction")]
        public async Task<String> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            //_logger.LogInformation("Fetching user contacts from the database.");

            //var customerContacts = await Task.Run(() => _dbContext.CustomerContacts.ToList());
            //var wordsBank = await Task.Run(() => _dbContext.WordsBanks.ToList());

            //return new OkObjectResult(wordsBank);
            _logger.LogInformation($"Function triggered at: {DateTime.UtcNow}");

            try
            {
                // Initialize Twilio
                string? accountSid = Environment.GetEnvironmentVariable("TwilioAccountSid");
                string? authToken = Environment.GetEnvironmentVariable("TwilioAuthToken");
                string? twilioNumber = Environment.GetEnvironmentVariable("TwilioWhatsAppNumber");
                string? contentSid = Environment.GetEnvironmentVariable("TwilioContentSid_1");

                TwilioClient.Init(accountSid, authToken);

                // Fetch phone numbers from DB
                var contacts = await _dbContext.CustomerContacts.ToListAsync();

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
                    string jsonContentVariables = System.Text.Json.JsonSerializer.Serialize(contentVariables).Replace("_","");


                    tasks.Add(MessageResource.CreateAsync(
                        contentSid: contentSid,
                        from: new Twilio.Types.PhoneNumber($"whatsapp:{twilioNumber}"),
                        to: new Twilio.Types.PhoneNumber($"whatsapp:{contact.Phone}"),
                        contentVariables: jsonContentVariables
                    ));
                }

                /*
                         const contentVariables = JSON.stringify({
            "1": contact.name,

            "2": formattedDate, //contact.name,
            
          //  "3": `*${word1.word}*`,
            "3": `*${word1.word.charAt(0).toUpperCase() + word1.word.slice(1).toLowerCase()}*`,
            "4": `  _${word1.PartofSpeech}_`,
            "5": word1.meaning,
            "6": word1Example,

            "7": `*${word2.word.charAt(0).toUpperCase() + word2.word.slice(1).toLowerCase()}*`,
            "8": `  _${word2.PartofSpeech}_`,
            "9": word2.meaning,
            "10": word2Example,
            

            "11":`*${word3.word.charAt(0).toUpperCase() + word3.word.slice(1).toLowerCase()}*`,
            "12": `  _${word3.PartofSpeech}_`,
            "13": word3.meaning,
            "14": word3Example,
            
        });
                */
                await Task.WhenAll(tasks);
                _logger.LogInformation("WhatsApp messages sent successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending WhatsApp messages: {ex.Message}");
            }
            return "Success";

        }
    }
    
}
