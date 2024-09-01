using System;
using System.Threading.Tasks;
using Application.Comments;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

public class ChatHub(IMediator mediator) : Hub
{
	public async Task SendComment(Create.Command command)
	{
		var comment = await mediator.Send(command);

		await Clients.Group(command.ActivityId.ToString())
			.SendAsync("ReceiveComment", comment.Value);
	}

	public override async Task OnConnectedAsync()
	{
		var httpContext = Context.GetHttpContext();
		var activityId = httpContext.Request.Query["activityId"];
		await Groups.AddToGroupAsync(Context.ConnectionId, activityId);
		var result = await mediator.Send(new Create.List.Query {ActivityId = Guid.Parse(activityId)});
		await Clients.Caller.SendAsync("LoadComments", result.Value);
	}
}