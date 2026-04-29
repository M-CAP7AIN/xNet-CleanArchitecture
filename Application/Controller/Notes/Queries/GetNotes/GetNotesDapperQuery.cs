using Domain.Interfaces;
using MediatR;

namespace Application.Controller.Notes.Queries.GetNotes
{
    public record GetNotesDapperQuery() : IRequest<List<NoteResponse>>;

    public class NoteResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid UserId { get; set; }
    }



    public class GetNotesDapperQueryHandler(IDapperService dapper) : IRequestHandler<GetNotesDapperQuery, List<NoteResponse>>
    {
       

        public async Task<List<NoteResponse>> Handle(GetNotesDapperQuery request, CancellationToken cancellationToken)
        {
            const string sql = @"
                SELECT 
                    Id, 
                    Title, 
                    Content, 
                    CreatedAt, 
                    UserId
                FROM Notes
                ORDER BY CreatedAt DESC";

            var notes = await dapper.QueryAsync<NoteResponse>(sql, cancellationToken);

            return notes.ToList();
        }
    }
}

