using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using VanillaWebApi.Models;
using System.Linq;

namespace VanillaWebApi.Helpers
{
    public static class FileHelper
    {
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static string RootFolder
        {
            get
            {
                return ConfigurationManager.AppSettings["RootFolder"] ?? AssemblyDirectory + "\\Testie";
            }
        }

        public static DirectoryItem GetFileItemsForDirectory(string baseUrl, string directory = null)
        {
            directory = directory ?? RootFolder;

            var rootDirInfo = new DirectoryInfo(directory);

            var toReturn = new DirectoryItem();

            toReturn.uploadAction = new ActionItem
            {
                name = "upload-file",
                title = "Upload File",
                method = "POST",
                href = baseUrl + "/file/upload",
                type = "multipart/form-data",
            };

            // Add the parent directory if we are not on the root folder
            if (!directory.Equals(RootFolder, StringComparison.InvariantCultureIgnoreCase))
            {
                var identifier = rootDirInfo.Parent.FullName.Base64Encode();
                var parent = new FileItem
                {
                    id = identifier,
                    displayName = "..",
                    date = rootDirInfo.Parent.LastWriteTimeUtc,
                    isFolder = true,
                    self = baseUrl + "/directory/" +  identifier
                };

                toReturn.fileItems.Add(parent);
            }

            // Add directories
            foreach(var dir in rootDirInfo.GetDirectories())
            {
                var identifier = dir.FullName.Base64Encode();
                var dirItem = new FileItem
                {
                    id = identifier,
                    displayName = dir.Name,
                    date = dir.LastWriteTimeUtc,
                    isFolder = true,
                    self = baseUrl + "/directory/" + identifier
                };
                dirItem.actions.Add(new ActionItem
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

                toReturn.fileItems.Add(dirItem);
            }

            // Add files
            foreach (var file in rootDirInfo.GetFiles())
            {
                var identifier = file.FullName.Base64Encode();
                var fileItem = new FileItem
                {
                    id = identifier,
                    displayName = file.Name,
                    date = file.LastWriteTimeUtc,
                    self = baseUrl + "/file/" + identifier,
                    isReadOnly = file.IsReadOnly,
                    length = file.Length,
                };

                // Move & Delete actions only if file isn't ReadOnly
                if (!fileItem.isReadOnly)
                {
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

                fileItem.actions.Add(new ActionItem
                {
                    name = "download-file",
                    title = "Download File",
                    method = "GET",
                    href = baseUrl + "/file/" + identifier,
                    type = "application/json",
                });
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

                toReturn.fileItems.Add(fileItem);
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
                        fInfo.MoveTo(destPath + "\\" + fInfo.Name);

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
                    dInfo.MoveTo(destPath + "\\" + dInfo.Name);

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