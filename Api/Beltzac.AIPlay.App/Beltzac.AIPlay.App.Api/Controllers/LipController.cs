using System;
using System.IO;
using System.Threading.Tasks;
using Beltzac.AIPlay.App.Api.Apis;
using Beltzac.AIPlay.App.Api.Contract;
using Beltzac.AIPlay.App.Api.Helpers;
using Beltzac.AIPlay.App.Api.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Refit;

namespace Beltzac.AIPlay.App.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LipController : ControllerBase
    {
        private readonly IRabbitMQHelper _bus;
        private readonly LipHub _hub;
        public LipController(IRabbitMQHelper bus, LipHub hub)
        {
            _bus = bus;
            _hub = hub;
        }

        [HttpPost]
        public async Task<IActionResult> Post(IFormFile image, IFormFile audio)
        {
            if (image != null && audio != null)
            {
                var fileApi = RestService.For<IFileApi>("http://file/");

                var idRequest = Guid.NewGuid();

                Console.WriteLine($"Files received -> {idRequest}");

                //persist files

                var idImage = Guid.Empty;
                using (var ms = new MemoryStream())
                {
                    image.CopyTo(ms);
                    idImage = await fileApi.UploadFile(new ByteArrayPart(ms.ToArray(), image.FileName, image.ContentType));
                }

                var idAudio = Guid.Empty;
                using (var ms = new MemoryStream())
                {
                    audio.CopyTo(ms);
                    idAudio = await fileApi.UploadFile(new ByteArrayPart(ms.ToArray(), audio.FileName, audio.ContentType));     
                }

                //put in queue

                var message = new Lip
                {
                    IdAudio = idAudio,
                    IdImage = idImage,
                    IdRequest = idRequest
                };

                //TODO: colocar conexão no singleton e serializar dentro do helper
                var connection = _bus.CreateConnection(_bus.GetConnectionFactory());
                var json = JsonConvert.SerializeObject(message);
                _bus.WriteMessageOnQueue(json, "lip", connection);

                return Ok(idRequest);
            }

            return BadRequest();
        }
    }
}
