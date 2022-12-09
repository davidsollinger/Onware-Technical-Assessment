using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using health_path.Model;

namespace health_path.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly ILogger<ScheduleController> _logger;
    private readonly IDbConnection _connection;

    public ScheduleController(ILogger<ScheduleController> logger, IDbConnection connection)
    {
        _logger = logger;
        _connection = connection;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ScheduleEvent>> Fetch()
    {
        var dbResults = ReadData();

        //attempted this strategy to group the items together but it did not work
        //dbResults.GroupBy(o => o.Item1).Select(g => g.Skip(1).Aggregate(g.First(), (a, o) => {a.Item2.StartTime += o.Item2.StartTime; a.Item2.EndTime += o.Item2.EndTime; return a;}));

        var preparedResults = dbResults.Select((t) => {
            t.Item1.Recurrences.Add(t.Item2);
            return t.Item1;
        });

        return Ok(preparedResults);
    }

    private IEnumerable<(ScheduleEvent, ScheduleEventRecurrence)> ReadData() 
    {
        //change to combine two rows with the same EventId
        //GROUP BY was not working
        var sql = @"
            SELECT e.*, r.*
            FROM Event e
            JOIN EventRecurrence r ON e.Id = r.EventId
            ORDER BY e.Id, r.DayOfWeek, r.StartTime, r.EndTime
        ";
        
        return _connection.Query<ScheduleEvent, ScheduleEventRecurrence, (ScheduleEvent, ScheduleEventRecurrence)>(sql, (e, r) => (e, r));
    }
}
