using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using VanillaWebApi.Helpers;

namespace VanillaWebApi.Controllers
{
    [RoutePrefix("directory")]
    public class DirectoryController : ApiController
    {
        [Route("")]
        public IHttpActionResult Get()
        {
            try
            {
                var baseUrl = Request.RequestUri.Scheme + "://" + Request.RequestUri.Authority;
                var toReturn = FileHelper.GetFileItemsForDirectory(baseUrl);

                return Json(toReturn);

            }
            catch (Exception)
            {
                // TODO: Log exception
                return Json("Something went wrong!");
            }
        }

        [Route("{id}")]
        public IHttpActionResult Get(string id)
        {
            try
            {
                var path = id.Base64Decode();
                var attr = File.GetAttributes(path);

                if (attr.HasFlag(FileAttributes.Directory))
                {
                    var baseUrl = Request.RequestUri.Scheme + "://" + Request.RequestUri.Authority;
                    var toReturn = FileHelper.GetFileItemsForDirectory(baseUrl, path);

                    return Json(toReturn);
                }
                else
                {
                    return Json("Not a folder!");
                }
            }
            catch (Exception)
            {
                // TODO: Log exception
                return Json("Something went wrong!");
            }
        }


        [Route("{id}/move")]
        [HttpPost]
        public IHttpActionResult Move(string id, [FromBody]string destinationId)
        {
            try
            {
                if (!FileHelper.Move(id.Base64Decode(), destinationId.Base64Decode()))
                {
                    throw new Exception("Tharr be errors in these waters!");
                }

                return Json("Moved!");
            }
            catch (Exception)
            {
                // TODO: Log exception
                return Json("Something went wrong!");
            }
        }

        [Route("{id}/upload")]
        [HttpPost]
        public Task<HttpResponseMessage> Upload(string id)
        {
            var rootFolder = FileHelper.RootFolder;
            
            if (!id.Equals("home", StringComparison.OrdinalIgnoreCase))
            {
                var path = id.Base64Decode();
                var attr = File.GetAttributes(path);
                if (!attr.HasFlag(FileAttributes.Directory))
                {
                    throw new HttpResponseException(HttpStatusCode.ExpectationFailed);
                }

                rootFolder = path;
            }

            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = new MultipartFormDataStreamProvider(rootFolder);

            var task = Request.Content.ReadAsMultipartAsync(provider).
                ContinueWith<HttpResponseMessage>(o =>
                {
                    //Select the appropriate content item this assumes only 1 part
                    var fileContent = provider.Contents.SingleOrDefault();

                    try
                    {
                        if (fileContent != null)
                        {
                            var fileName = fileContent.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);
                            string tempFile = provider.FileData.First().LocalFileName;
                            var file = new FileInfo(tempFile);
                            File.Move(file.FullName, rootFolder + "\\" + fileName);
                        }

                        return new HttpResponseMessage()
                        {
                            Content = new StringContent("File uploaded.")
                        };
                    }
                    catch (IOException ex)
                    {
                        return new HttpResponseMessage()
                        {
                            Content = new StringContent(ex.Message)
                        };
                    }
                    catch (Exception)
                    {
                        return new HttpResponseMessage()
                        {
                            Content = new StringContent("Something went wrong.")
                        };
                    }
                }
            );
            return task;
        }
    }
}
