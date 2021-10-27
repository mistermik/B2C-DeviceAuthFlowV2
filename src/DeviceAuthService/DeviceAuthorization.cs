using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using  System.Linq;

namespace Ltwlf.Azure.B2C
{
    public class DeviceAuthorization
    {
        class AuthorizationResponse
        {
            [JsonProperty("device_code")] public string DeviceCode { get; set; }
            [JsonProperty("user_code")] public string UserCode { get; set; }
            [JsonProperty("verification_uri")] public string VerificationUri { get; set; }
            [JsonProperty("expires_in")] public int ExpiresIn { get; set; }
        }

        private readonly IConnectionMultiplexer _muxer;

        private readonly ConfigOptions _config;

        public DeviceAuthorization(IConnectionMultiplexer muxer, IOptions<ConfigOptions> options)
        {
            _muxer = muxer;
            _config = options.Value;
        }

        [FunctionName("device_authorization")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "oauth/device_authorization")]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("DeviceAuthorization function is processing a request.");

            if (req.ContentLength == null || !req.ContentType.Equals("application/x-www-form-urlencoded",
                StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException("Request content type must be application/x-www-form-urlencoded");
            }

            if (!req.Form.TryGetValue("clientId", out var clientId))
            {
                throw new ArgumentException("ClientId is missing!");
            }

            var authState = new AuthorizationState()
            {
                DeviceCode = GenerateDeviceCode(),
                ClientId = clientId,
                UserCode = GenerateUserCode(),
                ExpiresIn = 300,
                VerificationUri = _config.VerificationUri,
                Scope = req.Form?["scope"]
            };

            var response = new AuthorizationResponse()
            {
                DeviceCode = authState.DeviceCode,
                UserCode = authState.UserCode,
                ExpiresIn = authState.ExpiresIn,
                VerificationUri = authState.VerificationUri
            };
            var key = $"{authState.DeviceCode}:{authState.UserCode}";
            _muxer.GetDatabase().StringSet(key,
                JsonConvert.SerializeObject(authState), new TimeSpan(0, 0, authState.ExpiresIn));

            return new OkObjectResult(response);
        }

        private static string GenerateDeviceCode()
        {
            return Guid.NewGuid().ToString();
        }

        private string GenerateUserCode()
        {
            int num = 1;
            for (var i = 0; i < _config.UserCodeLength; i++)
            {
                num = num * 10;
            }
            return new Random().Next(0, num -1).ToString($"D{_config.UserCodeLength}");
        }
    }
}