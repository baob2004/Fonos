using Fonos.API.Common;
using Fonos.API.DTOs.Authors;

namespace Fonos.API.Services.Authors
{
    public interface IAuthorService
    {
        Task<PagedResponse<AuthorDto>> GetAllAuthorsAsync(QueryFilter filter, CancellationToken cancellationToken);
        Task<AuthorDto> GetAuthorAsync(Guid id);
        Task<AuthorDto> CreateAuthorAsync(AuthorCreateDto command);
        Task UpdateAuthorAsync(Guid id, AuthorUpdateDto command);
        Task DeleteAuthorAsync(Guid id);
    }
}