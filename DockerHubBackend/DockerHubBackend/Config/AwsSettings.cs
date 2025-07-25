using DockerHubBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace DockerHubBackend.Config
{
    public class AwsSettings
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Region { get; set; }
    }
}
