using Application.Controller.Notes;
using Application.Controller.Notes.Commands.CreateNote;
using Application.Controller.Notes.Commands.DeleteNote;
using Application.Controller.Notes.Commands.UpdateNote;
using Application.Controller.Notes.Queries.GetNoteById;
using Application.Controller.Notes.Queries.GetNotes;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiExplorerSettings(GroupName = "v1")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<NoteDto>>> GetNotes([FromQuery] GetNotesQuery query)
        {
            return Ok(await _mediator.Send(query));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NoteDto>> GetNote(Guid id)
        {
            return Ok(await _mediator.Send(new GetNoteByIdQuery(id)));
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateNote(CreateNoteCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetNote), new { id }, id);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(Guid id, UpdateNoteCommand command)
        {
            if (id != command.Id)
                return BadRequest();

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(Guid id)
        {
            await _mediator.Send(new DeleteNoteCommand(id));
            return NoContent();
        }
    }
}
