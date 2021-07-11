using fr4nc3.com.containers.DTO;
using fr4nc3.com.containers.Models;
using Microsoft.Azure.Storage.Blob;
using System.Collections.Generic;

namespace fr4nc3.com.containers.Services
{
    /// <summary>
    /// Container service use for 
    /// </summary>
    public class ContainerService
    {
        /// <summary>
        ///  access level for a container
        /// </summary>
        /// <param name="containername"></param>
        /// <returns></returns>
        public static BlobContainerPublicAccessType GetContainerAccessType(string containername)
        {

            return containername.Contains("public") ? BlobContainerPublicAccessType.Blob : BlobContainerPublicAccessType.Off;

        }
        public static bool IsPublic (BlobContainerPublicAccessType accessType)
        {
            return accessType == BlobContainerPublicAccessType.Blob;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="containername"></param>
        /// <returns>list of validation error</returns>
        public static List<ErrorResponse> ValidateContainer(string containername)
        {
            List<ErrorResponse> errors = new List<ErrorResponse>();
            ErrorResponse errorResponse = new ErrorResponse();
            if (string.IsNullOrWhiteSpace(containername))
            {
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ParameterCannotBeNull}");
                errorResponse.parameterName = "containername";
                errorResponse.parameterValue = $"{containername}";
                errors.Add(errorResponse);
            }
            if (containername.Length < 3)
            {
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ParameterTooSmall}");
                errorResponse.parameterName = "containername";
                errorResponse.parameterValue = $"{containername}";
                errors.Add(errorResponse);
            }
            if (containername.Length > 63)
            {
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ParameterTooLarge}");
                errorResponse.parameterName = "containername";
                errorResponse.parameterValue = $"{containername}";
                errors.Add(errorResponse);

            }
            return errors;
        }
        public static List<ErrorResponse> ValidateFilename(string fileName)
        {
            List<ErrorResponse> errors = new List<ErrorResponse>();
            ErrorResponse errorResponse = new ErrorResponse();

            if (string.IsNullOrWhiteSpace(fileName))
            {
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ParameterCannotBeNull}");
                errorResponse.parameterName = "fileName";
                errorResponse.parameterValue = $"{fileName}";
                errors.Add(errorResponse);

            }
            if (fileName.Length > 75)
            {
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ParameterTooLarge}");
                errorResponse.parameterName = "fileName";
                errorResponse.parameterValue = $"{fileName}";
                errors.Add(errorResponse);
            }
            if (fileName.Length == 0)
            {
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ParameterTooSmall}");
                errorResponse.parameterName = "fileName";
                errorResponse.parameterValue = $"{fileName}";
                errors.Add(errorResponse);
            }
            return errors;
        }

        public static List<ErrorResponse> ValidateFileParams(string containername, string fileName)
        {
            List<ErrorResponse> errors = new List<ErrorResponse>();
            errors.AddRange(ValidateContainer(containername));
            errors.AddRange(ValidateFilename(fileName));
            return errors;
        }
    }
}
