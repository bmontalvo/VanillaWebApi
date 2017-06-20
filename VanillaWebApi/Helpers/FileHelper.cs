using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using VanillaWebApi.Models;

namespace VanillaWebApi.Helpers
{
    public static class FileHelper
    {
        public static string rootFolder = ConfigurationManager.AppSettings["RootFolder"] ?? "C:\\";

        public static IEnumerable<FileItem> GetFileItemsForDirectory(string baseUrl, string directory = null)
        {
            directory = directory ?? rootFolder;

            var dirInfo = new DirectoryInfo(directory);

            var sInfos = dirInfo.GetFileSystemInfos();

            var toReturn = new List<FileItem>();

            // Add the parent directory if we are not on the root folder
            if (!directory.Equals(rootFolder, StringComparison.InvariantCultureIgnoreCase))
            {
                var identifier = dirInfo.Parent.FullName.Base64Encode();
                var parent = new FileItem
                {
                    id = identifier,
                    displayName = "..",
                    date = dirInfo.Parent.LastWriteTimeUtc,
                    isFolder = true,
                    self = baseUrl + "/directory/" +  identifier
                };

                toReturn.Add(parent);
            }

            foreach (var sInfo in sInfos)
            {
                var type = sInfo.GetType();

                if (type == typeof(FileInfo) || type == typeof(DirectoryInfo))
                {
                    var identifier = sInfo.FullName.Base64Encode();
                    var fileItem = new FileItem
                    {
                        id = identifier,
                        displayName = sInfo.Name,
                        date = sInfo.LastWriteTimeUtc
                    };

                    if (type == typeof(FileInfo))
                    {
                        fileItem.isReadOnly = ((FileInfo)sInfo).IsReadOnly;
                        fileItem.length = ((FileInfo)sInfo).Length;
                        fileItem.self = baseUrl + "/file/" + identifier;
                        fileItem.actions.Add(new ActionItem
                        {
                            name = "copy-file",
                            title = "Copy File",
                            method = "POST",
                            href = baseUrl + "/file/" + identifier + "/copy/",
                            type = "application/json",
                            fields = new List<Field>
                            {
                                new Field { name = "destinationId", type = "string" }
                            }
                        });

                        if (!fileItem.isReadOnly)
                        {
                            fileItem.actions.Add(new ActionItem
                            {
                                name = "download-file",
                                title = "Download File",
                                method = "GET",
                                href = baseUrl + "/file/" + identifier + "/download",
                                type = "application/json",
                            });
                            fileItem.actions.Add(new ActionItem
                            {
                                name = "move-file",
                                title = "Move File",
                                method = "POST",
                                href = baseUrl + "/file/" + identifier + "/move",
                                type = "application/json",
                                fields = new List<Field>
                                {
                                    new Field { name = "destinationId", type = "string" }
                                }
                            });
                            fileItem.actions.Add(new ActionItem
                            {
                                name = "delete-file",
                                title = "Delete File",
                                method = "GET",
                                href = baseUrl + "/file/" + identifier + "/delete",
                                type = "application/json",
                            });

                        }
                    }
                    else
                    {
                        fileItem.isFolder = true;
                        fileItem.self = baseUrl + "/directory/" + identifier;
                        fileItem.actions.Add(new ActionItem
                        {
                            name = "move-directory",
                            title = "Move Directory",
                            method = "POST",
                            href = baseUrl + "/directory/" + identifier + "/move",
                            type = "application/json",
                            fields = new List<Field>
                            {
                                new Field { name = "destinationId", type = "string" }
                            }
                        });
                    }

                    toReturn.Add(fileItem);
                }
            }

            return toReturn;
        }

        public static bool Copy(string sourcePath, string destPath)
        {
            var successful = false;
            var sourceAttr = File.GetAttributes(sourcePath);
            var destAttr = File.GetAttributes(destPath);

            // only allow copying files to directories (for now)
            if (sourceAttr.HasFlag(FileAttributes.Archive) && destAttr.HasFlag(FileAttributes.Directory))
            {
                var fInfo = new FileInfo(sourcePath);
                fInfo.CopyTo(destPath);

                successful = true;
            }

            return successful;
        }

        public static bool Move(string sourcePath, string destPath)
        {
            var successful = false;
            var sourceAttr = File.GetAttributes(sourcePath);
            var destAttr = File.GetAttributes(destPath);

            if (destAttr.HasFlag(FileAttributes.Directory))
            {
                if (sourceAttr.HasFlag(FileAttributes.Archive))
                {
                    if (!sourceAttr.HasFlag(FileAttributes.ReadOnly))
                    {
                        var fInfo = new FileInfo(sourcePath);
                        fInfo.MoveTo(destPath);

                        successful = true;
                    }
                    else
                    {
                        throw new Exception(string.Format("Cannot move file {0} because it is ReadOnly!", sourcePath));
                    }
                }
                else if (sourceAttr.HasFlag(FileAttributes.Archive))
                {
                    var dInfo = new DirectoryInfo(sourcePath);
                    dInfo.MoveTo(destPath);

                    successful = true;
                }
            }

            return successful;
        }

        public static bool Delete(string sourcePath)
        {
            var successful = false;
            var sourceAttr = File.GetAttributes(sourcePath);

            if (sourceAttr.HasFlag(FileAttributes.Archive))
            {
                if (!sourceAttr.HasFlag(FileAttributes.ReadOnly))
                {
                    var fInfo = new FileInfo(sourcePath);
                    fInfo.Delete();

                    successful = true;
                }
                else
                {
                    throw new Exception(string.Format("Cannot delete file {0} because it is ReadOnly!", sourcePath));
                }
            }

            return successful;
        }
    }
}