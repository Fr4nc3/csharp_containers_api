using Microsoft.AspNetCore.Http;
using Microsoft.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSDKClient;
using RestSDKClient.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FunctionalTestProject
{
    [TestClass]
    public class FunctionalTests
    {

        // DEMO: Local testing

        /// <summary>
        ///  API endpoint
        /// </summary>
        const string EndpointUrlString = "https://localhost:5001/";
       // const string EndpointUrlString = "https://friesco.azurewebsites.net/";

        /// <summary>
        /// Cliente Service credential
        /// </summary>
        public ServiceClientCredentials serviceClientCredentials;
        /// <summary>
        /// RestClientSDKLibraryClient
        /// </summary>
        private RestSDKClientClient client;


        public HttpClient httpClient;

        /// <summary>
        /// initilize the variables used on all the test cases
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            serviceClientCredentials = new TokenCredentials("FakeTokenValue");
            client = new RestSDKClientClient(new Uri(EndpointUrlString), serviceClientCredentials);
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(EndpointUrlString);
        }
        /// <summary>
        /// test invalid container parameters
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task InvalidContainerTooShort()
        {
            // Arrange

            string containername = "12";
            string fileName = "1";

            using FileStream fileStream = File.OpenRead($"images/Kitten.JPG");

            // Act
            var resultObject = await client.UploadFileWithHttpMessagesAsync(fileData: fileStream,
                contentType: System.Net.Mime.MediaTypeNames.Image.Jpeg,
                fileName: fileName, containername: containername);
            IList<ErrorResponse> resultPayload = resultObject.Body as IList<ErrorResponse>;

            // Assert
            Assert.IsTrue(resultPayload.Count(x => x.ParameterName == "containername") > 0);
            Assert.IsTrue(resultPayload.Count(x => x.ParameterValue == containername) > 0);
            Assert.IsTrue(resultPayload.Count(x => x.ErrorNumber == 5) > 0);
            Assert.AreNotEqual((int)StatusCodes.Status201Created, (int)resultObject.Response.StatusCode);
            Assert.AreEqual((int)StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
        }

        /// <summary>
        /// test invalid container parameters
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task InvalidContainerTooLong()
        {
            // Arrange

            string containername = "lalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalala";
            string fileName = "1";

            using FileStream fileStream = File.OpenRead($"images/Kitten.JPG");

            // Act
            var resultObject = await client.UploadFileWithHttpMessagesAsync(fileData: fileStream,
                contentType: System.Net.Mime.MediaTypeNames.Image.Jpeg,
                fileName: fileName, containername: containername);
            IList<ErrorResponse> resultPayload = resultObject.Body as IList<ErrorResponse>;

            // Assert
            Assert.IsTrue(resultPayload.Count(x => x.ParameterName == "containername") > 0);
            Assert.IsTrue(resultPayload.Count(x => x.ParameterValue == containername) > 0);
            Assert.IsTrue(resultPayload.Count(x => x.ErrorNumber == 2) > 0);
            Assert.AreNotEqual((int)StatusCodes.Status201Created, (int)resultObject.Response.StatusCode);
            Assert.AreEqual((int)StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
        }

        /// <summary>
        /// test invalid file parameters
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task InvalidFileNameTooLong()
        {
            // Arrange

            string containername = "lalalalalalalalalalalalalalalalalalalalalalalalala"; ;
            string fileName = "lalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalala";

            using FileStream fileStream = File.OpenRead($"images/Kitten.JPG");

            // Act
            var resultObject = await client.UploadFileWithHttpMessagesAsync(fileData: fileStream,
                contentType: System.Net.Mime.MediaTypeNames.Image.Jpeg,
                fileName: fileName, containername: containername);
            IList<ErrorResponse> resultPayload = resultObject.Body as IList<ErrorResponse>;

            // Assert
            Assert.IsTrue(resultPayload.Count(x => x.ParameterName == "fileName") > 0);
            Assert.IsTrue(resultPayload.Count(x => x.ParameterValue == fileName) > 0);
            Assert.IsTrue(resultPayload.Count(x => x.ErrorNumber == 2) > 0);
            Assert.AreNotEqual((int)StatusCodes.Status201Created, (int)resultObject.Response.StatusCode);
            Assert.AreEqual((int)StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
        }

        /// <summary>
        /// test invalid container parameters
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task InvalidContainerTooShortPatch()
        {
            // Arrange

            string containername = "12";
            string fileName = "1";

            using FileStream fileStream = File.OpenRead($"images/Kitten.JPG");

            // Act
            var resultObject = await client.PatchFileWithHttpMessagesAsync(fileData: fileStream,
                contentType: System.Net.Mime.MediaTypeNames.Image.Jpeg,
                fileName: fileName, containername: containername);
            IList<ErrorResponse> resultPayload = resultObject.Body as IList<ErrorResponse>;

            // Assert
            Assert.IsTrue(resultPayload.Count(x => x.ParameterName == "containername") > 0);
            Assert.IsTrue(resultPayload.Count(x => x.ParameterValue == containername) > 0);
            Assert.IsTrue(resultPayload.Count(x => x.ErrorNumber == 5) > 0);
            Assert.AreNotEqual((int)StatusCodes.Status204NoContent, (int)resultObject.Response.StatusCode);
            Assert.AreEqual((int)StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
        }

        /// <summary>
        /// test invalid container parameters
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task InvalidContainerTooLongPatch()
        {
            // Arrange

            string containername = "lalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalala";
            string fileName = "1";

            using FileStream fileStream = File.OpenRead($"images/Kitten.JPG");

            // Act
            var resultObject = await client.PatchFileWithHttpMessagesAsync(fileData: fileStream,
                contentType: System.Net.Mime.MediaTypeNames.Image.Jpeg,
                fileName: fileName, containername: containername);
            IList<ErrorResponse> resultPayload = resultObject.Body as IList<ErrorResponse>;

            // Assert
            Assert.IsTrue(resultPayload.Count(x => x.ParameterName == "containername") > 0);
            Assert.IsTrue(resultPayload.Count(x => x.ParameterValue == containername) > 0);
            Assert.IsTrue(resultPayload.Count(x => x.ErrorNumber == 2) > 0);
            Assert.AreNotEqual((int)StatusCodes.Status204NoContent, (int)resultObject.Response.StatusCode);
            Assert.AreEqual((int)StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
        }

        /// <summary>
        /// test invalid file parameters
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task InvalidFileNameTooLongPatch()
        {
            // Arrange

            string containername = "lalalalalalalalalalalalalalalalalalalalalalalalala"; ;
            string fileName = "lalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalala";

            using FileStream fileStream = File.OpenRead($"images/Kitten.JPG");

            // Act
            var resultObject = await client.PatchFileWithHttpMessagesAsync(fileData: fileStream,
                contentType: System.Net.Mime.MediaTypeNames.Image.Jpeg,
                fileName: fileName, containername: containername);
            IList<ErrorResponse> resultPayload = resultObject.Body as IList<ErrorResponse>;

            // Assert
            Assert.IsTrue(resultPayload.Count(x => x.ParameterName == "fileName") > 0);
            Assert.IsTrue(resultPayload.Count(x => x.ParameterValue == fileName) > 0);
            Assert.IsTrue(resultPayload.Count(x => x.ErrorNumber == 2) > 0);
            Assert.AreNotEqual((int)StatusCodes.Status204NoContent, (int)resultObject.Response.StatusCode);
            Assert.AreEqual((int)StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
        }

        /// <summary>
        /// test invalid container parameters
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task InvalidContainerTooShortDelete()
        {
            // Arrange

            string containername = "12";
            string fileName = "1";


            // Act
            var resultObject = await client.DeleteFileWithHttpMessagesAsync(fileName: fileName, containername: containername);
            IList<ErrorResponse> resultPayload = resultObject.Body as IList<ErrorResponse>;

            // Assert
            Assert.IsTrue(resultPayload.Count(x => x.ParameterName == "containername") > 0);
            Assert.IsTrue(resultPayload.Count(x => x.ParameterValue == containername) > 0);
            Assert.IsTrue(resultPayload.Count(x => x.ErrorNumber == 5) > 0);
            Assert.AreNotEqual((int)StatusCodes.Status204NoContent, (int)resultObject.Response.StatusCode);
            Assert.AreEqual((int)StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
        }

        /// <summary>
        /// test invalid container parameters
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task InvalidContainerTooLongDelete()
        {
            // Arrange

            string containername = "lalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalala";
            string fileName = "1";

            // Act
            var resultObject = await client.DeleteFileWithHttpMessagesAsync(fileName: fileName, containername: containername);
            IList<ErrorResponse> resultPayload = resultObject.Body as IList<ErrorResponse>;

            // Assert
            Assert.IsTrue(resultPayload.Count(x => x.ParameterName == "containername") > 0);
            Assert.IsTrue(resultPayload.Count(x => x.ParameterValue == containername) > 0);
            Assert.IsTrue(resultPayload.Count(x => x.ErrorNumber == 2) > 0);
            Assert.AreNotEqual((int)StatusCodes.Status204NoContent, (int)resultObject.Response.StatusCode);
            Assert.AreEqual((int)StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
        }

        /// <summary>
        /// test invalid file parameters
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task InvalidFileNameTooLongDelete()
        {
            // Arrange

            string containername = "lalalalalalalalalalalalalalalalalalalalalalalalala"; ;
            string fileName = "lalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalalala";

            // Act
            var resultObject = await client.DeleteFileWithHttpMessagesAsync(fileName: fileName, containername: containername);
            IList<ErrorResponse> resultPayload = resultObject.Body as IList<ErrorResponse>;

            // Assert
            Assert.IsTrue(resultPayload.Count(x => x.ParameterName == "fileName") > 0);
            Assert.IsTrue(resultPayload.Count(x => x.ParameterValue == fileName) > 0);
            Assert.IsTrue(resultPayload.Count(x => x.ErrorNumber == 2) > 0);
            Assert.AreNotEqual((int)StatusCodes.Status204NoContent, (int)resultObject.Response.StatusCode);
            Assert.AreEqual((int)StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);

        }


        /// <summary>
        /// patch container not found
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task ContainerNoFoundPatch()
        {
            // Arrange

            Random randomName = new Random(); //only required once
            var number = randomName.Next(1, 101);
            string containername = $"wrongConainer{number}";
            string fileName = "Kitten.JPG";

            using FileStream fileStream = File.OpenRead($"images/Kitten.JPG");

            // Act
            var resultObject = await client.PatchFileWithHttpMessagesAsync(fileData: fileStream,
                fileName: fileName, containername: containername, contentType: System.Net.Mime.MediaTypeNames.Image.Jpeg);
            ErrorResponse resultPayload = resultObject.Body as ErrorResponse;

            // Assert
            Assert.IsTrue(resultPayload.ErrorNumber == 4);
            Assert.IsTrue(resultPayload.ParameterName == "containername");
            Assert.IsTrue(resultPayload.ParameterValue == containername.ToLower());
            Assert.AreNotEqual((int)StatusCodes.Status204NoContent, (int)resultObject.Response.StatusCode);
            Assert.AreEqual((int)StatusCodes.Status404NotFound, (int)resultObject.Response.StatusCode);
        }

        /// <summary>
        /// Patch file no found
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task FileNoFoundPatch()
        {
            // Arrange

            string containername = "publicsamples"; // known existence
            string fileName = "One.JPG";

            using FileStream fileStream = File.OpenRead($"images/Kitten.JPG");

            // Act
            var resultObject = await client.PatchFileWithHttpMessagesAsync(fileData: fileStream,
                fileName: fileName, containername: containername, contentType: System.Net.Mime.MediaTypeNames.Image.Jpeg);
            ErrorResponse resultPayload = resultObject.Body as ErrorResponse;

            // Assert
            Assert.IsTrue(resultPayload.ErrorNumber == 4);
            Assert.IsTrue(resultPayload.ParameterName == "fileName");
            Assert.IsTrue(resultPayload.ParameterValue == fileName);
            Assert.AreNotEqual((int)StatusCodes.Status204NoContent, (int)resultObject.Response.StatusCode);
            Assert.AreEqual((int)StatusCodes.Status404NotFound, (int)resultObject.Response.StatusCode);
        }
        /// <summary>
        /// Delete  container No found 
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task ContainerNoFoundDelete()
        {
            // Arrange
            Random randomName = new Random(); //only required once
            var number = randomName.Next(1, 101);
            string containername = $"wrongConainer{number}";
            string fileName = "Kitten.JPG";

            // Act
            var resultObject = await client.DeleteFileWithHttpMessagesAsync(fileName: fileName, containername: containername);
            ErrorResponse resultPayload = resultObject.Body as ErrorResponse;

            // Assert
            Assert.IsTrue(resultPayload.ErrorNumber == 4);
            Assert.IsTrue(resultPayload.ParameterName == "containername");
            Assert.IsTrue(resultPayload.ParameterValue == containername.ToLower());
            Assert.AreNotEqual((int)StatusCodes.Status204NoContent, (int)resultObject.Response.StatusCode);
            Assert.AreEqual((int)StatusCodes.Status404NotFound, (int)resultObject.Response.StatusCode);
        }
        /// <summary>
        /// Delete file no found
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task FileNoFoundDelete()
        {
            // Arrange
            Random randomName = new Random(); //only required once
            var number = randomName.Next(1, 101);
            string containername = "publicsamples"; // known existence
            string fileName = $"One{number}.JPG";


            // Act
            var resultObject = await client.DeleteFileWithHttpMessagesAsync(fileName: fileName, containername: containername);
            ErrorResponse resultPayload = resultObject.Body as ErrorResponse;

            // Assert
            Assert.IsTrue(resultPayload.ErrorNumber == 4);
            Assert.IsTrue(resultPayload.ParameterName == "fileName");
            Assert.IsTrue(resultPayload.ParameterValue == fileName);
            Assert.AreNotEqual((int)StatusCodes.Status204NoContent, (int)resultObject.Response.StatusCode);
            Assert.AreEqual((int)StatusCodes.Status404NotFound, (int)resultObject.Response.StatusCode);
        }
        /// <summary>
        /// Get Container No found 
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task ContainerNoFoundGet()
        {
            // Arrange
            Random randomName = new Random(); //only required once
            var number = randomName.Next(1, 101);
            string containername = $"wrongConainer{number}";
            string fileName = "Kitten.JPG";

            // Act
            var resultObject = await client.GetFileWithHttpMessagesAsync(fileName: fileName, containername: containername);
            ErrorResponse resultPayload = resultObject.Body as ErrorResponse;

            // Assert
            Assert.IsTrue(resultPayload.ErrorNumber == 4);
            Assert.IsTrue(resultPayload.ParameterName == "containername");
            Assert.IsTrue(resultPayload.ParameterValue == containername.ToLower());
            Assert.AreNotEqual((int)StatusCodes.Status200OK, (int)resultObject.Response.StatusCode);
            Assert.AreEqual((int)StatusCodes.Status404NotFound, (int)resultObject.Response.StatusCode);
        }
        /// <summary>
        ///Get file no found
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task FileNoFoundGet()
        {
            // Arrange
            Random randomName = new Random(); //only required once
            var number = randomName.Next(1, 101);
            string containername = "publicsamples"; // known existence
            string fileName = $"One{number}.JPG";


            // Act
            var resultObject = await client.GetFileWithHttpMessagesAsync(fileName: fileName, containername: containername);
            ErrorResponse resultPayload = resultObject.Body as ErrorResponse;

            // Assert
            Assert.IsTrue(resultPayload.ErrorNumber == 4);
            Assert.IsTrue(resultPayload.ParameterName == "fileName");
            Assert.IsTrue(resultPayload.ParameterValue == fileName);
            Assert.AreNotEqual(StatusCodes.Status200OK, resultObject.Response.StatusCode);
            Assert.AreEqual((int)StatusCodes.Status404NotFound, (int)resultObject.Response.StatusCode);
        }


        /// <summary>
        ///Get files contianer no found
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task ContainerNoFoundGetFiles()
        {
            // Arrange
            Random randomName = new Random(); //only required once
            var number = randomName.Next(1, 101);
            string containername = $"publicsample{number}"; // known existence


            // Act
            var resultObject = await client.GetFilesWithHttpMessagesAsync(containername: containername);
            ErrorResponse resultPayload = resultObject.Body as ErrorResponse;

            // Assert
            Assert.IsTrue(resultPayload.ErrorNumber == 4);
            Assert.IsTrue(resultPayload.ParameterName == "containername");
            Assert.IsTrue(resultPayload.ParameterValue == containername);
            Assert.AreNotEqual(StatusCodes.Status200OK, resultObject.Response.StatusCode);
            Assert.AreEqual((int)StatusCodes.Status404NotFound, (int)resultObject.Response.StatusCode);
        }

        /// <summary>
        /// test valid patch null filedata
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Negative")]
        [TestMethod]
        public async Task PatchUpdateNullFile()
        {
            // Arrange
            Random randomName = new Random(); //only required once
            var number = randomName.Next(1, 101);
            string containername = "privateupload";
            string fileName = $"Kitten{number}.JPG";

            using FileStream fileStream = File.OpenRead($"images/Kitten.JPG");

            // Act
            var resultObject = await client.UploadFileWithHttpMessagesAsync(fileData: fileStream,
                contentType: System.Net.Mime.MediaTypeNames.Image.Jpeg,
                fileName: fileName, containername: containername);


            Assert.AreEqual((int)StatusCodes.Status201Created, (int)resultObject.Response.StatusCode);
            Assert.AreNotEqual((int)StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
            Assert.AreNotEqual((int)StatusCodes.Status404NotFound, (int)resultObject.Response.StatusCode);

            // stream  file is null 
            // Act
            var resultObject2 = await client.PatchFileWithHttpMessagesAsync(fileData: fileStream,
                contentType: System.Net.Mime.MediaTypeNames.Image.Jpeg,
                fileName: fileName, containername: containername);
            Assert.AreEqual((int)StatusCodes.Status400BadRequest, (int)resultObject2.Response.StatusCode);

            // delete the image at the end of the test
            var resultObject3 = await client.DeleteFileWithHttpMessagesAsync(fileName: fileName, containername: containername);
            Assert.AreEqual((int)StatusCodes.Status204NoContent, (int)resultObject3.Response.StatusCode);
        }
        /// <summary>
        /// test valid Uploload public
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Positive")]
        [TestMethod]
        public async Task UploadPublic()
        {
            // Arrange
            Random randomName = new Random(); //only required once
            var number = randomName.Next(1, 101);
            string containername = "publicupload";
            string fileName = $"Kitten{number}.JPG";

            using FileStream fileStream = File.OpenRead($"images/Kitten.JPG");

            // Act
            var resultObject = await client.UploadFileWithHttpMessagesAsync(fileData: fileStream,
                contentType: System.Net.Mime.MediaTypeNames.Image.Jpeg,
                fileName: fileName, containername: containername);

            string location = resultObject.Response.Headers.Location.ToString();
            //https://fr.blob.core.windows.net/publicsamples/Dogs.JPG example of public path
            // Assert
            Assert.IsTrue(location.Contains("https://fr.blob.core.windows.net/"));
            Assert.AreEqual((int)StatusCodes.Status201Created, (int)resultObject.Response.StatusCode);
            Assert.AreNotEqual((int)StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
            Assert.AreNotEqual((int)StatusCodes.Status404NotFound, (int)resultObject.Response.StatusCode);

            // delete the image at the end of the test
            var resultObject2 = await client.DeleteFileWithHttpMessagesAsync(fileName: fileName, containername: containername);
            Assert.AreEqual((int)StatusCodes.Status204NoContent, (int)resultObject2.Response.StatusCode);
        }

        /// <summary>
        /// test valid Uploload public
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Positive")]
        [TestMethod]
        public async Task UploadPrivate()
        {
            // Arrange
            Random randomName = new Random(); //only required once
            var number = randomName.Next(1, 101);
            string containername = "privateupload";
            string fileName = $"Kitten{number}.JPG";

            using FileStream fileStream = File.OpenRead($"images/Kitten.JPG");

            // Act
            var resultObject = await client.UploadFileWithHttpMessagesAsync(fileData: fileStream,
                contentType: System.Net.Mime.MediaTypeNames.Image.Jpeg,
                fileName: fileName, containername: containername);

            string location = resultObject.Response.Headers.Location.ToString();

            // Assert
            Assert.IsTrue(location.Contains("/api/v1/"));
            Assert.AreEqual((int)StatusCodes.Status201Created, (int)resultObject.Response.StatusCode);
            Assert.AreNotEqual((int)StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
            Assert.AreNotEqual((int)StatusCodes.Status404NotFound, (int)resultObject.Response.StatusCode);

            // delete the image at the end of the test
            var resultObject2 = await client.DeleteFileWithHttpMessagesAsync(fileName: fileName, containername: containername);
            Assert.AreEqual((int)StatusCodes.Status204NoContent, (int)resultObject2.Response.StatusCode);
        }

        /// <summary>
        /// test valid Patch upload
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Positive")]
        [TestMethod]
        public async Task PatchUpdate()
        {
            // Arrange
            Random randomName = new Random(); //only required once
            var number = randomName.Next(1, 101);
            string containername = "privateupload";
            string fileName = $"Kitten{number}.JPG";

            using FileStream fileStream = File.OpenRead($"images/Kitten.JPG");

            // Act
            var resultObject = await client.UploadFileWithHttpMessagesAsync(fileData: fileStream,
                contentType: System.Net.Mime.MediaTypeNames.Image.Jpeg,
                fileName: fileName, containername: containername);


            Assert.AreEqual((int)StatusCodes.Status201Created, (int)resultObject.Response.StatusCode);
            Assert.AreNotEqual((int)StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
            Assert.AreNotEqual((int)StatusCodes.Status404NotFound, (int)resultObject.Response.StatusCode);

            using FileStream fileStream2 = File.OpenRead($"images/Kitten.JPG");
            // Act
            var resultObject2 = await client.PatchFileWithHttpMessagesAsync(fileData: fileStream2,
                contentType: System.Net.Mime.MediaTypeNames.Image.Jpeg,
                fileName: fileName, containername: containername);
            Assert.AreEqual((int)StatusCodes.Status204NoContent, (int)resultObject2.Response.StatusCode);

            // delete the image at the end of the test
            var resultObject3 = await client.DeleteFileWithHttpMessagesAsync(fileName: fileName, containername: containername);
            Assert.AreEqual((int)StatusCodes.Status204NoContent, (int)resultObject3.Response.StatusCode);
        }

        /// <summary>
        /// test valid delete  file
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Positive")]
        [TestMethod]
        public async Task DeleteFIle()
        {
            // Arrange
            Random randomName = new Random(); //only required once
            var number = randomName.Next(1, 101);
            string containername = "privateupload";
            string fileName = $"Kitten{number}.JPG";

            using FileStream fileStream = File.OpenRead($"images/Kitten.JPG");

            // act /arrange
            var resultObject = await client.UploadFileWithHttpMessagesAsync(fileData: fileStream,
                contentType: System.Net.Mime.MediaTypeNames.Image.Jpeg,
                fileName: fileName, containername: containername);


            Assert.AreEqual((int)StatusCodes.Status201Created, (int)resultObject.Response.StatusCode);
            Assert.AreNotEqual((int)StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
            Assert.AreNotEqual((int)StatusCodes.Status404NotFound, (int)resultObject.Response.StatusCode);

            // Act
            var resultObject2 = await client.DeleteFileWithHttpMessagesAsync(fileName: fileName, containername: containername);
            Assert.AreEqual((int)StatusCodes.Status204NoContent, (int)resultObject2.Response.StatusCode);
        }

        /// <summary>
        /// test valid get
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Positive")]
        [TestMethod]
        public async Task GetFile()
        {
            // Arrange

            string containername = "publicsamples";
            string fileName = "HelloFile.txt";
            // https://fr.blob.core.windows.net/publicsamples/HelloFile.txt

            // act 
            var resultObject = await client.GetFileWithHttpMessagesAsync(fileName: fileName, containername: containername);

            // Assert


            Assert.AreEqual((int)StatusCodes.Status200OK, (int)resultObject.Response.StatusCode);
            Assert.AreNotEqual((int)StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
            Assert.AreNotEqual((int)StatusCodes.Status404NotFound, (int)resultObject.Response.StatusCode);
        }
        /// <summary>
        /// test valid get
        /// </summary>
        /// <returns>success</returns>
        [TestCategory("Positive")]
        [TestMethod]
        public async Task GetFiles()
        {
            // Arrange

            string containername = "publicsamples";


            // act 
            var resultObject = await client.GetFilesWithHttpMessagesAsync(containername: containername);
            IList<ContainerFile> resultPayload = resultObject.Body as IList<ContainerFile>;
            // Assert
            Assert.IsTrue(resultPayload.Count() > 0);

            // Assert
            Assert.IsTrue(resultPayload.Count(x => x.Name == "HelloFile.txt") == 1);
            Assert.IsTrue(resultPayload.Count(x => x.Name == "Dogs.JPG") == 1);
            Assert.IsTrue(resultPayload.Count(x => x.Name == "usconstitution.pdf") == 1);
            Assert.AreEqual((int)StatusCodes.Status200OK, (int)resultObject.Response.StatusCode);
            Assert.AreNotEqual((int)StatusCodes.Status400BadRequest, (int)resultObject.Response.StatusCode);
            Assert.AreNotEqual((int)StatusCodes.Status404NotFound, (int)resultObject.Response.StatusCode);


        }


    }
}
