// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Telemetry
{
    using System;
    using System.Threading;
    using Context;


    public sealed class TelemetryLogEventContext :
        LogEventContext
    {
        readonly PayloadCache _payloadCache;

        public TelemetryLogEventContext(TelemetryLogEvent logEvent, CancellationToken cancellationToken = default(CancellationToken))
        {
            LogEvent = logEvent;
            CancellationToken = cancellationToken;

            _payloadCache = new PayloadCache();
        }

        public TelemetryLogEvent LogEvent { get; }

        public bool HasPayloadType(Type contextType)
        {
            return _payloadCache.HasPayloadType(contextType);
        }

        public bool TryGetPayload<TPayload>(out TPayload context)
            where TPayload : class
        {
            return _payloadCache.TryGetPayload(out context);
        }

        public TPayload GetOrAddPayload<TPayload>(PayloadFactory<TPayload> payloadFactory)
            where TPayload : class
        {
            return _payloadCache.GetOrAddPayload(payloadFactory);
        }

        public CancellationToken CancellationToken { get; }
    }
}