using Fonos.API.Common;
using Fonos.API.DTOs.Books;
using Fonos.API.DTOs.Chapters;
using Fonos.API.Services.Books;
using Fonos.API.Services.Chapters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fonos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly IChapterService _chapterService;
        public BooksController(IBookService bookService, IChapterService chapterService)
        {
            _bookService = bookService;
            _chapterService = chapterService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<BookDto>>> GetAll([FromQuery] QueryFilter filter, CancellationToken cancellationToken)
        {
            var books = await _bookService.GetAllBooksAsync(filter,cancellationToken);
            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> GetById(Guid id)
        {
            var book = await _bookService.GetBookAsync(id);
            return Ok(book);
        }

        [HttpPost]
        public async Task<ActionResult<BookDto>> Create([FromForm] BookCreateDto command)
        {
            var result = await _bookService.CreateBookAsync(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update(Guid id, [FromForm] BookUpdateDto command)
        {
            await _bookService.UpdateBookAsync(id, command);
            return Ok(new { message = "Cập nhật sách thành công!" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _bookService.DeleteBookAsync(id);
            return NoContent();
        }

        [HttpGet("{bookId}/chapters")]
        public async Task<ActionResult<IEnumerable<ChapterDto>>> GetChapters(Guid bookId)
        {
            var userId = User.FindFirst("uid")?.Value;

            var chapters = await _chapterService.GetChaptersByBookAsync(bookId, userId);
            return Ok(chapters);
        }

        [HttpGet("purchased")]
        [Authorize]
        public async Task<ActionResult<PagedResponse<BookDto>>> GetPurchased([FromQuery] QueryFilter filter, CancellationToken ct)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _bookService.GetPurchasedBooksAsync(userId, filter, ct);
            return Ok(result);
        }

        [HttpGet("{id}/ownership")]
        [Authorize]
        public async Task<ActionResult<bool>> CheckOwnership(Guid id)
        {
            var userId = User.FindFirst("uid")?.Value;

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var isOwned = await _bookService.CheckOwnershipAsync(userId, id);

            return Ok(isOwned);
        }
    }
}