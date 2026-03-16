using BarberShop.Communication.Enums.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberShop.Communication.Models
{
    public class ResponseModel<T>
    {
        public ResponseModel(bool success, int statusCode, EResponseLabel responselabel, string? description = null, T? data = default)
        {
            Success = success;
            StatusCode = statusCode;
            Description = description;
            Data = data;
            ResponseLabel = responselabel;
        }

        public bool Success { get; private set; }
        public int StatusCode { get; private set; }
        public string? Description { get; private set; }
        public T? Data { get; private set; }
        public EResponseLabel ResponseLabel { get; private set; }
    }

    public static class FactoryResponse<T>
    {
        public static ResponseModel<T> Success(T? obj = default, string? description = null) =>
            new(true, 200, EResponseLabel.SUCCESSFUL, description, obj);

        public static ResponseModel<T> SuccessfulCreation(T? obj = default, string? description = null) =>
            new(true, 201, EResponseLabel.SUCCESSFULCREATION, description, obj);

        public static ResponseModel<T> NotFound(string? description = null) =>
            new(false, 404, EResponseLabel.NOT_FOUND, description, default);

        public static ResponseModel<T> Unauthorized(string? description = null) =>
            new(false, 401, EResponseLabel.INVALID_CREDENTIALS, description, default);

        public static ResponseModel<T> UnauthorizedData(T? obj = default, string? description = null) =>
            new(false, 401, EResponseLabel.INVALID_CREDENTIALS, description, obj);

        public static ResponseModel<T> Forbiden(string? description = null) =>
            new(false, 403, EResponseLabel.FORBIDDEN, description, default);

        public static ResponseModel<T> ForbidenLogin(T? obj = default, string? description = null) =>
    new(false, 403, EResponseLabel.FORBIDDEN, description, obj);

        public static ResponseModel<T> Conflict(string? description = null) =>
            new(false, 409, EResponseLabel.ALREADY_EXISTS, description, default);

        public static ResponseModel<T> ConflictLogin(string? description = null, T? obj = default) => new(true, 200, EResponseLabel.ALREADY_EXISTS, description, obj);

        public static ResponseModel<T> InvalidModel(string? description = null) =>
            new(false, 406, EResponseLabel.INVALID_MODEL, description, default);

        public static ResponseModel<T> InvalidModel(string? description = null, T? obj = default) =>
           new(false, 406, EResponseLabel.INVALID_MODEL, description, obj);

        public static ResponseModel<T> BadRequest(string? description = null) =>
            new(false, 400, EResponseLabel.BAD_REQUEST, description, default);

        public static ResponseModel<T> BadRequest(string? description = null, T? obj = default) =>
            new(false, 400, EResponseLabel.BAD_REQUEST, description, obj);

        public static ResponseModel<T> BadRequestErroInterno(string description, T? obj = default) =>
            new(false, 400, EResponseLabel.BAD_REQUEST, $"erro interno: {description}", obj);
    }
}
