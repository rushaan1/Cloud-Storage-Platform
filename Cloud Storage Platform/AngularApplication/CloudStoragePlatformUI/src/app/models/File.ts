export class File {
  fileId: string;
  fileName: string;
  filePath: string;
  isFavorite: boolean;
  isTrash: boolean;
  uncreated:boolean=false;

  // TODO ADD ADDITIONAL PROPERTIES HERE FOR FILE MODEL BUT AS NULLABLE PROPERTIES
  // TODO Optionally rename fileId to id, fileName to name etc
  constructor(fileId: string, fileName: string, filePath: string, isFavorite: boolean = false, isTrash: boolean = false, isFolder: boolean = false, uncreated:boolean=false) {
    this.isFavorite = isFavorite;
    this.isTrash = isTrash;
    this.filePath = filePath;
    this.fileName = fileName;
    this.fileId = fileId;
    this.uncreated = uncreated;

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
