﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using AmplitudeSharp.Utils;
using Newtonsoft.Json;

namespace AmplitudeSharp.Api
{
    public class AmplitudeEvent : Dictionary<string, object>, IAmplitudeEvent
    {
        private static int s_eventId;
        public static string LibraryVersion;

        static AmplitudeEvent()
        {
            LibraryVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
        }

        [JsonIgnore]
        public string UserId
        {
            get
            {
                return this.TryGet("user_id") as string;
            }
            set
            {
                this["user_id"] = value;
            }
        }

        [JsonIgnore]
        public string EventType
        {
            get
            {
                return this.TryGet("event_type") as string;
            }
            set
            {
                this["event_type"] = value;
            }
        }

        [JsonIgnore]
        public long Time
        {
            get
            {
                return (long)this.TryGet("time");
            }
            set
            {
                this["time"] = value;
            }
        }

        [JsonIgnore]
        public long SessionId
        {
            get
            {
                return (long)this.TryGet("session_id");
            }
            set
            {
                this["session_id"] = value;
            }
        }

        [JsonIgnore]
        public int EventId
        {
            get
            {
                return (int)this.TryGet("event_id");
            }
            set
            {
                this["event_id"] = value;
            }
        }

        [JsonProperty(propertyName: "event_properties")]
        public Dictionary<string, object> Properties { get; set; }

        public AmplitudeEvent()
        {
            EventId = Interlocked.Increment(ref s_eventId);
        }

        public AmplitudeEvent(string eventName, object eventProperties)
            : this()
        {
            Time = DateTime.UtcNow.ToUnixEpoch();
            EventType = Uri.EscapeUriString(eventName);
            Properties = ToDictionary(eventProperties);

            this["library"] = $"{nameof(AmplitudeSharp)}/{LibraryVersion}";
            this["ip"] = "$remote";

            if (Properties?.Count > 0)
            {
                this["event_properties"] = eventProperties;
            }
        }

        /// <summary>
        /// Convert an object to a dictionary
        /// </summary>
        private Dictionary<string, object> ToDictionary<T>(T obj)
        {
            if (obj != null)
            {
                var amplitudeProps = new Dictionary<string, object>();

                foreach (PropertyInfo property in obj.GetType().GetRuntimeProperties())
                {
                    amplitudeProps[property.Name] = property.GetValue(obj, null);
                }

                if (amplitudeProps.Count != 0)
                {
                    return amplitudeProps;
                }
            }

            return null;
        }
    }
}
