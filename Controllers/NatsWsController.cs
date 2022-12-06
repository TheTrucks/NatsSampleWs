using MessagePack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NATS_WS.Services;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace NATS_WS.Controllers
{
    [ApiController]
    public class NatsWsController : ControllerBase
    {
        private readonly Encoding _enc = UTF8Encoding.UTF8;
        private readonly WSSender _sender;

        public NatsWsController(WSSender sender)
        {
            _sender = sender;
        }

        [HttpGet("wsconnect")]
        public async Task Connect()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var C_ID = Request.HttpContext.Connection.Id;
                using (var WS = await HttpContext.WebSockets.AcceptWebSocketAsync())
                {
                    await _sender.Subscribe(WS, C_ID);
                    byte[] Message = new byte[1024 * 4];
                    while ((await WS.ReceiveAsync(Message, CancellationToken.None)).CloseStatus == null)
                    {
                        //do nothing, wait for "disconnect" message from client
                    }
                    await _sender.Unsubscribe(C_ID);
                }
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.BodyWriter.WriteAsync(_enc.GetBytes("Only WebSocket connection is allowed"));
            }
        }
    }
}
