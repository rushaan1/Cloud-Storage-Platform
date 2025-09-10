using AutoFixture;
using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = CloudStoragePlatform.Core.Domain.Entities.File;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CloudStoragePlatform.Core
{
    public static class Utilities
    {
        public static async Task UpdateChildPaths(IFoldersRepository _foldersRepository, IFilesRepository _filesRepository, Folder source, string oldp, string newp)
        {
            Queue<Folder> tempTraversal = new Queue<Folder>();
            tempTraversal.Enqueue(source);
            while (tempTraversal.Count > 0)
            {
                Folder temp = tempTraversal.Dequeue();
                if (temp.FolderId != source.FolderId)
                {
                    string folderPathBeforeAfter = temp.FolderPath;
                    folderPathBeforeAfter = folderPathBeforeAfter.Replace(oldp, newp);
                    temp.FolderPath = folderPathBeforeAfter;
                    await _foldersRepository.UpdateFolder(temp, true, false, false, false, false, false);
                }
                if (temp.SubFolders.Count > 0)
                {
                    foreach (Folder temp2 in temp.SubFolders)
                    {
                        tempTraversal.Enqueue(temp2);
                    }
                }
                foreach (Domain.Entities.File temp2 in temp.Files)
                {
                    string filePathBeforeAfter = temp2.FilePath;
                    filePathBeforeAfter = filePathBeforeAfter.Replace(oldp, newp);
                    temp2.FilePath = filePathBeforeAfter;
                    await _filesRepository.UpdateFile(temp2, true, false, false, false);
                }
            }
        }

        public static string ReplaceLastOccurance(string main, string previousPart, string newPart)
        {
            int lastIndex = main.LastIndexOf(previousPart);
            string replacedString = main.Substring(0, lastIndex) + newPart;
            return replacedString;
        }

        public static List<Folder> Sort(List<Folder> folders, SortOrderOptions option)
        {
            List<Folder> sorted = new List<Folder>();
            switch (option)
            {
                case SortOrderOptions.ALPHABETICAL_ASCENDING:
                    sorted = folders.OrderBy(f => f.FolderName).ToList();
                    break;
                case SortOrderOptions.ALPHABETICAL_DESCENDING:
                    sorted = folders.OrderByDescending(f => f.FolderName).ToList();
                    break;
                case SortOrderOptions.DATEADDED_ASCENDING:
                    sorted = folders.OrderBy(f => f.CreationDate).ToList();
                    break;
                case SortOrderOptions.DATEADDED_DESCENDING:
                    sorted = folders.OrderByDescending(f => f.CreationDate).ToList();
                    break;
                case SortOrderOptions.LASTOPENED_ASCENDING:
                    sorted = folders.OrderBy(f => f.Metadata?.LastOpened).ToList();
                    break;
                case SortOrderOptions.LASTOPENED_DESCENDING:
                    sorted = folders.OrderByDescending(f => f.Metadata?.LastOpened).ToList();
                    break;
                case SortOrderOptions.SIZE_ASCENDING:
                    sorted = folders.OrderBy(f => f.Size).ToList();
                    break;
                case SortOrderOptions.SIZE_DESCENDING:
                    sorted = folders.OrderByDescending(f => f.Size).ToList();
                    break;
            }
            return sorted;
        }
        public static List<Domain.Entities.File> Sort(List<Domain.Entities.File> files, SortOrderOptions option)
        {
            List<Domain.Entities.File> sorted = new List<Domain.Entities.File>();
            switch (option)
            {
                case SortOrderOptions.ALPHABETICAL_ASCENDING:
                    sorted = files.OrderBy(f => f.FileName).ToList();
                    break;
                case SortOrderOptions.ALPHABETICAL_DESCENDING:
                    sorted = files.OrderByDescending(f => f.FileName).ToList();
                    break;
                case SortOrderOptions.DATEADDED_ASCENDING:
                    sorted = files.OrderBy(f => f.CreationDate).ToList();
                    break;
                case SortOrderOptions.DATEADDED_DESCENDING:
                    sorted = files.OrderByDescending(f => f.CreationDate).ToList();
                    break;
                case SortOrderOptions.LASTOPENED_ASCENDING:
                    sorted = files.OrderBy(f => f.Metadata?.LastOpened).ToList();
                    break;
                case SortOrderOptions.LASTOPENED_DESCENDING:
                    sorted = files.OrderByDescending(f => f.Metadata?.LastOpened).ToList();
                    break;
                case SortOrderOptions.SIZE_ASCENDING:
                    sorted = files.OrderBy(f => f.Size).ToList();
                    break;
                case SortOrderOptions.SIZE_DESCENDING:
                    sorted = files.OrderByDescending(f => f.Size).ToList();
                    break;
            }
            return sorted;
        }
        public static async Task UpdateMetadataRename(Folder folder, IFoldersRepository foldersRepository) 
        {
            folder.Metadata!.RenameCount++;
            //folder.Metadata!.PreviousPath = previousPath;
            folder.Metadata.PreviousRenameDate = DateTime.Now;
            await foldersRepository.UpdateFolder(folder, false, false, true, false, false, false);
        }

        public static async Task UpdateMetadataRename(File file, IFilesRepository filesRepository)
        {
            file.Metadata!.RenameCount++;
            //folder.Metadata!.PreviousPath = previousPath;
            file.Metadata.PreviousRenameDate = DateTime.Now;
            await filesRepository.UpdateFile(file, false, false, true, false);
        }


        public static async Task UpdateMetadataMove(Folder folder, string previousPath, IFoldersRepository foldersRepository)
        {
            folder.Metadata!.MoveCount++;
            folder.Metadata!.PreviousPath = previousPath;
            folder.Metadata.PreviousMoveDate = DateTime.Now;
            await foldersRepository.UpdateFolder(folder, false, false, true, false, false, false);
        }

        public static async Task UpdateMetadataMove(File file, string previousPath, IFilesRepository filesRepository)
        {
            file.Metadata!.MoveCount++;
            file.Metadata!.PreviousPath = previousPath;
            file.Metadata.PreviousMoveDate = DateTime.Now;
            await filesRepository.UpdateFile(file, false, false, true, false);
        }

        public static async Task UpdateMetadataOpen(Folder folder, IFoldersRepository foldersRepository) 
        {
            if (folder.MetadataId == null) 
            {
                return;
            }
            folder!.Metadata!.LastOpened = DateTime.Now;
            folder!.Metadata.OpenCount++;
            await foldersRepository.UpdateFolder(folder, false, false, true, false, false, false);
        }


        public static async Task UpdateMetadataOpen(File file, IFilesRepository filesRepository)
        {
            if (file.MetadataId == null)
            {
                return;
            }
            file!.Metadata!.LastOpened = DateTime.Now;
            file!.Metadata.OpenCount++;
            await filesRepository.UpdateFile(file, false, false, true, false);
        }

        public static void AttachMetadataForTesting(IFixture _fixture, Folder? folder, File? file) 
        {
            if (file != null)
            {
                Metadata m = _fixture.Build<Metadata>()
                                .With(m => m.File, file)
                                .With(m => m.Folder, (Folder)null)
                                .Create();
                file.Metadata = m;
            }
            else 
            {
                Metadata m = _fixture.Build<Metadata>()
                                .With(m => m.File, (File)null)
                                .With(m => m.Folder, folder)
                                .Create();
                folder.Metadata = m;
            }
        }

        public static string FindUniqueName(string[] existingOnes, string startingName, bool preserveExtension = false)
        {
            string folderNameToBeUsed = startingName;
            int newFolderIndex = 1;
            string extension = "";

            if (preserveExtension)
            {
                int extensionIndex = startingName.LastIndexOf('.');
                if (extensionIndex != -1)
                {
                    extension = startingName.Substring(extensionIndex);
                    startingName = startingName.Substring(0, extensionIndex);
                    folderNameToBeUsed = startingName;
                }
            }

            while (Array.Exists(existingOnes, name => name == folderNameToBeUsed + extension))
            {
                folderNameToBeUsed = $"{startingName} ({newFolderIndex})";
                newFolderIndex++;
            }

            return folderNameToBeUsed + extension;
        }

        public static bool IsHomeFolderPath(string folderPath, IConfiguration config)
        {
            string homeFolderPath = Path.Combine(config["InitialPathForStorage"], "home");
            return string.Equals(folderPath, homeFolderPath, StringComparison.OrdinalIgnoreCase);
        }

        public static string? GetMimeType(string filePath) 
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var mimeType = extension switch
            {
                ".txt" => "text/plain",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".mp4" => "video/mp4",
                ".webm" => "video/webm",
                ".mp3" => "audio/mpeg",
                ".wav" => "audio/wav",
                ".webp" => "image/webp",
                ".pdf" => "application/pdf",
                _ => null
            };
            return mimeType;
        }
    }
}
