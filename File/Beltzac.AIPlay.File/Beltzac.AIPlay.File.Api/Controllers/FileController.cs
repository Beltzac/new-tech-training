using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Beltzac.AIPlay.File.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment hostEnvironment;
        public FileController(IWebHostEnvironment environment)
        {
            hostEnvironment = environment;
        }

        [HttpPost]
        public IActionResult Create(IFormFile file)
        {
            return CreateWithId(file, Guid.NewGuid());
        }

        [HttpPost("{id}")]
        public IActionResult CreateWithId(IFormFile file, Guid id)
        {
            if (file == null || id == Guid.Empty)
                return BadRequest();            

            var uniqueFileName = id + Path.GetExtension(file.FileName);
            var uploads = Path.Combine(hostEnvironment.WebRootPath, "uploads");
            var filePath = Path.Combine(uploads, uniqueFileName);

            using var stream = new FileStream(filePath, FileMode.Create);

            file.CopyTo(stream);
                
            return Ok(id);     
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();            

            var uploads = Path.Combine(hostEnvironment.WebRootPath, "uploads");
            var allFiles = Directory.GetFiles(uploads);

            //TODO: Bem simplista, melhorar
            var file = allFiles.FirstOrDefault(f => f.Contains(id.ToString()));

            if (file == null)
                return NotFound();

            var stream = new FileStream(file, FileMode.Open);

            return File(stream, "application/octet-stream", Path.GetFileName(file)); 
        }
    }
}
