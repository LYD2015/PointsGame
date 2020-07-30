using Microsoft.AspNetCore.SignalR;
using PointsGame.Dto.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PointsGame.Helper
{
    /// <summary>
    /// 下发客户端的消息
    /// </summary>
    public class HelperSendClientMessage : Hub {

        /// <summary>
        /// 下发多个的消息
        /// </summary>
        /// <returns></returns>
        public async Task SendInfos(List<SendClientType> types) {
            if (Clients == null) {
                return; // 没有活动的客户端
            }
            // 下发数据到浏览器
            await Clients.All.SendAsync("recv", new { types = types.Select(item => (int)item).ToList() });
        }
        /// <summary>
        /// 下发单个消息
        /// </summary>
        /// <returns></returns>
        public async Task SendInfo(SendClientType type) {
            // 下发数据到浏览器
            await SendInfos(new List<SendClientType> { type });
        }

        //public override Task OnConnectedAsync() {
        //    Console.WriteLine("哇，有人进来了：{0}", this.Context.ConnectionId);
        //    return base.OnConnectedAsync();
        //}

        //public override Task OnDisconnectedAsync(Exception exception) {
        //    Console.WriteLine("靠，有人跑路了：{0}", this.Context.ConnectionId);
        //    return base.OnDisconnectedAsync(exception);
        //}
    }
}
