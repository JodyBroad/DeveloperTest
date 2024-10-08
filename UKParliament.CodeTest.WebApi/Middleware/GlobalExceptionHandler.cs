﻿using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using UKParliament.CodeTest.Services.Exceptions;
using UKParliament.CodeTest.WebApi.Models;

namespace UKParliament.CodeTest.WebApi.Middleware
{
    // implements IException handler interface
    public class GlobalExceptionHandler : IExceptionHandler
    {
        // injecting the logger
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        // when an exception occurs the message is logged - an ErrorResponse object is created, and the status code and title are set based on the exception type
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                $"An error occurred while processing your request: {exception.Message}");

            var errorResponse = new ErrorResponse
            {
                  Message = exception.Message
            };

            switch (exception)
            {

                case FileNotFoundException:
                    errorResponse.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.Title = exception.GetType().Name;
                    break;

                case AlreadyCompleteException:
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Title = exception.GetType().Name;
                    break;

                case ValidationException:
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Title = exception.GetType().Name;
                    break;

                case ArgumentNullException:
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Title = exception.GetType().Name;
                    break;

                // not actually raising this anywhere, do I need to change the ones that fail model validation into these exceptions perhaps?
                case BadHttpRequestException:
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Title = exception.GetType().Name;
                    break;

                default:
                    errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Title = "Internal Server Error";
                    break;
            }

            httpContext.Response.StatusCode = errorResponse.StatusCode;

            await httpContext
                .Response
                .WriteAsJsonAsync(errorResponse, cancellationToken);

            return true;
        }
    }
}
