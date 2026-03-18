using Fonos.API.DTOs.Chapters;
using Fonos.API.Services.Chapters;
using Microsoft.AspNetCore.Mvc;

namespace Fonos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class ChaptersController : ControllerBase
    {
        private readonly IChapterService _chapterService;

        public ChaptersController(IChapterService chapterService)
        {
            _chapterService = chapterService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ChapterDto>> GetById(Guid id)
        {
            return Ok(await _chapterService.GetChapterAsync(id));
        }

        [HttpPost]
        public async Task<ActionResult<ChapterDto>> Create([FromBody] ChapterCreateDto command)
        {
            var result = await _chapterService.CreateChapterAsync(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ChapterUpdateDto command)
        {
            await _chapterService.UpdateChapterAsync(id, command);
            return NoContent();
        }

        [HttpPatch("{id}/audio")]
        public async Task<IActionResult> UpdateAudio(Guid id, [FromBody] ChapterAudioUpdateDto command)
        {
            await _chapterService.UpdateAudioAsync(id, command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _chapterService.DeleteChapterAsync(id);
            return NoContent();
        }
    }
}