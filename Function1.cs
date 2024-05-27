using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

public static class RunExecutableFunction
{
    [FunctionName("RunExecutable")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        try
        {
            string username = Environment.UserName;
            string basePath = $@"C:\Users\{username}\source\repos\setupFunction\tools\";
            string executablePath = Path.Combine(basePath, "setup.exe");

            if (!File.Exists(executablePath))
            {
                return new BadRequestObjectResult("Executable file setup.exe not found.");
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    return new OkObjectResult($"Output: {output}");
                }
                else
                {
                    return new BadRequestObjectResult($"Error: {error}");
                }
            }
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error running executable.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
