using Application.Behaviors;
using Application.Controller.Notes;
using AutoMapper;
using Domain.Enums;
using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Controller.Notes.Queries.GetNotes
{
    public record GetNotesQuery : IRequest<List<NoteDto>>
    {
        public bool? IsArchived { get; set; }
        public NotePriority? Priority { get; set; }
    }

    public class GetNotesQueryHandler(ApplicationDbContext context, IMapper mapper) : IRequestHandler<GetNotesQuery, List<NoteDto>>
    {

        public async Task<List<NoteDto>> Handle(GetNotesQuery request, CancellationToken cancellationToken)
        {
            var query = context.Notes.AsQueryable();

            if (request.IsArchived.HasValue)
                query = query.Where(n => n.IsArchived == request.IsArchived.Value);

            if (request.Priority.HasValue)
                query = query.Where(n => n.Priority == request.Priority.Value);

            var notes = await query
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);

            return mapper.Map<List<NoteDto>>(notes);
        }
    }
}
