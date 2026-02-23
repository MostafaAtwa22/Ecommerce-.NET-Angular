// Microsoft.AspNetCore - Most commonly used across controllers
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.RateLimiting;
global using Microsoft.EntityFrameworkCore;

// AutoMapper - Used in 25+ files
global using AutoMapper;

// Hangfire - Used in multiple files
global using Hangfire;

// Ecommerce.API namespaces - Used extensively across the project
global using Ecommerce.API.BackgroundJobs;
global using Ecommerce.API.Dtos;
global using Ecommerce.API.Dtos.Requests;
global using Ecommerce.API.Dtos.Responses;
global using Ecommerce.API.Errors;
global using Ecommerce.API.Extensions;
global using Ecommerce.API.Helpers;
global using Ecommerce.API.Helpers.Attributes;

// Ecommerce.Core namespaces - Used in 40+ files
global using Ecommerce.Core.Constants;
global using Ecommerce.Core.Entities;
global using Ecommerce.Core.Entities.Chat;
global using Ecommerce.Core.Entities.Emails;
global using Ecommerce.Core.Entities.Identity;
global using Ecommerce.Core.Entities.orderAggregate;
global using Ecommerce.Core.Enums;
global using Ecommerce.Core.googleDto;
global using Ecommerce.Core.Interfaces;
global using Ecommerce.Core.Params;
global using Ecommerce.Core.Spec;

// Ecommerce.Infrastructure - Used in multiple files
global using Ecommerce.Infrastructure.Constants;
global using Ecommerce.Infrastructure.Data;
global using Ecommerce.Infrastructure.Services;
global using Ecommerce.Infrastructure.Settings;

// System namespaces - Common utilities
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Net;
global using System.Reflection;
global using System.Security.Claims;
global using System.Text;
global using System.Threading.Tasks;
