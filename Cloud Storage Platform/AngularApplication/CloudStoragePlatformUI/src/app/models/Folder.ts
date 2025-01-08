export class Folder implements BaseFile {
  fileId: string;
  fileName: string;
  filePath: string;
  isFavorite: boolean;
  isTrash: boolean;

  constructor(folderId: string, folderName: string, folderPath: string, isFavorite: boolean = false, isTrash: boolean = false) {
    this.fileId = folderId;
    this.fileName = folderName;
    this.isFavorite = isFavorite;
    this.isTrash = isTrash;
    this.filePath = folderPath;
  }
}
