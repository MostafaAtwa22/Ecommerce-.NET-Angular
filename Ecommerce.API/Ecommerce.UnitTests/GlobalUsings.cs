// Global using directives for Ecommerce.UnitTests project

// Framework
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Linq.Expressions;
global using System.Reflection;
global using System.Security.Claims;
global using System.Threading;
global using System.Threading.Tasks;

// Testing
global using Xunit;
global using Moq;

// AspNetCore
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Identity;

// Configuration
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;

// Third-party
global using AutoMapper;

// Project - API
global using Ecommerce.API.Controllers;
global using Ecommerce.API.Dtos;
global using Ecommerce.API.Dtos.Requests;
global using Ecommerce.API.Dtos.Responses;
global using Ecommerce.API.Errors;
global using Ecommerce.API.Extensions;
global using Ecommerce.API.BackgroundJobs;

// Project - Core
global using Ecommerce.Core.Entities;
global using Ecommerce.Core.Entities.Identity;
global using Ecommerce.Core.Entities.orderAggregate;
global using Ecommerce.Core.Interfaces;
global using Ecommerce.Core.Spec;
global using Ecommerce.Core.Params;
global using Ecommerce.Core.Enums;
global using Ecommerce.Core.googleDto;

// Project - Infrastructure
global using Ecommerce.Infrastructure.Services;
global using Ecommerce.Infrastructure.Constants;

// Project - UnitTests
global using Ecommerce.UnitTests.Helpers;
