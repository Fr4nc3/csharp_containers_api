using fr4nc3.com.containers.Common;
using fr4nc3.com.containers.DTO;
using fr4nc3.com.containers.Models;
using fr4nc3.com.containers.Services;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace fr4nc3.com.containers.Controllers
{
    /// <summary>
    /// Provides implementation for the task resource
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Produces("application/json")]
    [Route("api/v1/[Controller]")]
    [ApiController]
    public class ContainersController : ControllerBase
    {
        /// <summary>
        /// The get by container and identifier route name
        /// </summary>
        private const string GetByContainerAndIdRouteName = "GetByContainerAndIdRouteName";



        /// <summary>
        /// Logger instance
        /// </summary>
        private readonly ILogger Logger;
        


        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// The App Insights Telemetry Client
        /// </summary>
        private readonly TelemetryClient TelemetryClient;
        private IDictionary<string, string> traceVariables;
        /// <summary>
        /// Gets the storage connection string.
        /// </summary>
        /// <value>
        /// The storage connection string.
        /// </value>
        public string StorageConnectionString
        {
            get
            {
                return Configuration["StorageConnection"];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainersController" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">the logger</param>
        /// <param name="telemetryClient"> the client for telemetry</param>
        public ContainersController(IConfiguration configuration, ILogger<ContainersController> logger, TelemetryClient telemetryClient)
        {
            Logger = logger;
            Configuration = configuration;
            TelemetryClient = telemetryClient;
            traceVariables = new Dictionary<string, string>();
        }
        private CloudBlobContainer GetContainer(string containername)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(StorageConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container. 
            CloudBlobContainer container = blobClient.GetContainerReference(containername);

            return container;

        }
        private CloudBlockBlob GetCloudBlockBlob(CloudBlobContainer container, string fileName)
        {
            // Retrieve reference to a blob named the blob specified by the caller
            return container.GetBlockBlobReference(fileName);
            
        }
        private async Task<bool> BlockBlobExist(CloudBlockBlob blockBlob)
        {
            // if blockblob is null
            if(blockBlob == null)
            {
                return false;
            }
            // file exist or not
            bool fileExist = await blockBlob.ExistsAsync();

            return fileExist; 

        }

        private async Task<bool> ContainerExist(CloudBlobContainer container)
        {
            // if container is null
            if(container == null)
            {
                return false;
            }
            // container exist or not
            bool containerExist = await container.ExistsAsync();

            return containerExist; 

        }

        /// <summary>
        /// Allows upload a file to a public or private container
        /// </summary>
        /// <param name="containername">container name</param>
        /// <param name="fileName">file name </param>
        /// <param name="fileData">file</param>
        /// <returns>201 created or 204 updated</returns>

        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)] // updated
        [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status400BadRequest)]
        //[Route("{containername:length(3,63)}/contentfiles/{fileName:length(75)}")]
        [Route("/api/v1/{containername}/contentfiles/{fileName}")]
        [HttpPut]
        public async Task<IActionResult> UploadFile([FromRoute] string containername, [FromRoute] string fileName, IFormFile fileData)
        {
            // initials variables to process error and type of upload
            List<ErrorResponse> errors = ContainerService.ValidateFileParams(containername, fileName);
            // trace variables
            traceVariables.Add("fileName", fileName);
            traceVariables.Add("containername", containername);
            TelemetryClient.TrackTrace("UploadFilesURLVariables", traceVariables);
            HttpStatusCode statusCode = HttpStatusCode.Created;
            ErrorResponse errorResponse = new ErrorResponse();
            if (errors.Count > 0)// invalid variables
            {
                TelemetryClient.TrackEvent("InvalidParamsUploadFile", traceVariables);
                Logger.LogWarning(LoggingEvents.InvalidItem, $"Invalid parameters fileName = {fileName}, containername = {containername}");
                return BadRequest(errors);
            }
            // The container name must be lower case
            containername = containername.ToLower();
            BlobContainerPublicAccessType containerAccess = ContainerService.GetContainerAccessType(containername);
            bool isPublic = ContainerService.IsPublic(containerAccess);
            try
            {
                if (fileData == null || fileData.Length <= 0)// file is not coming 
                {
                    TelemetryClient.TrackEvent("InvalidFileDataUploadFile", traceVariables);
                    Logger.LogWarning(LoggingEvents.InvalidItem, $"Invalid File, fileName = {fileName}, containername = {containername}");
                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ParameterCannotBeNull}");
                    errorResponse.parameterName = "fileData";
                    errorResponse.parameterValue = null;
                    errors.Add(errorResponse);
                    return BadRequest(errors);
                }

                CloudBlobContainer container = GetContainer(containername);

                bool containerExist = await ContainerExist(container);

                if (!containerExist) 
                {
                    TelemetryClient.TrackEvent("CreateContainerUploadFile", traceVariables);
                    await container.CreateAsync();
                    // set the proper access level
                    await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = containerAccess });
                }

                CloudBlockBlob blockBlob = GetCloudBlockBlob(container, fileName);

                bool fileExist = await BlockBlobExist(blockBlob);
                if (fileExist) // file exist we update file
                {
                    statusCode = HttpStatusCode.NoContent;
                }

                using (Stream uploadedFileStream = fileData.OpenReadStream())
                {
                    blockBlob.Properties.ContentType = fileData.ContentType;
                    await blockBlob.UploadFromStreamAsync(uploadedFileStream);
                }
                if (statusCode == HttpStatusCode.Created)
                {
                   
                    Logger.LogInformation(LoggingEvents.UploadItem, $"Upload fileName = {fileName}, containername = {containername}");
                    if (isPublic)
                    {
                        TelemetryClient.TrackEvent("CreateNewFilePrivate", traceVariables);
                        // public container
                        return Created(blockBlob.Uri.AbsoluteUri, null);

                    }
                    else
                    {
                        TelemetryClient.TrackEvent("CreateNewFilePublic", traceVariables);
                        // private container
                        return CreatedAtRoute(GetByContainerAndIdRouteName, new { containername = containername, fileName = fileName }, null);
                    }

                }
                else
                {
                    TelemetryClient.TrackEvent("UpdateFileUploaded", traceVariables);
                    // update file
                    Logger.LogInformation(LoggingEvents.UpdateItem, $"Update fileName = {fileName}, containername = {containername}");
                    return NoContent();
                }
            }
            catch (StorageException se) // storage exceptions 
            {
                WebException webException = se.InnerException as WebException;

                TelemetryClient.TrackException(se, traceVariables);
                if (webException != null)
                {
                    HttpWebResponse httpWebResponse = webException.Response as HttpWebResponse;

                    if (httpWebResponse != null)// storege error 
                    {
                        
                        Logger.LogWarning(LoggingEvents.UploadItemError, $"Upload error fileName = {fileName}, containername = {containername}", httpWebResponse.StatusDescription);
                        (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.EntityNoUploaded}");
                        errorResponse.parameterName = "fileData";
                        errorResponse.parameterValue = null;
                        errorResponse.errorDescription += $" {httpWebResponse.StatusDescription}";
                        errors.Add(errorResponse);
                        return BadRequest(errors);

                    }
                    else
                    {
                        // unknown web exception
                        Logger.LogWarning(LoggingEvents.InternalError, $"Upload error parameters fileName = {fileName}, containername = {containername}", webException.Message);
                        (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ServerError}");
                        errorResponse.parameterName = "fileData";
                        errorResponse.parameterValue = null;
                        errorResponse.errorDescription += $" {webException.Message}";
                        return BadRequest(errors);
                    }

                }
                else
                {
                    Logger.LogWarning(LoggingEvents.InternalError, $"upload error fileName = {fileName}, containername = {containername}", se.Message);
                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ServerError}");
                    errorResponse.parameterName = "fileData";
                    errorResponse.parameterValue = null;
                    errorResponse.errorDescription += $" {se.Message}";
                    return BadRequest(errors);
                }
            }
            catch (Exception ex)
            {
                TelemetryClient.TrackException(ex, traceVariables);
                // upload exception
                Logger.LogWarning(LoggingEvents.UploadItemError, $"upload fileName = {fileName}, containername = {containername}", ex.Message);
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ServerError}");
                errorResponse.parameterName = "fileData";
                errorResponse.parameterValue = null;
                errorResponse.errorDescription += $" {ex.Message}";
                return BadRequest(errors);

            }

        }
        /// <summary>
        ///  Update existing file
        /// </summary>
        /// <param name="containername">container name</param>
        /// <param name="fileName">file name </param>
        /// <param name="fileData">file</param>
        /// <returns>204 updated</returns>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [Route("/api/v1/{containername}/contentfiles/{fileName}")]
        [HttpPatch]
        public async Task<IActionResult> PatchFile([FromRoute] string containername, [FromRoute] string fileName, IFormFile fileData)
        {
            List<ErrorResponse> errors = ContainerService.ValidateFileParams(containername, fileName);
            // trace variables
            traceVariables.Add("fileName", fileName);
            traceVariables.Add("containername", containername);

            TelemetryClient.TrackTrace("PatchFilesURLVariables", traceVariables);
            ErrorResponse errorResponse = new ErrorResponse();
            if (errors.Count > 0) // invalid variables
            {
                TelemetryClient.TrackEvent("InvalidParamsPatchFile", traceVariables);
                Logger.LogWarning(LoggingEvents.InvalidItem, $"Invalid parameters fileName = {fileName}, containername = {containername}");
                return BadRequest(errors);
            }
            // The container name must be lower case
            containername = containername.ToLower();
            try
            {
                if (fileData == null || fileData.Length <= 0) // file is invalid
                {
                    TelemetryClient.TrackEvent("InvalidFileDataPatchFile", traceVariables);
                    Logger.LogWarning(LoggingEvents.InvalidItem, $"Invalid file fileName = {fileName}, containername = {containername}");
                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ParameterCannotBeNull}");
                    errorResponse.parameterName = "fileData";
                    errorResponse.parameterValue = null;
                    errors.Add(errorResponse);
                    return BadRequest(errors);
                }


                // Retrieve a reference to a container. 
                CloudBlobContainer container = GetContainer(containername);

                bool containerExist = await ContainerExist(container);

                if (!containerExist) // error if the container doesn't exist
                {
                    TelemetryClient.TrackEvent("ContainerNoPatchFile", traceVariables);
                    Logger.LogWarning(LoggingEvents.ContainerNotFound, $"Invalid container doens't exist fileName = {fileName}, containername = {containername}");
                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.EntityNoFound}");
                    errorResponse.parameterName = "containername";
                    errorResponse.parameterValue = containername;
                    return StatusCode((int)HttpStatusCode.NotFound, errorResponse);
                }

                // Retrieve reference to a blob named the blob specified by the caller
                CloudBlockBlob blockBlob =  GetCloudBlockBlob(container, fileName);

                bool fileExist = await BlockBlobExist(blockBlob);
                if (!fileExist) // error if the file doesn't exist
                {
                    TelemetryClient.TrackEvent("FileNoExistPatchFile", traceVariables);
                    Logger.LogWarning(LoggingEvents.UpdateItemNotFound, $"Invalid file doens't exist fileName = {fileName}, containername = {containername}");
                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.EntityNoFound}");
                    errorResponse.parameterName = "fileName";
                    errorResponse.parameterValue = fileName;
                    return StatusCode((int)HttpStatusCode.NotFound, errorResponse);
                }
                TelemetryClient.TrackEvent("FileUpdatedPatchFile", traceVariables);
                Logger.LogInformation(LoggingEvents.UpdateItem, $"Update Item fileName = {fileName}, containername = {containername}");
                using (Stream uploadedFileStream = fileData.OpenReadStream())
                {
                    blockBlob.Properties.ContentType = fileData.ContentType;
                    await blockBlob.UploadFromStreamAsync(uploadedFileStream);
                }
                return NoContent();
            }
            catch (StorageException se)
            {
                WebException webException = se.InnerException as WebException;

                TelemetryClient.TrackException(se, traceVariables);
                if (webException != null) // storage error exception
                {
                    HttpWebResponse httpWebResponse = webException.Response as HttpWebResponse;

                    if (httpWebResponse != null)
                    {
                        Logger.LogWarning(LoggingEvents.UpdateItemError, $"Update file error fileName = {fileName}, containername = {containername}", httpWebResponse.StatusDescription);
                        (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.EntityNoUploaded}");
                        errorResponse.parameterName = "fileData";
                        errorResponse.parameterValue = null;
                        errorResponse.errorDescription += $" {httpWebResponse.StatusDescription}";
                        errors.Add(errorResponse);
                        return BadRequest(errors);

                    }
                    else // unknow storage exception
                    {
                        Logger.LogWarning(LoggingEvents.InternalError, $"Update file error fileName = {fileName}, containername = {containername}", webException.Message);
                        (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ServerError}");
                        errorResponse.parameterName = "fileData";
                        errorResponse.parameterValue = null;
                        errorResponse.errorDescription += $" {webException.Message}";
                        return BadRequest(errors);
                    }

                }
                else // error exception
                {
                    Logger.LogWarning(LoggingEvents.InternalError, $"Update file error fileName = {fileName}, containername = {containername}", se.Message);
                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ServerError}");
                    errorResponse.parameterName = "fileData";
                    errorResponse.parameterValue = null;
                    errorResponse.errorDescription += $" {se.Message}";
                    return BadRequest(errors);
                }
            }
            catch (Exception ex)
            {
                TelemetryClient.TrackException(ex, traceVariables);
                Logger.LogWarning(LoggingEvents.UpdateItemError, $"Update file error fileName = {fileName}, containername = {containername}", ex.Message);
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ServerError}");
                errorResponse.parameterName = "fileData";
                errorResponse.parameterValue = null;
                errorResponse.errorDescription += $" {ex.Message}";
                return BadRequest(errors);

            }
        }
        /// <summary>
        ///  Delete existing file
        /// </summary>
        /// <param name="containername">container name</param>
        /// <param name="fileName">file name </param>
        /// <returns>204 deleted</returns>
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status400BadRequest)]
        [Route("/api/v1/{containername}/contentfiles/{fileName}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteFile([FromRoute] string containername, [FromRoute] string fileName)
        {

            List<ErrorResponse> errors = ContainerService.ValidateFileParams(containername, fileName);
            // trace variables
            traceVariables.Add("fileName", fileName);
            traceVariables.Add("containername", containername);
            TelemetryClient.TrackTrace("DeleteFilesURLVariables", traceVariables);
            ErrorResponse errorResponse = new ErrorResponse();
            if (errors.Count > 0)// invalid variables
            {
                Logger.LogWarning(LoggingEvents.InvalidItem, $"Invalid parameters fileName = {fileName}, containername = {containername}");
                return BadRequest(errors);
            }
            // The container name must be lower case
            containername = containername.ToLower();
            try
            {

                // Retrieve a reference to a container. 
                CloudBlobContainer container = GetContainer(containername);

                bool containerExist = await ContainerExist(container);

                if (!containerExist) // error if the container doesn't exist
                {
                    TelemetryClient.TrackEvent("ContainerNoExistDeleteFile", traceVariables);
                    Logger.LogWarning(LoggingEvents.ContainerNotFound, $"Invalid Container fileName = {fileName}, containername = {containername}");
                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.EntityNoFound}");
                    errorResponse.parameterName = "containername";
                    errorResponse.parameterValue = containername;
                    return StatusCode((int)HttpStatusCode.NotFound, errorResponse);
                }

                // Retrieve reference to a blob named the blob specified by the caller
                CloudBlockBlob blockBlob = GetCloudBlockBlob(container, fileName);

                bool fileExist = await BlockBlobExist(blockBlob);
                if (!fileExist) // error file doens't exist
                {
                    TelemetryClient.TrackEvent("FileNoExistDeleteFile", traceVariables);
                    Logger.LogWarning(LoggingEvents.DeleteItemNotFound, $"Invalid parameters fileName = {fileName}, containername = {containername}");
                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.EntityNoFound}");
                    errorResponse.parameterName = "fileName";
                    errorResponse.parameterValue = fileName;
                    return StatusCode((int)HttpStatusCode.NotFound, errorResponse);
                }
                Logger.LogInformation(LoggingEvents.DeleteItem, $"Delete Item fileName = {fileName}, containername = {containername}");
                // at this point we know that exist but I will use this method 
                TelemetryClient.TrackEvent("DeleteFile", traceVariables);
                await blockBlob.DeleteIfExistsAsync();

                return NoContent();
            }
            catch (StorageException se) // storage exception
            {
                WebException webException = se.InnerException as WebException;
                TelemetryClient.TrackException(se, traceVariables);

                if (webException != null)
                {
                    HttpWebResponse httpWebResponse = webException.Response as HttpWebResponse;

                    if (httpWebResponse != null)
                    {
                        Logger.LogWarning(LoggingEvents.DeleteItemError, $"Invalid parameters fileName = {fileName}, containername = {containername}", httpWebResponse.StatusDescription);
                        (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.EntityNoDeleted}");
                        errorResponse.parameterName = "fileName";
                        errorResponse.parameterValue = fileName;
                        errorResponse.errorDescription += $" {httpWebResponse.StatusDescription}";
                        errors.Add(errorResponse);
                        return BadRequest(errors);

                    }
                    else
                    {
                        Logger.LogWarning(LoggingEvents.InternalError, $"Invalid parameters fileName = {fileName}, containername = {containername}", webException.Message);
                        (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ServerError}");
                        errorResponse.parameterName = "fileData";
                        errorResponse.parameterValue = null;
                        errorResponse.errorDescription += $" {webException.Message}";
                        return BadRequest(errors);
                    }

                }
                else
                {
                    Logger.LogWarning(LoggingEvents.InternalError, $"Invalid parameters fileName = {fileName}, containername = {containername}", se.Message);
                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ServerError}");
                    errorResponse.parameterName = "fileName";
                    errorResponse.parameterValue = fileName;
                    errorResponse.errorDescription += $" { se.Message}";
                    return BadRequest(errors);
                }
            }
            catch (Exception ex) // server error
            {
                TelemetryClient.TrackException(ex, traceVariables);
                Logger.LogWarning(LoggingEvents.DeleteItemError, $"Invalid parameters fileName = {fileName}, containername = {containername}", ex.Message);
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ServerError}");
                errorResponse.parameterName = "fileName";
                errorResponse.parameterValue = fileName;
                errorResponse.errorDescription += $" { ex.Message}";
                return BadRequest(errors);

            }
        }

        /// <summary>
        /// get a file if exists
        /// </summary>
        /// <param name="containername">container name</param>
        /// <param name="fileName">file name </param>
        /// <returns>file</returns>
        [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(FileResult))]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status400BadRequest)]
        [Route("/api/v1/{containername}/contentfiles/{fileName}", Name = GetByContainerAndIdRouteName)]
        [HttpGet]
        public async Task<IActionResult> GetFile([FromRoute] string containername, [FromRoute] string fileName)
        {

            List<ErrorResponse> errors = ContainerService.ValidateFileParams(containername, fileName);
            // trace variables
            traceVariables.Add("fileName", fileName);
            traceVariables.Add("containername", containername);
            
            TelemetryClient.TrackTrace("GetFilesURLVariables", traceVariables);
            ErrorResponse errorResponse = new ErrorResponse();
            if (errors.Count > 0)// invalid variables
            {
                Logger.LogWarning(LoggingEvents.InvalidItem, $"Invalid parameters fileName = {fileName}, containername = {containername}");
                return BadRequest(errors);
            }
            // The container name must be lower case
            containername = containername.ToLower();

            try
            {

                // Retrieve a reference to a container. 
                CloudBlobContainer container = GetContainer(containername);

                bool containerExist = await ContainerExist(container);

                if (!containerExist)// error container doesn't exist
                {
                    TelemetryClient.TrackEvent("ContainerNoExistGetFile", traceVariables);
                    Logger.LogWarning(LoggingEvents.ContainerNotFound, $"Container not found fileName = {fileName}, containername = {containername}");
                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.EntityNoFound}");
                    errorResponse.parameterName = "containername";
                    errorResponse.parameterValue = containername;
                    return StatusCode((int)HttpStatusCode.NotFound, errorResponse);
                }

                // Retrieve reference to a blob named the blob specified by the caller
                CloudBlockBlob blockBlob = GetCloudBlockBlob(container, fileName);

                bool fileExist = await BlockBlobExist(blockBlob);
                if (!fileExist) // file doesn't exist
                {
                    TelemetryClient.TrackEvent("FileNoExistGetFile", traceVariables);
                    Logger.LogWarning(LoggingEvents.GetItemNotFound, $"Get item not found fileName = {fileName}, containername = {containername}");
                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.EntityNoFound}");
                    errorResponse.parameterName = "fileName";
                    errorResponse.parameterValue = fileName;
                    return StatusCode((int)HttpStatusCode.NotFound, errorResponse);
                }
                Logger.LogInformation(LoggingEvents.GetItem, $"Get item fileName = {fileName}, containername = {containername}");
                // Retrieve the blob content
                MemoryStream memoryStream = new MemoryStream();
                await blockBlob.DownloadToStreamAsync(memoryStream);

                memoryStream.Seek(0, SeekOrigin.Begin);
                TelemetryClient.TrackEvent("GetFile", traceVariables);
                // Retrieve the blob content
                return new FileStreamResult(memoryStream, blockBlob.Properties.ContentType);

            }
            catch (StorageException se) // storage exception
            {
                WebException webException = se.InnerException as WebException;
                TelemetryClient.TrackException(se, traceVariables);

                if (webException != null)
                {
                    HttpWebResponse httpWebResponse = webException.Response as HttpWebResponse;

                    if (httpWebResponse != null)
                    {
                        Logger.LogWarning(LoggingEvents.GetItemError, $"Get item error fileName = {fileName}, containername = {containername}", httpWebResponse.StatusDescription);
                        (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.EntityNoFound}");
                        errorResponse.parameterName = "fileName";
                        errorResponse.parameterValue = fileName;
                        errorResponse.errorDescription += $" {httpWebResponse.StatusDescription}";
                        errors.Add(errorResponse);
                        return BadRequest(errors);

                    }
                    else
                    {
                        Logger.LogWarning(LoggingEvents.InternalError, $"Get item error fileName = {fileName}, containername = {containername}", webException.Message);
                        (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ServerError}");
                        errorResponse.parameterName = "fileData";
                        errorResponse.parameterValue = null;
                        errorResponse.errorDescription += $" {webException.Message}";
                        return BadRequest(errors);
                    }

                }
                else
                {
                    Logger.LogWarning(LoggingEvents.InternalError, $"Get item error fileName = {fileName}, containername = {containername}", se.Message);
                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ServerError}");
                    errorResponse.parameterName = "fileName";
                    errorResponse.parameterValue = fileName;
                    errorResponse.errorDescription += $" { se.Message}";
                    return BadRequest(errors);
                }
            }
            catch (Exception ex) // server error
            {
                TelemetryClient.TrackException(ex, traceVariables);
                Logger.LogWarning(LoggingEvents.GetItemError, $"Get item error fileName = {fileName}, containername = {containername}", ex.Message);
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.ServerError}");
                errorResponse.parameterName = "fileName";
                errorResponse.parameterValue = fileName;
                errorResponse.errorDescription += $" { ex.Message}";
                return BadRequest(errors);

            }
        }
        /// <summary>
        /// get all the files from a container 
        /// </summary>
        /// <param name="containername">container name</param>
        /// <returns>json list of files in a container</returns>
        [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(List<ContainerFile>))]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status400BadRequest)]
        [Route("/api/v1/{containername}/contentfiles")]
        [HttpGet]
        public async Task<IActionResult> GetFiles([FromRoute] string containername)
        {

            List<ContainerFile> blobNames = new List<ContainerFile>();
            List<ErrorResponse> errors = ContainerService.ValidateContainer(containername);
            // trace variables
            traceVariables.Add("containername", containername);
            TelemetryClient.TrackTrace("GetFilesURLVariables", traceVariables);
            ErrorResponse errorResponse = new ErrorResponse();
            if (errors.Count > 0)
            {
                Logger.LogWarning(LoggingEvents.InvalidItem, $"Invalid parameters containername = {containername}");
                return BadRequest(errors);
            }
            // The container name must be lower case
            containername = containername.ToLower();

            // Retrieve a reference to a container. 
            CloudBlobContainer container = GetContainer(containername);

            bool containerExist = await ContainerExist(container);

            if (!containerExist) // error container doesnt exist
            {
                TelemetryClient.TrackTrace("ContainerNoExistGetFiles", traceVariables);
                Logger.LogWarning(LoggingEvents.ContainerNotFound, $"Container not found containername = {containername}");
                (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage($"{(int)ErrorCode.EntityNoFound}");
                errorResponse.parameterName = "containername";
                errorResponse.parameterValue = containername;
                return StatusCode((int)HttpStatusCode.NotFound, errorResponse);
            }

            BlobRequestOptions options = new BlobRequestOptions();
            OperationContext context = new OperationContext();

            // Loop over items within the container and output the length and URI.
            BlobResultSegment result = await container.ListBlobsSegmentedAsync(null, true, BlobListingDetails.Metadata, null, null, options, context);
            if (result?.Results != null)
            {
                foreach (var blob in result.Results)
                {
                    if (blob is CloudBlockBlob)
                    {
                        blobNames.Add(new ContainerFile() { name = ((CloudBlockBlob)(blob)).Name });
                    }
                }
            }
            TelemetryClient.TrackEvent("GetFiles", traceVariables);
            Logger.LogInformation(LoggingEvents.GetItemList, $"get list containername = {containername}");
            // return array of list of files
            return new ObjectResult(blobNames.ToArray());

        }


    }
}
