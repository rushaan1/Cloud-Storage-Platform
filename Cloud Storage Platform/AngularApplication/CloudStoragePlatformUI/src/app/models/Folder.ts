export class Folder {
  folderId: string;
  folderName: string;
  folderPath: string;
  isFavorite: boolean;
  isTrash: boolean;

  constructor(folderId: string, folderName: string, folderPath: string, isFavorite: boolean = false, isTrash: boolean = false) {
    this.folderId = folderId;
    this.folderName = folderName;
    this.isFavorite = isFavorite;
    this.isTrash = isTrash;
    this.folderPath = folderPath;
  }
}
