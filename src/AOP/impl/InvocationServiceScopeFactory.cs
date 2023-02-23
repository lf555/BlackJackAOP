﻿using Microsoft.Extensions.DependencyInjection;

namespace BlackJackAOP
{
    [NonInterceptable]
    internal class InvocationServiceScopeFactory : IInvocationServiceScopeFactory
    {
        private readonly IApplicationServicesAccessor _applicationServicesAccessor;
        public InvocationServiceScopeFactory(IApplicationServicesAccessor applicationServicesAccessor) => _applicationServicesAccessor = applicationServicesAccessor ?? throw new ArgumentNullException(nameof(applicationServicesAccessor));
        public IServiceScope CreateInvocationScope()=> _applicationServicesAccessor.ApplicationServices.CreateScope();
    }
}