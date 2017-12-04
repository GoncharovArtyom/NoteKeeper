using Microsoft.AspNet.SignalR;
using NoteKeeper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;

namespace NoteKeeper.Api.NotificationService
{
    public class NotificationHub : Hub
    {
        private static NotificationTicker notificationTicker = NotificationTicker.Instance;

        public void RegisterUser(Guid userId)
        {
            List<string> connectionIds;
            if (notificationTicker.UserToConnectionIdsMapping.TryGetValue(userId, out connectionIds))
            {
                if (connectionIds.FindIndex((item) => item.Equals(Context.ConnectionId)) == -1)
                {
                    connectionIds.Add(Context.ConnectionId);
                }
            }
            else
            {
                notificationTicker.UserToConnectionIdsMapping.TryAdd(userId, new List<string>() { Context.ConnectionId });
            }
            notificationTicker.ConnectionIdToUserMapping.TryAdd(Context.ConnectionId, userId);
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Guid userId;
            if (notificationTicker.ConnectionIdToUserMapping.TryRemove(Context.ConnectionId, out userId))
            {
                List<string> connectionIds;
                if (notificationTicker.UserToConnectionIdsMapping.TryGetValue(userId, out connectionIds))
                {
                    if (connectionIds.Count == 0)
                    {
                        notificationTicker.UserToConnectionIdsMapping.TryRemove(userId, out connectionIds);
                    }
                    else
                    {
                        connectionIds.Remove(Context.ConnectionId);
                    }
                }
            }
            return base.OnDisconnected(stopCalled);
        }
    }
}