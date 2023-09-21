using MediatR;

namespace FunBot.Infrastructure.MediatR
{
    public class MediatorSchedulerBridge
    {
        private readonly IMediator _mediator;

        public MediatorSchedulerBridge(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task SendAsync(IRequest command)
        {
            await _mediator.Send(command);
        }

        public async Task PublishAsync(INotification command)
        {
            await _mediator.Publish(command);
        }
    }
}
