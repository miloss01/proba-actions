using System.Text.Json;

namespace DockerHubBackend.Dto.Response
{
    public class PageDTO<T>
    {
        public List<T> Data { get; set; }
        public int TotalNumberOfElements { get; set; }

        public PageDTO() { }

        public PageDTO(List<T> data, int totalNumberOfElements)
        {
            this.Data = data;
            this.TotalNumberOfElements = totalNumberOfElements;
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
