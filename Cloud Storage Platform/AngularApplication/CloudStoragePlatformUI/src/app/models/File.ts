import {FileType} from "./FileType";

export class File {
  fileId: string;
  fileName: string;
  filePath: string;
  isFavorite: boolean;
  isTrash: boolean;
  uncreated:boolean=false;
  fileType:FileType;
  thumbnail:string | null = null;
  size:number | null = null;

  // TODO ADD ADDITIONAL PROPERTIES HERE FOR FILE MODEL BUT AS NULLABLE PROPERTIES
  // TODO Optionally rename fileId to id, fileName to name etc
  constructor(fileId: string, fileName: string, filePath: string, isFavorite: boolean = false, isTrash: boolean = false, fileType:FileType, uncreated:boolean=false, size:number, thumbnail:string | null = null) {
    this.isFavorite = isFavorite;
    this.isTrash = isTrash;
    this.filePath = filePath;
    this.fileName = fileName;
    this.fileId = fileId;
    this.fileType = fileType;
    this.uncreated = uncreated;
    this.thumbnail = thumbnail;
    this.size = size;
  }
}
