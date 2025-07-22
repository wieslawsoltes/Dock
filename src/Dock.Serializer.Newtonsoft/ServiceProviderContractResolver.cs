// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System;
using Newtonsoft.Json.Serialization;

namespace Dock.Serializer;

/// <summary>
/// Contract resolver that creates objects via <see cref="IServiceProvider"/>.
/// </summary>
internal sealed class ServiceProviderContractResolver : ListContractResolver
{
    private readonly IServiceProvider _provider;

    public ServiceProviderContractResolver(Type listType, IServiceProvider provider)
        : base(listType)
    {
        _provider = provider;
    }

    protected override JsonObjectContract CreateObjectContract(Type objectType)
    {
        var contract = base.CreateObjectContract(objectType);
        contract.DefaultCreator = () =>
            _provider.GetService(objectType) ?? Activator.CreateInstance(objectType)!;
        return contract;
    }
}
