﻿// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.EntityFrameworkIntegration
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration;
    using Internals.Reflection;
    using MassTransit.Saga;
    using Util;


    public abstract class SagaClassMapping<T> :
        EntityTypeConfiguration<T>
        where T : class, ISaga
    {
        protected SagaClassMapping()
        {
            ReadWriteProperty<T> property;
            if (!TypeMetadataCache<T>.ReadWritePropertyCache.TryGetProperty("CorrelationId", out property))
                throw new ConfigurationException("The CorrelationId property must be read/write for use with Entity Framework. Add a setter to the property.");

            HasKey(t => t.CorrelationId);

            Property(t => t.CorrelationId)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        }
    }
}