using EduKidsFunctionApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


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
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            _logger.LogInformation("Fetching user contacts from the database.");

            var customerContacts = await Task.Run(() => _dbContext.CustomerContacts.ToList());
            var wordsBank = await Task.Run(() => _dbContext.WordsBanks.ToList());

            return new OkObjectResult(wordsBank);
        }
    }
}
