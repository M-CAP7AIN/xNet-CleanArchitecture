using Application.Common.Exceptions;
using Application.Controller.Notes;
using AutoMapper;
using Domain.Entities;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Controller.Notes.Queries.GetNoteById
{
    public record GetNoteByIdQuery(Guid Id) : IRequest<NoteDto>;


    public class GetNoteByIdQueryHandler(ApplicationDbContext context, IMapper mapper) : IRequestHandler<GetNoteByIdQuery, NoteDto>
    {

        public async Task<NoteDto> Handle(GetNoteByIdQuery request, CancellationToken cancellationToken)
        {
            var note = await context.Notes
                .FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken);

            if (note == null)
                throw new NotFoundException(nameof(Note), request.Id);

            return mapper.Map<NoteDto>(note);
        }
    }
}
