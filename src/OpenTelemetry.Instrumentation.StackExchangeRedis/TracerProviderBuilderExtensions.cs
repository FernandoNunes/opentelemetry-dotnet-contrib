// <copyright file="TracerProviderBuilderExtensions.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Instrumentation.StackExchangeRedis;
using OpenTelemetry.Internal;
using StackExchange.Redis;

namespace OpenTelemetry.Trace;

/// <summary>
/// Extension methods to simplify registering of dependency instrumentation.
/// </summary>
public static class TracerProviderBuilderExtensions
{
    /// <summary>
    /// Enables automatic data collection of outgoing requests to Redis.
    /// </summary>
    /// <remarks>
    /// Note: A <see cref="IConnectionMultiplexer"/> will be resolved using the
    /// application <see cref="IServiceProvider"/>.
    /// </remarks>
    /// <param name="builder"><see cref="TracerProviderBuilder"/> being configured.</param>
    /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
    public static TracerProviderBuilder AddRedisInstrumentation(
        this TracerProviderBuilder builder)
        => AddRedisInstrumentation(builder, name: null, connection: null, configure: null);

    /// <summary>
    /// Enables automatic data collection of outgoing requests to Redis.
    /// </summary>
    /// <param name="builder"><see cref="TracerProviderBuilder"/> being configured.</param>
    /// <param name="connection"><see cref="IConnectionMultiplexer"/> to instrument.</param>
    /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
    public static TracerProviderBuilder AddRedisInstrumentation(
        this TracerProviderBuilder builder,
        IConnectionMultiplexer connection)
    {
        Guard.ThrowIfNull(connection);

        return AddRedisInstrumentation(builder, name: null, connection, configure: null);
    }

    /// <summary>
    /// Enables automatic data collection of outgoing requests to Redis.
    /// </summary>
    /// <remarks>
    /// Note: A <see cref="IConnectionMultiplexer"/> will be resolved using the
    /// application <see cref="IServiceProvider"/>.
    /// </remarks>
    /// <param name="builder"><see cref="TracerProviderBuilder"/> being configured.</param>
    /// <param name="configure">Callback to configure options.</param>
    /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
    public static TracerProviderBuilder AddRedisInstrumentation(
        this TracerProviderBuilder builder,
        Action<StackExchangeRedisCallsInstrumentationOptions> configure)
    {
        Guard.ThrowIfNull(configure);

        return AddRedisInstrumentation(builder, name: null, connection: null, configure);
    }

    /// <summary>
    /// Enables automatic data collection of outgoing requests to Redis.
    /// </summary>
    /// <param name="builder"><see cref="TracerProviderBuilder"/> being configured.</param>
    /// <param name="connection"><see cref="IConnectionMultiplexer"/> to instrument.</param>
    /// <param name="configure">Callback to configure options.</param>
    /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
    public static TracerProviderBuilder AddRedisInstrumentation(
        this TracerProviderBuilder builder,
        IConnectionMultiplexer connection,
        Action<StackExchangeRedisCallsInstrumentationOptions> configure)
    {
        Guard.ThrowIfNull(connection);
        Guard.ThrowIfNull(configure);

        return AddRedisInstrumentation(builder, name: null, connection, configure);
    }

    /// <summary>
    /// Enables automatic data collection of outgoing requests to Redis.
    /// </summary>
    /// <remarks>
    /// Note: If an <see cref="IConnectionMultiplexer"/> is not supplied
    /// using the <paramref name="connection"/> parameter it will be
    /// resolved using the application <see cref="IServiceProvider"/>.
    /// </remarks>
    /// <param name="builder"><see cref="TracerProviderBuilder"/> being configured.</param>
    /// <param name="name">Optional name which is used when retrieving options.</param>
    /// <param name="connection">Optional <see cref="IConnectionMultiplexer"/> to instrument.</param>
    /// <param name="configure">Optional callback to configure options.</param>
    /// <returns>The instance of <see cref="TracerProviderBuilder"/> to chain the calls.</returns>
    public static TracerProviderBuilder AddRedisInstrumentation(
        this TracerProviderBuilder builder,
        string? name,
        IConnectionMultiplexer? connection,
        Action<StackExchangeRedisCallsInstrumentationOptions>? configure)
    {
        Guard.ThrowIfNull(builder);

        name ??= Options.DefaultName;

        if (configure != null)
        {
            builder.ConfigureServices(services => services.Configure(name, configure));
        }

        return builder
            .AddSource(StackExchangeRedisCallsInstrumentation.ActivitySourceName)
            .AddInstrumentation(sp =>
            {
                if (connection == null)
                {
                    connection = sp.GetService<IConnectionMultiplexer>();
                    if (connection == null)
                    {
                        throw new InvalidOperationException($"StackExchange.Redis {nameof(IConnectionMultiplexer)} could not be resolved through application {nameof(IServiceProvider)}");
                    }
                }

                return new StackExchangeRedisCallsInstrumentation(
                    connection,
                    sp.GetRequiredService<IOptionsMonitor<StackExchangeRedisCallsInstrumentationOptions>>().Get(name));
            });
    }
}
