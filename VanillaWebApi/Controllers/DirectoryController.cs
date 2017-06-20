using System;
using System.Configuration;
using System.IO;
using System.Web.Http;
using VanillaWebApi.Helpers;

namespace VanillaWebApi.Controllers
{
    public class DirectoryController : ApiController
    {
        [Route("directory")]
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

        [Route("directory/{id}")]
        public IHttpActionResult Get(string id)
        {
            try
            {
                var path = id.Base64Decode();

                var attr = File.GetAttributes(path);

                if (attr.HasFlag(FileAttributes.Directory))
                {
                    var toReturn = FileHelper.GetFileItemsForDirectory(RequestContext.VirtualPathRoot, path);

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


        [Route("directory/{id}/move")]
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
