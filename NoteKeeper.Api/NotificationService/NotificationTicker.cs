using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using NoteKeeper.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NoteKeeper.Api.NotificationService
{
    public class NotificationTicker
    {
        private readonly static Lazy<NotificationTicker> _instance = new Lazy<NotificationTicker>(() => new NotificationTicker(GlobalHost.ConnectionManager.GetHubContext<NotificationHub>().Clients));
        public static NotificationTicker Instance { get { return _instance.Value; } }

        private IHubConnectionContext<dynamic> _clients;
        private ConcurrentDictionary<Guid, List<string>> _userToConnectionIdsMapping;
        private ConcurrentDictionary<string, Guid> _connectionIdToUserMapping;
        private NotificationTicker(IHubConnectionContext<dynamic> clients)
        {
            _clients = clients;
            _userToConnectionIdsMapping = new ConcurrentDictionary<Guid, List<string>>();
            _connectionIdToUserMapping = new ConcurrentDictionary<string, Guid>();
        }

        public ConcurrentDictionary<Guid, List<string>> UserToConnectionIdsMapping { get { return _userToConnectionIdsMapping; } }
        public ConcurrentDictionary<string, Guid> ConnectionIdToUserMapping { get { return _connectionIdToUserMapping; } }

        public void NotifyAboutNoteChanged(Note changedNote, Guid userToNotifyId)
        {
            List<string> connectionIds;
            if (_userToConnectionIdsMapping.TryGetValue(userToNotifyId, out connectionIds))
            {
                foreach (string id in connectionIds)
                {
                    _clients.Client(id).OnNoteChanged(changedNote.Id, changedNote.OwnerId);
                }
            }
        }

        public void NotifyAboutDeniedAccess(Guid noteId, Guid userToNotifyId)
        {
            List<string> connectionIds;
            if (_userToConnectionIdsMapping.TryGetValue(userToNotifyId, out connectionIds))
            {
                foreach (string id in connectionIds)
                {
                    _clients.Client(id).OnAccessDenied(noteId);
                }
            }
        }
    }
}