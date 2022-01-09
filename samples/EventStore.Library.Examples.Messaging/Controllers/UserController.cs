using System.ComponentModel.DataAnnotations;
using EventStore.Library.Examples.Messaging.Aggregates;
using EventStore.Library.Examples.Messaging.Processes;
using EventStore.Library.Examples.Messaging.Queries;
using EventStore.Library.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace EventStore.Library.Examples.Messaging.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public UserController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost]
    public async Task<IActionResult> EnrollUser([FromBody] EnrollUserRequest request)
    {
        // Puts the command on the queue to be processed
        await _dispatcher.Enqueue(new EnrollUserCommand(request.FirstName, request.LastName, request.Email, request.By));

        return Accepted();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserAccount(string id, [FromBody] DeleteUserAccountRequest request)
    {
        // Processes the command directly 
        var success = await _dispatcher.Send(new DeleteUserAccountCommand(new UserId(id), request.By));

        return success ? NoContent() : BadRequest();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        // Sends query
        var user = await _dispatcher.Send(new UserQuery(id));

        return user is not null ? Ok(user) : NotFound();
    }

    public class EnrollUserRequest
    {
        [Required]
        public string? FirstName { get; set; }
        [Required]
        public string? LastName { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? By { get; set; }
    }

    public class DeleteUserAccountRequest
    {
        [Required]
        public string? By { get; set; }
    }
}