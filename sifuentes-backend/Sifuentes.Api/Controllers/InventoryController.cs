using Microsoft.AspNetCore.Mvc;
using Sifuentes.Api.Models;

namespace Sifuentes.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InventoryController : ControllerBase
    {
        private static readonly List<InventoryItem> Items = new()
        {
            new InventoryItem(1, "DM-1001", "Double Medical Screw 4.5mm", 150),
            new InventoryItem(2, "DM-2001", "Double Medical Plate 6 holes", 75)
        };

        [HttpGet]
        public ActionResult<IEnumerable<InventoryItem>> Get() => Items;

        [HttpGet("{code}")]
        public ActionResult<InventoryItem> GetByCode(string code)
        {
            var item = Items.FirstOrDefault(i => i.Code == code);
            return item is not null ? Ok(item) : NotFound();
        }
    }
}
