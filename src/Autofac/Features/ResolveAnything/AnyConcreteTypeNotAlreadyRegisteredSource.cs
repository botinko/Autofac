﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;

namespace Autofac.Features.ResolveAnything;

/// <summary>
/// Provides registrations on-the-fly for any concrete type not already registered with
/// the container.
/// </summary>
public class AnyConcreteTypeNotAlreadyRegisteredSource : IRegistrationSource
{
    private readonly Func<Type, bool> _predicate;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnyConcreteTypeNotAlreadyRegisteredSource"/> class.
    /// </summary>
    public AnyConcreteTypeNotAlreadyRegisteredSource()
        : this(t => true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnyConcreteTypeNotAlreadyRegisteredSource"/> class.
    /// </summary>
    /// <param name="predicate">A predicate that selects types the source will register.</param>
    public AnyConcreteTypeNotAlreadyRegisteredSource(Func<Type, bool> predicate)
    {
        _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
    }

    /// <summary>
    /// Retrieve registrations for an unregistered service, to be used
    /// by the container.
    /// </summary>
    /// <param name="service">The service that was requested.</param>
    /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
    /// <returns>Registrations providing the service.</returns>
    public IEnumerable<IComponentRegistration> RegistrationsFor(
        Service service,
        Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
    {
        if (registrationAccessor == null)
        {
            throw new ArgumentNullException(nameof(registrationAccessor));
        }

        var ts = service as TypedService;
        if (ts == null || ts.ServiceType == typeof(string))
        {
            return Enumerable.Empty<IComponentRegistration>();
        }

        var serviceType = ts.ServiceType;
        if (!serviceType.IsClass ||
            serviceType.IsSubclassOf(typeof(Delegate)) ||
            serviceType.IsAbstract ||
            serviceType.IsGenericTypeDefinition ||
            !_predicate(ts.ServiceType) ||
            registrationAccessor(service).Any())
        {
            return Enumerable.Empty<IComponentRegistration>();
        }

        if (serviceType.IsGenericType && !ShouldRegisterGenericService(serviceType))
        {
            return Enumerable.Empty<IComponentRegistration>();
        }

        var builder = RegistrationBuilder.ForType(serviceType);
        RegistrationConfiguration?.Invoke(builder);
        return new[] { builder.CreateRegistration() };
    }

    /// <summary>
    /// Gets a value indicating whether the registrations provided by this source are 1:1 adapters on top
    /// of other components (e.g., Meta, Func, or Owned).
    /// </summary>
    public bool IsAdapterForIndividualComponents => false;

    /// <summary>
    /// Gets or sets an expression used to configure generated registrations.
    /// </summary>
    /// <value>
    /// A <see cref="System.Action{T}"/> that can be used to modify the behavior
    /// of registrations that are generated by this source.
    /// </value>
    public Action<IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>>? RegistrationConfiguration { get; set; }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> that represents the current <see cref="object"/>.
    /// </returns>
    public override string ToString()
    {
        return AnyConcreteTypeNotAlreadyRegisteredSourceResources.AnyConcreteTypeNotAlreadyRegisteredSourceDescription;
    }

    private static bool ShouldRegisterGenericService(Type type)
    {
        var genericType = type.GetGenericTypeDefinition();

        return genericType != typeof(Lazy<>) &&
               !IsInsideAutofac(genericType);
    }

    private static bool IsInsideAutofac(Type type)
    {
        return typeof(IRegistrationSource).Assembly == type.Assembly;
    }
}
