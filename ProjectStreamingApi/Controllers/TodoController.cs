using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ProjectStreamingApi.Application.Models;
using ProjectStreamingApi.Hubs;
using ProjectStreamingApi.Results;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ProjectStreamingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private static ConcurrentBag<StreamWriter> _clients = new ConcurrentBag<StreamWriter>();
        private static List<ItemModel> _itens = new List<ItemModel>();
        private readonly IHubContext<StreamingHub> _streaming;

        public TodoController(IHubContext<StreamingHub> streaming)
        {
            _streaming = streaming;
        }

        [HttpGet]
        public ActionResult<List<ItemModel>> Get() => _itens;

        [HttpPost]
        public async Task<ActionResult<ItemModel>> Post([FromBody] ItemModel request)
        {
            if (request == null)
                return BadRequest();

            if (request.Id == Guid.Empty) request.Id = Guid.NewGuid();

            _itens.Add(request);

            await WriteOnStream(request, "Added");

            return request;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var client = _itens.SingleOrDefault(i => i.Id == id);
            if (client != null)
            {
                _itens.Remove(client);

                await WriteOnStream(client, "removed");

                return Ok(new { Description = "Item removed" });
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("streaming")]
        public IActionResult Streaming()
        {
            return new StreamResult(
                (stream, cancelToken) =>
                {
                    var wait = cancelToken.WaitHandle;
                    var client = new StreamWriter(stream);
                    _clients.Add(client);

                    wait.WaitOne();

                    StreamWriter ignore;
                    _clients.TryTake(out ignore);
                },
                HttpContext.RequestAborted);
        }

        private async Task WriteOnStream(ItemModel data, string action)
        {
            string jsonData = string.Format("{0}\n", JsonSerializer.Serialize(new { data, action }));

            await _streaming.Clients.All.SendAsync("ReceiveMessage", jsonData);

            foreach (var client in _clients)
            {
                await client.WriteAsync(jsonData);
                await client.FlushAsync();
            }
        }
    }
}