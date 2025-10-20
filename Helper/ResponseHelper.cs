namespace PhotoGallery.Helper
{
    public class ResponseHelper
    {
        public static OkObjectResult OK_Result(dynamic? data, DefaultResponseMessageModel? model)
        {
            return new(new DefaultResponseModel
            {
                Success = true,
                Code = StatusCodes.Status200OK,
                Message = model,
                Data = data,
            });
        }

        public static CreatedResult Created_Result(string endpoint, dynamic? data, DefaultResponseMessageModel? model)
        {
            return new CreatedResult(endpoint, new DefaultResponseModel
            {
                Success = true,
                Code = StatusCodes.Status201Created,
                Message = model,
                Data = data
            });
        }

        public static BadRequestObjectResult Bad_Request(dynamic? data, DefaultResponseMessageModel? model)
        {
            return new(new DefaultResponseModel
            {
                Success = false,
                Code = StatusCodes.Status400BadRequest,
                Message = model,
                Data = data,
            });
        }

        public static NotFoundObjectResult NotFound_Request(dynamic? data, DefaultResponseMessageModel? model)
        {
            return new(new DefaultResponseModel
            {
                Success = false,
                Code = StatusCodes.Status404NotFound,
                Message = model,
                Data = data,
            });
        }

        public static UnauthorizedObjectResult Unauthorized_Request(dynamic? data, DefaultResponseMessageModel? model)
        {
            return new(new DefaultResponseModel
            {
                Success = false,
                Code = StatusCodes.Status401Unauthorized,
                Message = model,
                Data = data,
            });
        }

        public static BadRequestObjectResult InternalServerError_Request(dynamic? data, DefaultResponseMessageModel? model)
        {
            return new(new DefaultResponseModel
            {
                Success = false,
                Code = StatusCodes.Status500InternalServerError,
                Message = model,
                Data = data,
            });
        }

        public static IResult OK_Result_Endpoint(dynamic? data, DefaultResponseMessageModel? model)
        {
            return Results.Ok(new DefaultResponseModel
            {
                Success = true,
                Code = StatusCodes.Status200OK,
                Message = model,
                Data = data
            });
        }

    }
}
