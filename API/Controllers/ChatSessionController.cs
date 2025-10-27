using Application.Dto;
using Application.Interfaces;
using Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatSessionController(
        IUseCase<CreateChatRequest, CreateChatResponse> create,
        IUseCase<Guid, PollResponse> poll) : ControllerBase
    {
        private readonly IUseCase<CreateChatRequest, CreateChatResponse> _create = create;
        private readonly IUseCase<Guid, PollResponse> _poll = poll;

        [HttpPost]
        public async Task<ActionResult<CreateChatResponse>> Create([FromBody] CreateChatRequest request, CancellationToken ct)
        {
            var idem = request.IdempotencyKey ?? Request.Headers["Idempotency-Key"].FirstOrDefault();
            var res = await _create.HandleAsync(new CreateChatRequest(idem), ct);

            if (res.Status == "REFUSED")
                return StatusCode(StatusCodes.Status429TooManyRequests, res);

            return Ok(res);
        }

        [HttpPost("{id:guid}/poll")]
        [EnableRateLimiting("polls")]
        public async Task<ActionResult<PollResponse>> Poll(Guid id, CancellationToken ct)
        {
            var res = await _poll.HandleAsync(id, ct);
            return Ok(res);
        }
    }
}
