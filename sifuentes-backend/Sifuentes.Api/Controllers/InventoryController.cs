using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sifuentes.Api.Data;
using Sifuentes.Api.Models;
using System.Linq;
using System.Text;

namespace Sifuentes.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryContext _db;

        public InventoryController(InventoryContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> Get()
            => await _db.Items.AsNoTracking().ToListAsync();

        [HttpGet("{id:int}")]
        public async Task<ActionResult<InventoryItem>> GetById(int id)
        {
            var item = await _db.Items.FindAsync(id);
            return item is not null ? Ok(item) : NotFound();
        }

        [HttpGet("code/{code}")]
        public async Task<ActionResult<InventoryItem>> GetByCode(string code)
        {
            var item = await _db.Items.FirstOrDefaultAsync(i => i.Code == code);
            return item is not null ? Ok(item) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<InventoryItem>> Create(InventoryItem item)
        {
            _db.Items.Add(item);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, InventoryItem updated)
        {
            var existing = await _db.Items.FindAsync(id);
            if (existing is null) return NotFound();

            existing.Code = updated.Code;
            existing.Description = updated.Description;
            existing.Quantity = updated.Quantity;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> Search([FromQuery] string term)
        {
            term = term.ToLower();
            var results = await _db.Items
                .Where(i => i.Code.ToLower().Contains(term) || i.Description.ToLower().Contains(term))
                .AsNoTracking()
                .ToListAsync();
            return Ok(results);
        }

        [HttpGet("report")]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> Report()
            => await _db.Items.AsNoTracking().OrderBy(i => i.Code).ToListAsync();

        [HttpGet("export/csv")]
        public async Task<FileResult> ExportCsv()
        {
            var items = await _db.Items.AsNoTracking().ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Id,Code,Description,Quantity");
            foreach (var i in items)
                sb.AppendLine($"{i.Id},{i.Code},\"{i.Description}\",{i.Quantity}");
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "inventory.csv");
        }

        [HttpGet("export/pdf")]
        public async Task<FileResult> ExportPdf()
        {
            var items = await _db.Items.AsNoTracking().ToListAsync();
            var content = new StringBuilder();
            content.AppendLine("Inventory Report");
            content.AppendLine("----------------");
            foreach (var i in items)
                content.AppendLine($"{i.Code} - {i.Description} : {i.Quantity}");

            // minimal PDF file with text content
            var text = content.ToString();
            var pdf = GenerateSimplePdf(text);
            return File(pdf, "application/pdf", "inventory.pdf");
        }

        private static byte[] GenerateSimplePdf(string text)
        {
            // This is a very small and naive PDF generator for demo purposes
            var sb = new StringBuilder();
            sb.AppendLine("%PDF-1.1");
            sb.AppendLine("1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj");
            sb.AppendLine("2 0 obj<</Type/Pages/Count 1/Kids[3 0 R]>>endobj");
            var stream = $"BT /F1 12 Tf 72 720 Td ({text}) Tj ET";
            sb.AppendLine($"3 0 obj<</Type/Page/Parent 2 0 R/MediaBox[0 0 612 792]/Contents 4 0 R/Resources<</Font<</F1 5 0 R>>>>>>endobj");
            sb.AppendLine($"4 0 obj<</Length {stream.Length}>>stream{stream}endstreamendobj");
            sb.AppendLine("5 0 obj<</Type/Font/Subtype/Type1/Name/F1/BaseFont/Helvetica>>endobj");
            sb.AppendLine("xref 0 6 0000000000 65535 f 0000000010 00000 n 0000000053 00000 n 0000000100 00000 n 0000000235 00000 n 0000000305 00000 n trailer<</Size 6/Root 1 0 R>>startxref 357 %%EOF");
            return Encoding.ASCII.GetBytes(sb.ToString());
        }
    }
}
