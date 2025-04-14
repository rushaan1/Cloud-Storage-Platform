import {ArgumentInvalidException} from "./ArgumentInvalidException";
import {FileType} from "./models/FileType";
import {File} from "./models/File";

export class Utils {
  private constructor() {}
  public static validString(str:string|undefined|null):boolean{
    if (str && str.trim().length > 0){
      return true;
    }
    return false;
  }

  public static handleStringInvalidError(str:string, errorMessage:string="Invalid string supplied"):void {
    if (!this.validString(str)){
      throw new ArgumentInvalidException(errorMessage);
    }
  }

  // MAIN Purpose is to combine an array of strings into a string containing \ before every element string
  public static constructFilePathForApi(url:string[]):string{
    let constructedPathForApi = "";
    for (let i = url.indexOf("home"); i< url.length; i++) {
      constructedPathForApi = constructedPathForApi + "\\" + url[i];
    }
    return constructedPathForApi;
  }

  public static obtainBreadCrumbs(crumbs:string[]):string[] {
    let breadCrumbs:string[] = [];
    for (let i = crumbs.indexOf("home"); i < crumbs.length; i++) {
      breadCrumbs.push(crumbs[i]);
    }
    return breadCrumbs;
  }

  public static cleanPath(path:string):string[]{
    let constructedPath = path.split("\\");
    let index = constructedPath.indexOf("home");
    constructedPath = constructedPath.slice(index, constructedPath.length);

    return constructedPath;
  }

  public static resize(str:string, n:number):string {
    if (str.length >= n) {
      return str.substring(0, n) + "...";
    }
    else{
      return str;
    }
  }

  public static processFileModel(data: any): File {
    if (data.fileType !== undefined && data.fileType !== null) {
      return new File(
        data.fileId,
        data.fileName,
        data.filePath,
        data.isFavorite,
        data.isTrash,
        data.fileType as FileType,
        false,
        data.thumbnail
      );
    } else {
      return new File(
        data.folderId,
        data.folderName,
        data.folderPath,
        data.isFavorite,
        data.isTrash,
        FileType.Folder,
        false
      );
    }
  }

  public static findUniqueName(existingOnes: string[], startingName: string, preserveExtension:boolean=false): string {
    let folderNameToBeUsed = startingName;
    let newFolderIndex = 1;
    let extension = "";
    if (preserveExtension){
      const extensionIndex = startingName.lastIndexOf(".");
      extension = startingName.substring(extensionIndex);
      if (extensionIndex !== -1) {
        startingName = startingName.substring(0, extensionIndex);
        folderNameToBeUsed = startingName;
      }
    }
    while (existingOnes.includes(folderNameToBeUsed+extension)) {
      folderNameToBeUsed = `${startingName} (${newFolderIndex})`;
      newFolderIndex++;
    }

    return folderNameToBeUsed+extension;
  }

}
