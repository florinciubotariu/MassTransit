﻿// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.ActiveMqTransport.Builders
{
    using System;
    using EndpointSpecifications;
    using GreenPipes;
    using MassTransit.Builders;
    using Topology;
    using Topology.Builders;
    using Transport;
    using Transports;


    public class ActiveMqReceiveEndpointBuilder :
        ReceiveEndpointBuilder,
        IReceiveEndpointBuilder
    {
        readonly bool _bindMessageTopics;
        readonly IActiveMqEndpointConfiguration _configuration;
        readonly ActiveMqHost _host;
        readonly BusHostCollection<ActiveMqHost> _hosts;

        public ActiveMqReceiveEndpointBuilder(ActiveMqHost host, BusHostCollection<ActiveMqHost> hosts, bool bindMessageTopics,
            IActiveMqEndpointConfiguration configuration)
            : base(configuration)
        {
            _bindMessageTopics = bindMessageTopics;
            _configuration = configuration;
            _host = host;
            _hosts = hosts;
        }

        public override ConnectHandle ConnectConsumePipe<T>(IPipe<ConsumeContext<T>> pipe)
        {
            if (_bindMessageTopics)
                _configuration.Topology.Consume
                    .GetMessageTopology<T>()
                    .Bind();

            return base.ConnectConsumePipe(pipe);
        }

        public IActiveMqReceiveEndpointTopology CreateReceiveEndpointTopology(Uri inputAddress, ReceiveSettings settings)
        {
            var brokerTopology = BuildTopology(settings);

            return new ActiveMqReceiveEndpointTopology(_configuration, inputAddress, _host, _hosts, brokerTopology);
        }

        BrokerTopology BuildTopology(ReceiveSettings settings)
        {
            var topologyBuilder = new ReceiveEndpointBrokerTopologyBuilder();

            topologyBuilder.Queue = topologyBuilder.CreateQueue(settings.EntityName, settings.Durable, settings.AutoDelete);

            _configuration.Topology.Consume.Apply(topologyBuilder);

            return topologyBuilder.BuildTopologyLayout();
        }
    }
}