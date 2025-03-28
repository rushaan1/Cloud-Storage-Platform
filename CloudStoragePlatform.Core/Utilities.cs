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
                    sorted = folders.OrderBy(f => f.Metadata?.Size).ToList();
                    break;
                case SortOrderOptions.SIZE_DESCENDING:
                    sorted = folders.OrderByDescending(f => f.Metadata?.Size).ToList();
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
                    sorted = files.OrderBy(f => f.Metadata?.Size).ToList();
                    break;
                case SortOrderOptions.SIZE_DESCENDING:
                    sorted = files.OrderByDescending(f => f.Metadata?.Size).ToList();
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

        public static async Task<string> ProcessFileOrFolderName(string name, IFoldersRepository? foldersRepo = null, IFilesRepository? filesRepo = null)
        {
            int index = 0;
            string baseName = name;
            string newName = baseName;
            bool exists = true;

            if (name.Length >= 80)
            {
                string preservedExtension = "";
                if (filesRepo != null)
                {
                    preservedExtension = name.Split(".")[name.Split(".").Length - 1];
                }
                baseName = name.Substring(0, 72);
                if (preservedExtension != "")
                {
                    baseName = baseName + $".{preservedExtension}";
                }
                newName = baseName;
                while (exists)
                {
                    if (foldersRepo != null)
                    {
                        exists = await foldersRepo.GetFolderByFolderPath(newName) != null;
                    }
                    else if (filesRepo != null)
                    {
                        exists = await filesRepo.GetFileByFilePath(newName) != null;
                    }
                    if (exists)
                    {
                        index++;
                        string suffix = $"({index})";
                        newName = baseName + suffix;
                    }
                }

                name = newName;
                baseName = newName;

            }
            exists = true;
            if (name.Contains("."))
            {
                if (filesRepo != null)
                {
                    if (filesRepo != null)
                    {
                        string preservedExtension = "";
                        string[] splitted = baseName.Split(".");
                        preservedExtension = splitted[splitted.Length - 1];
                        splitted[splitted.Length - 1] = "";
                        splitted[splitted.Length - 1] = preservedExtension;
                        foreach (string item in splitted)
                        {
                            baseName = item + "-";
                        }
                    }
                    else
                    {
                        baseName = baseName.Replace(".", "-");
                    }
                }
                newName = baseName;
                while (exists)
                {
                    if (foldersRepo != null)
                    {
                        exists = await foldersRepo.GetFolderByFolderPath(newName) != null;
                    }
                    else if (filesRepo != null)
                    {
                        exists = await filesRepo.GetFileByFilePath(newName) != null;
                    }
                    if (exists)
                    {
                        index++;
                        string suffix = $"({index})";
                        newName = baseName + suffix;
                    }
                }
                name = newName;
            }
            return name;
        }
    }
}
