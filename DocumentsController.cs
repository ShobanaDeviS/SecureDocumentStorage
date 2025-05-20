using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public DocumentsController(AppDbContext context) => _context = context;

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Invalid file");

        var username = User.Identity.Name!;
        var existingVersions = _context.Documents
            .Where(d => d.FileName == file.FileName && d.Owner == username);
        var version = existingVersions.Any() ? existingVersions.Max(d => d.Revision) + 1 : 0;

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var doc = new Document
        {
            FileName = file.FileName,
            Content = ms.ToArray(),
            Owner = username,
            Revision = version,
            UploadedAt = DateTime.UtcNow
        };

        _context.Documents.Add(doc);
        await _context.SaveChangesAsync();
        return Ok("File uploaded");
    }

    [HttpGet("{fileName}")]
    public async Task<IActionResult> Download(string fileName, [FromQuery] int? revision)
    {
        var username = User.Identity.Name!;
        var query = _context.Documents
            .Where(d => d.FileName == fileName && d.Owner == username);
        var doc = revision.HasValue
            ? await query.OrderBy(d => d.Revision).Skip(revision.Value).FirstOrDefaultAsync()
            : await query.OrderByDescending(d => d.Revision).FirstOrDefaultAsync();

        if (doc == null) return NotFound();
        return File(doc.Content, "application/octet-stream", doc.FileName);
    }
}