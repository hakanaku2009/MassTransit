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
namespace MassTransit.Context
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using Serialization;
    using Telemetry;
    using Util;


    public abstract class BaseReceiveContext :
        ReceiveContext
    {
        static readonly ContentType DefaultContentType = JsonMessageSerializer.JsonContentType;
        readonly CancellationTokenSource _cancellationTokenSource;
        readonly Lazy<ContentType> _contentType;
        readonly Lazy<Headers> _headers;
        readonly ConcurrentBag<Task> _pendingTasks;
        readonly IPropertyCache _propertyCache;
        readonly IReceiveObserver _receiveObserver;
        readonly Stopwatch _receiveTimer;

        protected BaseReceiveContext(Uri inputAddress, bool redelivered, IReceiveObserver receiveObserver)
        {
            InputAddress = inputAddress;
            Redelivered = redelivered;
            _receiveObserver = receiveObserver;

            _receiveTimer = Stopwatch.StartNew();

            _propertyCache = new PropertyCache();
            _cancellationTokenSource = new CancellationTokenSource();
            _contentType = new Lazy<ContentType>(GetContentType);
            _pendingTasks = new ConcurrentBag<Task>();

            _headers = new Lazy<Headers>(() => new JsonHeaders(ObjectTypeDeserializer.Instance, HeaderProvider));
        }

        protected abstract IHeaderProvider HeaderProvider { get; }
        public bool IsDelivered { get; private set; }
        public bool IsFaulted { get; private set; }
        public Task CompleteTask => Task.WhenAll(_pendingTasks.ToArray());

        public void AddPendingTask(Task task)
        {
            _pendingTasks.Add(task);
        }

        public virtual bool HasPayloadType(Type contextType)
        {
            return _propertyCache.HasPayloadType(contextType);
        }

        public virtual bool TryGetPayload<TPayload>(out TPayload context)
            where TPayload : class
        {
            return _propertyCache.TryGetPayload(out context);
        }

        public virtual TPayload GetOrAddPayload<TPayload>(PayloadFactory<TPayload> payloadFactory)
            where TPayload : class
        {
            return _propertyCache.GetOrAddPayload(payloadFactory);
        }

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;
        public bool Redelivered { get; }
        public Headers TransportHeaders => _headers.Value;

        public virtual Task NotifyConsumed<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType) where T : class
        {
            IsDelivered = true;

            return _receiveObserver.PostConsume(context, duration, consumerType);
        }

        public virtual Task NotifyFaulted<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType, Exception exception) where T : class
        {
            IsFaulted = true;

            return _receiveObserver.ConsumeFault(context, duration, consumerType, exception);
        }

        public virtual Task NotifyFaulted(Exception exception)
        {
            IsFaulted = true;

            return _receiveObserver.ReceiveFault(this, exception);
        }

        public virtual Stream GetBody()
        {
            return GetBodyStream();
        }

        public TimeSpan ElapsedTime => _receiveTimer.Elapsed;
        public Uri InputAddress { get; }
        public ContentType ContentType => _contentType.Value;
        protected abstract Stream GetBodyStream();

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        protected virtual ContentType GetContentType()
        {
            object contentTypeHeader;
            if (_headers.Value.TryGetHeader("Content-Type", out contentTypeHeader))
            {
                var contentType = contentTypeHeader as ContentType;
                if (contentType != null)
                    return contentType;

                var contentTypeString = contentTypeHeader as string;
                if (contentTypeString != null)
                    return new ContentType(contentTypeString);
            }

            return DefaultContentType;
        }
    }
}