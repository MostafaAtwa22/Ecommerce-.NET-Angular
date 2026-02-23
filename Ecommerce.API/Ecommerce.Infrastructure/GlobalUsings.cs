// Global using directives for Ecommerce.Infrastructure project

// Framework
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Linq.Expressions;
global using System.Reflection;
global using System.Security.Claims;
global using System.Text;
global using System.Text.Json;
global using System.Security.Cryptography;

// Entity Framework
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Infrastructure;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.EntityFrameworkCore.Metadata;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
global using Microsoft.EntityFrameworkCore.Diagnostics;

// Identity
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using Microsoft.AspNetCore.Http;

// Configuration
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

// Project - Core
global using Ecommerce.Core.Entities;
global using Ecommerce.Core.Entities.Identity;
global using Ecommerce.Core.Entities.orderAggregate;
global using Ecommerce.Core.Entities.Chat;
global using Ecommerce.Core.Entities.Emails;
global using Ecommerce.Core.Interfaces;
global using Ecommerce.Core.Spec;
global using Ecommerce.Core.Constants;
global using Ecommerce.Core.Enums;
global using Ecommerce.Core.googleDto;

// Project - Infrastructure
global using Ecommerce.Infrastructure.Data;
global using Ecommerce.Infrastructure.Constants;
global using Ecommerce.Infrastructure.Settings;
