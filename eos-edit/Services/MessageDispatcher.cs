using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eos.ViewModels.Base
{
    public enum MessageType
    {
        ChangeLanguage,
        DoGameDataImport,

        NewProject,
        OpenProject,
        SaveProject,
        CloseProject,
        UpdateProject,
        NewDetail,
        OverrideDetail,
        CopyDetail,
        OpenDetail,
        OpenDetailSilent,
        CloseDetail,
        DeleteDetail,
        NewCustomDetail,
        GotoDetail,

        OpenGlobalSearch,
        OpenProjectSettings,
        OpenDataImport,
        OpenTlkEditor
    }

    public delegate void MessageHandler(MessageType type, object? message, object? param);

    public static class MessageDispatcher
    {
        private static Dictionary<MessageType, List<MessageHandler>> messageSubscriptions = new Dictionary<MessageType, List<MessageHandler>>();

        static MessageDispatcher()
        {
            foreach (var type in Enum.GetValues<MessageType>())
                messageSubscriptions[type] = new List<MessageHandler>();
        }

        public static void Subscribe(MessageType messageType, MessageHandler handler)
        {
            messageSubscriptions[messageType].Add(handler);
        }

        public static void Unsubscribe(MessageType messageType, MessageHandler handler)
        {
            messageSubscriptions[messageType].Remove(handler);
        }

        public static void Send(MessageType messageType, object? message, object? param = null)
        {
            foreach (var handler in messageSubscriptions[messageType])
                handler(messageType, message, param);
        }
    }
}
