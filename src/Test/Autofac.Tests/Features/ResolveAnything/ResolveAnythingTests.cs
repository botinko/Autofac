﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Features.ResolveAnything;
using Autofac.Tests.Scenarios.RegistrationSources;
using NUnit.Framework;

namespace Autofac.Tests.Features.ResolveAnything
{
    [TestFixture]
    public class ResolveAnythingTests
    {
        class NotRegisteredType { }

        [Test]
        public void AConcreteTypeNotRegisteredWithTheContainerWillBeProvided()
        {
            var container = CreateResolveAnythingContainer();
            Assert.That(container.IsRegistered<NotRegisteredType>());
        }

        abstract class AbstractType { }

        [Test]
        public void AnAbstractTypeNotRegisteredWithTheContainerWillNotBeProvided()
        {
            var container = CreateResolveAnythingContainer();
            Assert.IsFalse(container.IsRegistered<AbstractType>());
        }

        interface IInterfaceType { }

        [Test]
        public void AnInterfaceTypeNotRegisteredWithTheContainerWillNotBeProvided()
        {
            var container = CreateResolveAnythingContainer();
            Assert.IsFalse(container.IsRegistered<IInterfaceType>());
        }

        [Test]
        public void TypesFromTheRegistrationSourceAreProvidedToOtherSources()
        {
            var container = CreateResolveAnythingContainer();
            // The RS for Func<> is getting the NotRegisteredType from the resolve-anything source
            Assert.That(container.IsRegistered<Func<NotRegisteredType>>());
            Assert.AreEqual(1, container.Resolve<IEnumerable<Func<NotRegisteredType>>>().Count());
        }

        [Test]
        public void AServiceProvideByAnotherRegistrationSourceWillNotBeProvided()
        {
            var cb = new ContainerBuilder();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource(), false);
            cb.RegisterSource(new ObjectRegistrationSource(), false);
            var container = cb.Build();
            Assert.IsTrue(container.IsRegistered<object>());
            Assert.AreEqual(1, container.Resolve<IEnumerable<object>>().Count());
        }

        class RegisteredType { }

        [Test]
        public void AServiceAlreadyRegisteredWillNotBeProvided()
        {
            var cb = new ContainerBuilder();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource(), false);
            cb.RegisterType<RegisteredType>();
            var container = cb.Build();
            Assert.IsTrue(container.IsRegistered<RegisteredType>());
            Assert.AreEqual(1, container.Resolve<IEnumerable<object>>().Count());
        }

        [Test]
        public void AServiceRegisteredBeforeTheSourceWillNotBeProvided()
        {
            var cb = new ContainerBuilder();
            cb.RegisterType<RegisteredType>();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource(), false);
            var container = cb.Build();

            Assert.IsTrue(container.IsRegistered<RegisteredType>());
            Assert.AreEqual(1, container.Resolve<IEnumerable<object>>().Count());
        }

        static IContainer CreateResolveAnythingContainer()
        {
            var cb = new ContainerBuilder();
            cb.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource(), false);
            return cb.Build();
        }

    }
}
