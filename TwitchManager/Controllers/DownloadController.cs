using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace TwitchManager.Controllers
{
    public class DownloadController(ILogger<DownloadController> logger, IDataProtectionProvider provider) : ControllerBase
    {
        private readonly IDataProtector _protector = provider.CreateProtector("DownloadController");

        [HttpGet("/download/{path}")]
        public async Task<IActionResult> Download(string path)
        {
            string decryptedPath;
            try
            {
                decryptedPath = _protector.Unprotect(path);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error decrypting path: {Path}", path);
                return NotFound();
            }

            var split = decryptedPath.Split('|'); 

            if(split.Length != 2 )
            {
                logger.LogWarning("Invalid decrypted path format: {DecryptedPath}", decryptedPath);
                return NotFound();
            }

            var dateTime = DateTime.Parse(split[0]);

            if(dateTime < DateTime.UtcNow)
            {
                logger.LogWarning("Download link expired: {DecryptedPath}", decryptedPath);
                return NotFound();
            }

            var filePath = split[1];

            if(!System.IO.File.Exists(filePath))
            {
                logger.LogWarning("File not found: {FilePath}", filePath);
                return NotFound();
            }

            var file = new FileInfo(filePath);
            var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);

            return File(stream, "application/octet-stream", file.Name);
        }
    }
}
