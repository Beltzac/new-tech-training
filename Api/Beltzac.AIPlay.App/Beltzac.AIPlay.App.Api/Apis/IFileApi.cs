using Microsoft.AspNetCore.Http;
using Refit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Beltzac.AIPlay.App.Api.Apis
{
    interface IFileApi
    {
        [Multipart]
        [Post("/api/file")]
        Task<Guid> UploadFile(ByteArrayPart file);
    }
}
