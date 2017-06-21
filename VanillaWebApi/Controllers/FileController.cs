using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using VanillaWebApi.Helpers;

namespace VanillaWebApi.Controllers
{
    public class FileController : ApiController
    {
        [Route("file/{id}")]
        [HttpGet]
        public HttpResponseMessage Download(string id)
        {
            try
            {
                var path = id.Base64Decode();
                var attr = File.GetAttributes(path);

                if (attr.HasFlag(FileAttributes.Archive))
                {
                    var result = new HttpResponseMessage(HttpStatusCode.OK);
                    var stream = new FileStream(path, FileMode.Open);
                    result.Content = new StreamContent(stream);
                    result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    result.Content.Headers.ContentDisposition.FileName = Path.GetFileName(path);
                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    result.Content.Headers.ContentLength = stream.Length;

                    return result;
                }
                else
                {
                    var result = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    result.Content = new StringContent("Not a file!");

                    return result;
                }
            }
            catch (Exception ex)
            {
                // TODO: Log exception
                var result = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                result.Content = new StringContent("Something went wrong!");

                return result;
            }
        }

        [Route("file/{id}/delete")]
        [HttpGet]
        public IHttpActionResult Delete(string id)
        {
            try
            {
                var path = id.Base64Decode();

                FileHelper.Delete(path);

                return Json("Deleted!");
            }
            catch (Exception)
            {
                // TODO: Log exception
                return Json("Something went wrong!");
            }
        }

        [Route("file/{id}/upload")]
        [HttpPost]
        public Task<HttpResponseMessage> Upload()
        {
            var rootFolder = ConfigurationManager.AppSettings["RootFolder"] ?? "C:\\";

            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            string root = System.Web.HttpContext.Current.Server.MapPath(rootFolder);
            var provider = new MultipartFormDataStreamProvider(root);

            var task = Request.Content.ReadAsMultipartAsync(provider).
                ContinueWith<HttpResponseMessage>(o =>
                {

                    string file1 = provider.FileData.First().LocalFileName;
                    // this is the file name on the server where the file was saved

                    return new HttpResponseMessage()
                    {
                        Content = new StringContent("File uploaded.")
                    };
                }
            );
            return task;
        }

        [Route("file/{id}/copy/")]
        [HttpPost]
        public IHttpActionResult Copy(string id, [FromBody]string destinationId)
        {
            try
            {
                if (!FileHelper.Copy(id.Base64Decode(), destinationId.Base64Decode()))
                {
                    throw new Exception("Tharr be errors in these waters!");
                }

                return Json("Copied!");
            }
            catch (Exception)
            {
                // TODO: Log exception
                return Json("Something went wrong!");
            }
        }

        [Route("file/{id}/move")]
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
    }
}
