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
        public async Task<ActionResult<IEnumerable<BookDto>>> GetAll()
        {
            var books = await _bookService.GetAllBooksAsync();
            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> GetById(Guid id)
        {
            var book = await _bookService.GetBookAsync(id);
            return Ok(book);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<BookDto>> Create([FromBody] BookCreateDto command)
        {
            var result = await _bookService.CreateBookAsync(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update(Guid id, [FromBody] BookUpdateDto command)
        {
            await _bookService.UpdateBookAsync(id, command);
            return NoContent();
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
            var chapters = await _chapterService.GetChaptersByBookAsync(bookId);
            return Ok(chapters);
        }
    }
}