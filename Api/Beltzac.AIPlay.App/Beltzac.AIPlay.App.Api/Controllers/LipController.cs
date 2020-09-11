using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Beltzac.AIPlay.App.Api.Apis;
using Beltzac.AIPlay.App.Api.Contract;
using Beltzac.AIPlay.App.Api.Hubs;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Refit;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Beltzac.AIPlay.App.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LipController : ControllerBase
    {
        private readonly ICapPublisher _capPublisher;
        private readonly LipHub _hub;
        public LipController(ICapPublisher capPublisher, LipHub hub)
        {
            _capPublisher = capPublisher;
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

                await _capPublisher.PublishAsync(nameof(Lip), message);

                return Ok(idRequest);
            }

            return BadRequest();
        }

        [CapSubscribe(nameof(StatusUpdate))]
        public async Task OnUpdateProcessingStatusAsync(StatusUpdate status)
        {
            Console.WriteLine($"Message received -> {status.ProcessId} {status.PercentageProcessed} {status.Message}");
            await _hub.OnUpdateProcessingStatus(status);
        }
    }
}
