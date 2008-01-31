/// Copyright 2007-2008 The Apache Software Foundation.
/// 
/// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
/// this file except in compliance with the License. You may obtain a copy of the 
/// License at 
/// 
///   http://www.apache.org/licenses/LICENSE-2.0 
/// 
/// Unless required by applicable law or agreed to in writing, software distributed 
/// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
/// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
/// specific language governing permissions and limitations under the License.

namespace MassTransit.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using Subscriptions;

    /// <summary>
    /// Defines storage for subscriptions
    /// </summary>
    public interface ISubscriptionStorage : IDisposable
    {
        /// <summary>
        /// Returns a list of endpoints that are subscribed to the specified message type
        /// </summary>
        /// <param name="messageName">Message to find the Uri's for</param>
        /// <returns>A list of endpoints subscribed to the message type</returns>
        IList<Subscription> List(string messageName);

        IList<Subscription> List();

        /// <summary>
        /// Add a message type and endpoint pair to the subscription storage
        /// </summary>
        void Add(string messageName, Uri endpoint);

        /// <summary>
        /// Removes a message from the subscription store.
        /// </summary>
        void Remove(string messageName, Uri endpoint);

        event EventHandler<SubscriptionChangedEventArgs> SubscriptionChanged;
    }
}