using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PDFReader;

namespace MOTORSPdfHelper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PDFSplitter : ControllerBase
    {
        protected ILogger logger;
        
        public PDFSplitter(ILogger<PDFSplitter> _logger)
        {
            logger = _logger;
        }
        
        [HttpGet("/")]
        public IActionResult Console()
        {
            return Redirect("/index.html");
        }

        /// <summary>
        ///  切分 DahSing Renewal PDF
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("/dahsing/renewal")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> DahSingRenewal(IFormFile file)
        {
            if (!file.ContentType.Equals("application/pdf"))
            {
                throw new Exception("upload file not a PDF");
            }
            
            var list = PDFHelper.SplitDahSingRenewal(file.OpenReadStream());

            var output = PDFHelper.ZipPDFFiles(list);
                
            return File(output, "application/zip", file.FileName + ".zip");
        }
    }
}