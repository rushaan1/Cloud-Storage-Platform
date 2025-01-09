export class File {
  fileId: string;
  fileName: string;
  filePath: string;
  isFavorite: boolean;
  isTrash: boolean;

  // TODO ADD ADDITIONAL PROPERTIES HERE FOR FILE MODEL BUT AS NULLABLE PROPERTIES

  constructor(fileId: string, fileName: string, filePath: string, isFavorite: boolean = false, isTrash: boolean = false, isFolder: boolean = false) {
    this.isFavorite = isFavorite;
    this.isTrash = isTrash;
    this.filePath = filePath;
    this.fileName = fileName;
    this.fileId = fileId;

    // if (isFolder){
    //   this.folderId = fileId;
    //   this.folderName = fileName;
    //   this.folderPath = filePath;
    // }
    // else{
    //   this.folderId = "";
    //   this.folderName = "";
    //   this.folderPath = "";
    // }
  }
}
