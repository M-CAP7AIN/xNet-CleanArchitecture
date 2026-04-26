using Application.Behaviors;
using Application.Common.Interfaces;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Notes.Queries.GetNotes
{
    public record GetNotesQuery : IRequest<List<NoteDto>>, ICacheableQuery
    {
        public bool? IsArchived { get; set; }
        public NotePriority? Priority { get; set; }

        public string CacheKey => $"notes_{IsArchived}_{Priority}";
        public int CacheDurationInMinutes => 5; // کش به مدت 5 دقیقه
    }

    public class GetNotesQueryHandler : IRequestHandler<GetNotesQuery, List<NoteDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetNotesQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<NoteDto>> Handle(GetNotesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Notes.AsQueryable();

            if (request.IsArchived.HasValue)
                query = query.Where(n => n.IsArchived == request.IsArchived.Value);

            if (request.Priority.HasValue)
                query = query.Where(n => n.Priority == request.Priority.Value);

            var notes = await query
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<NoteDto>>(notes);
        }
    }
}
