using System.Collections.Generic;
using System;

namespace Manager {

    public enum ListenerType : byte {
        RequireListener,
        DontRequireListener,
    }

    internal static class ListenerInternal {
        public static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();
        public static readonly ListenerType type = ListenerType.DontRequireListener;

        public static void OnListenerAdding(string eventType, Delegate listenerBeingAdded) {
            if(!eventTable.ContainsKey(eventType)) {
                eventTable.Add(eventType, null);
            }

            Delegate d = eventTable[eventType];
            if(d != null && d.GetType() != listenerBeingAdded.GetType()) {
                throw new ListenerException(string.Format("Attempting to add listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being added has type {2}", eventType, d.GetType().Name, listenerBeingAdded.GetType().Name));
            }
        }

        public static void OnListenerRemoving(string eventType, Delegate listenerBeingRemoved) {
            if(eventTable.ContainsKey(eventType)) {
                Delegate d = eventTable[eventType];

                if(d == null) {
                    throw new ListenerException(string.Format("Attempting to remove listener with for event type {0} but current listener is null.", eventType));
                } else if(d.GetType() != listenerBeingRemoved.GetType()) {
                    throw new ListenerException(string.Format("Attempting to remove listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being removed has type {2}", eventType, d.GetType().Name, listenerBeingRemoved.GetType().Name));
                }
            } else {
                throw new ListenerException(string.Format("Attempting to remove listener for type {0} but ListenerManager doesn't know about this event type.", eventType));
            }
        }

        public static void OnListenerRemoved(string eventType) {
            if(eventTable[eventType] == null) {
                eventTable.Remove(eventType);
            }
        }

        public static void OnBroadcasting(string eventType, ListenerType mode) {
            if(mode == ListenerType.RequireListener && !eventTable.ContainsKey(eventType)) {
                throw new ListenerInternal.BroadcastException(string.Format("Broadcasting message {0} but no listener found.", eventType));
            }
        }

        public static BroadcastException CreateBroadcastSignatureException(string eventType) {
            return new BroadcastException(string.Format("Broadcasting message {0} but listeners have a different signature than the broadcaster.", eventType));
        }

        public class BroadcastException : Exception {
            public BroadcastException(string msg)
                : base(msg) {
            }
        }

        public class ListenerException : Exception {
            public ListenerException(string msg)
                : base(msg) {
            }
        }
    }

    public static class ListenerManager {

        private static Dictionary<string, Delegate> eventTable = ListenerInternal.eventTable;

        public static void AddListener(string eventType, Callback handler) {
            ListenerInternal.OnListenerAdding(eventType, handler);
            eventTable[eventType] = (Callback)eventTable[eventType] + handler;
        }

        public static void RemoveListener(string eventType, Callback handler) {
            ListenerInternal.OnListenerRemoving(eventType, handler);
            eventTable[eventType] = (Callback)eventTable[eventType] - handler;
            ListenerInternal.OnListenerRemoved(eventType);
        }

        public static void Broadcast(string eventType) {
            Broadcast(eventType, ListenerInternal.type);
        }

        public static void Broadcast(string eventType, ListenerType mode) {
            ListenerInternal.OnBroadcasting(eventType, mode);
            Delegate d;
            if(eventTable.TryGetValue(eventType, out d)) {
                Callback callback = d as Callback;
                if(callback != null) {
                    callback();
                } else {
                    throw ListenerInternal.CreateBroadcastSignatureException(eventType);
                }
            }
        }
    }

    // One parameter
    public static class ListenerManager<T> {
        private static Dictionary<string, Delegate> eventTable = ListenerInternal.eventTable;

        public static void AddListener(string eventType, Callback<T> handler) {
            ListenerInternal.OnListenerAdding(eventType, handler);
            eventTable[eventType] = (Callback<T>)eventTable[eventType] + handler;
        }

        public static void RemoveListener(string eventType, Callback<T> handler) {
            ListenerInternal.OnListenerRemoving(eventType, handler);
            eventTable[eventType] = (Callback<T>)eventTable[eventType] - handler;
            ListenerInternal.OnListenerRemoved(eventType);
        }

        public static void Broadcast(string eventType, T arg1) {
            Broadcast(eventType, arg1, ListenerInternal.type);
        }

        public static void Broadcast(string eventType, T arg1, ListenerType mode) {
            ListenerInternal.OnBroadcasting(eventType, mode);
            Delegate d;
            if(eventTable.TryGetValue(eventType, out d)) {
                Callback<T> callback = d as Callback<T>;
                if(callback != null) {
                    callback(arg1);
                } else {
                    throw ListenerInternal.CreateBroadcastSignatureException(eventType);
                }
            }
        }
    }


    // Two parameters
    public static class ListenerManager<T, U> {
        private static Dictionary<string, Delegate> eventTable = ListenerInternal.eventTable;

        public static void AddListener(string eventType, Callback<T, U> handler) {
            ListenerInternal.OnListenerAdding(eventType, handler);
            eventTable[eventType] = (Callback<T, U>)eventTable[eventType] + handler;
        }

        public static void RemoveListener(string eventType, Callback<T, U> handler) {
            ListenerInternal.OnListenerRemoving(eventType, handler);
            eventTable[eventType] = (Callback<T, U>)eventTable[eventType] - handler;
            ListenerInternal.OnListenerRemoved(eventType);
        }

        public static void Broadcast(string eventType, T arg1, U arg2) {
            Broadcast(eventType, arg1, arg2, ListenerInternal.type);
        }

        public static void Broadcast(string eventType, T arg1, U arg2, ListenerType mode) {
            ListenerInternal.OnBroadcasting(eventType, mode);
            Delegate d;
            if(eventTable.TryGetValue(eventType, out d)) {
                Callback<T, U> callback = d as Callback<T, U>;
                if(callback != null) {
                    callback(arg1, arg2);
                } else {
                    throw ListenerInternal.CreateBroadcastSignatureException(eventType);
                }
            }
        }
    }


    // Three parameters
    public static class ListenerManager<T, U, V> {
        private static Dictionary<string, Delegate> eventTable = ListenerInternal.eventTable;

        public static void AddListener(string eventType, Callback<T, U, V> handler) {
            ListenerInternal.OnListenerAdding(eventType, handler);
            eventTable[eventType] = (Callback<T, U, V>)eventTable[eventType] + handler;
        }

        public static void RemoveListener(string eventType, Callback<T, U, V> handler) {
            ListenerInternal.OnListenerRemoving(eventType, handler);
            eventTable[eventType] = (Callback<T, U, V>)eventTable[eventType] - handler;
            ListenerInternal.OnListenerRemoved(eventType);
        }

        public static void Broadcast(string eventType, T arg1, U arg2, V arg3) {
            Broadcast(eventType, arg1, arg2, arg3, ListenerInternal.type);
        }

        public static void Broadcast(string eventType, T arg1, U arg2, V arg3, ListenerType mode) {
            ListenerInternal.OnBroadcasting(eventType, mode);
            Delegate d;
            if(eventTable.TryGetValue(eventType, out d)) {
                Callback<T, U, V> callback = d as Callback<T, U, V>;
                if(callback != null) {
                    callback(arg1, arg2, arg3);
                } else {
                    throw ListenerInternal.CreateBroadcastSignatureException(eventType);
                }
            }
        }
    }

    // Four parameters
    public static class ListenerManager<T, U, V, W> {
        private static Dictionary<string, Delegate> eventTable = ListenerInternal.eventTable;

        public static void AddListener(string eventType, Callback<T, U, V, W> handler) {
            ListenerInternal.OnListenerAdding(eventType, handler);
            eventTable[eventType] = (Callback<T, U, V, W>)eventTable[eventType] + handler;
        }

        public static void RemoveListener(string eventType, Callback<T, U, V, W> handler) {
            ListenerInternal.OnListenerRemoving(eventType, handler);
            eventTable[eventType] = (Callback<T, U, V, W>)eventTable[eventType] - handler;
            ListenerInternal.OnListenerRemoved(eventType);
        }

        public static void Broadcast(string eventType, T arg1, U arg2, V arg3, W arg4) {
            Broadcast(eventType, arg1, arg2, arg3, arg4, ListenerInternal.type);
        }

        public static void Broadcast(string eventType, T arg1, U arg2, V arg3, W arg4, ListenerType mode) {
            ListenerInternal.OnBroadcasting(eventType, mode);
            Delegate d;
            if(eventTable.TryGetValue(eventType, out d)) {
                Callback<T, U, V, W> callback = d as Callback<T, U, V, W>;
                if(callback != null) {
                    callback(arg1, arg2, arg3, arg4);
                } else {
                    throw ListenerInternal.CreateBroadcastSignatureException(eventType);
                }
            }
        }
    }

    // Five parameters
    public static class ListenerManager<T, U, V, W, X> {
        private static Dictionary<string, Delegate> eventTable = ListenerInternal.eventTable;

        public static void AddListener(string eventType, Callback<T, U, V, W, X> handler) {
            ListenerInternal.OnListenerAdding(eventType, handler);
            eventTable[eventType] = (Callback<T, U, V, W, X>)eventTable[eventType] + handler;
        }

        public static void RemoveListener(string eventType, Callback<T, U, V, W, X> handler) {
            ListenerInternal.OnListenerRemoving(eventType, handler);
            eventTable[eventType] = (Callback<T, U, V, W, X>)eventTable[eventType] - handler;
            ListenerInternal.OnListenerRemoved(eventType);
        }

        public static void Broadcast(string eventType, T arg1, U arg2, V arg3, W arg4, X arg5) {
            Broadcast(eventType, arg1, arg2, arg3, arg4, arg5, ListenerInternal.type);
        }

        public static void Broadcast(string eventType, T arg1, U arg2, V arg3, W arg4, X arg5, ListenerType mode) {
            ListenerInternal.OnBroadcasting(eventType, mode);
            Delegate d;
            if(eventTable.TryGetValue(eventType, out d)) {
                Callback<T, U, V, W, X> callback = d as Callback<T, U, V, W, X>;
                if(callback != null) {
                    callback(arg1, arg2, arg3, arg4, arg5);
                } else {
                    throw ListenerInternal.CreateBroadcastSignatureException(eventType);
                }
            }
        }
    }

    // Six parameters
    public static class ListenerManager<T, U, V, W, X, Y> {
        private static Dictionary<string, Delegate> eventTable = ListenerInternal.eventTable;

        public static void AddListener(string eventType, Callback<T, U, V, W, X, Y> handler) {
            ListenerInternal.OnListenerAdding(eventType, handler);
            eventTable[eventType] = (Callback<T, U, V, W, X, Y>)eventTable[eventType] + handler;
        }

        public static void RemoveListener(string eventType, Callback<T, U, V, W, X, Y> handler) {
            ListenerInternal.OnListenerRemoving(eventType, handler);
            eventTable[eventType] = (Callback<T, U, V, W, X, Y>)eventTable[eventType] - handler;
            ListenerInternal.OnListenerRemoved(eventType);
        }

        public static void Broadcast(string eventType, T arg1, U arg2, V arg3, W arg4, X arg5, Y arg6) {
            Broadcast(eventType, arg1, arg2, arg3, arg4, arg5, arg6, ListenerInternal.type);
        }

        public static void Broadcast(string eventType, T arg1, U arg2, V arg3, W arg4, X arg5, Y arg6, ListenerType mode) {
            ListenerInternal.OnBroadcasting(eventType, mode);
            Delegate d;
            if(eventTable.TryGetValue(eventType, out d)) {
                Callback<T, U, V, W, X, Y> callback = d as Callback<T, U, V, W, X, Y>;
                if(callback != null) {
                    callback(arg1, arg2, arg3, arg4, arg5, arg6);
                } else {
                    throw ListenerInternal.CreateBroadcastSignatureException(eventType);
                }
            }
        }
    }
}