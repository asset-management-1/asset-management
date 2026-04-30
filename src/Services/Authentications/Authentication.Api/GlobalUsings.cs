// Global using directives

global using System.Text.Json.Serialization;
global using Authentication.Api.Extensions;
global using Authentication.Application.Extensions;
global using Authentication.Infrastructure.Extensions;
global using Be.Haven.ApiCommon.Extensions;
global using Be.Haven.ApiCommon.Middlewares;
global using Be.Haven.ApiCommon.Routing;
global using Be.Haven.Cache.Extensions;
global using Be.Haven.Core.Extensions.DI;
global using Be.Haven.Core.Factories;
global using Be.Haven.Core.Filters;
global using Be.Haven.Shared.Dtos.Options;
global using MediatR;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.ApplicationModels;
global using Microsoft.AspNetCore.Server.Kestrel.Core;
global using static Be.Haven.Shared.Constants.AppConstants.SystemVariable;
global using static Be.Haven.Shared.Constants.AuthConstants;
global using static Be.Haven.Shared.Constants.CoreLogConstants;
global using static Be.Haven.Shared.Constants.ApiConstants;
global using static Be.Haven.Shared.Constants.AuthConstants.SystemMessage;