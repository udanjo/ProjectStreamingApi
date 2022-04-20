﻿using Microsoft.AspNetCore.Mvc;

namespace ProjectStreamingApi.Results
{
    public class StreamResult : IActionResult
    {
        private readonly CancellationToken _requestAborted;
        private readonly Action<Stream, CancellationToken> _onStreaming;

        public StreamResult(Action<Stream, CancellationToken> onStreaming, CancellationToken requestAborted)
        {
            _requestAborted = requestAborted;
            _onStreaming = onStreaming;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            var stream = context.HttpContext.Response.Body;
            context.HttpContext.Response.GetTypedHeaders().ContentType = new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("text/event-stream");
            _onStreaming(stream, _requestAborted);

            return Task.CompletedTask;
        }
    }
}
