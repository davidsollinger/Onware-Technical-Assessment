using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Data;

namespace health_path.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsletterController : ControllerBase
{
    private readonly ILogger<NewsletterController> _logger;
    private readonly IDbConnection _connection;

    public NewsletterController(ILogger<NewsletterController> logger, IDbConnection connection)
    {
        _logger = logger;
        _connection = connection;
    }

    [HttpPost]
    public ActionResult Subscribe(string Email)
    {
        Email = RemovePeriodFromEmail(Email);

        var inserted = _connection.Execute(@"
            INSERT INTO NewsletterSubscription (Email)
            SELECT *
            FROM ( VALUES (@Email) ) AS V(Email)
            WHERE NOT EXISTS ( SELECT * FROM NewsletterSubscription e WHERE e.Email = v.Email )
        ", new { Email = Email });

        return inserted == 0 ? Conflict("email is already subscribed") : Ok();
    }

    // Checks for . character before @ symbol in email
    // Splits email by @ symbol into 'email name' and '@domain'
    // Removes . character if found in 'email name'
    // Recombines and returns email
    private string RemovePeriodFromEmail(string email) {
        string pattern = @"(?<=[@])";
        string[] emailDetails = Regex.Split(email, pattern); //splits the email into the 'name' and the '@domain'
        if(emailDetails.Length > 0) {
            string emailName = emailDetails[0];
            pattern = @"([.])";
            emailName = Regex.Replace(emailName, pattern, String.Empty); //can also use "" instead of String.Empty
            email = String.Concat(emailName, emailDetails[1]);
        }
        return email;
    }
}
