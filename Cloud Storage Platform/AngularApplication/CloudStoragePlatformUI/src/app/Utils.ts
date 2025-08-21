import {ArgumentInvalidException} from "./ArgumentInvalidException";
import {FileType} from "./models/FileType";
import {File} from "./models/File";
import { AbstractControl, ValidationErrors } from '@angular/forms';

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

  /**
   * MAIN Purpose is to combine an array of strings into a string containing \ before every element string
   * @param str - The string array to be combined
   */
  public static constructFilePathForApi(url:string[]):string{
    let constructedPathForApi = "";
    for (let i = url.indexOf("home"); i< url.length; i++) {
      constructedPathForApi = constructedPathForApi + "\\" + url[i];
    }
    return constructedPathForApi;
  }

  /**
   * JUST TAKES PATH INTO ARRAY INPUT THENN FINDS FIRST INDEX OF home AND RETURNS NEW ARRAY STARTING FROM HOME TO END
   * @param crumbs
   */
  public static obtainBreadCrumbs(crumbs:string[]):string[] {
    let breadCrumbs:string[] = [];
    for (let i = crumbs.indexOf("home"); i < crumbs.length; i++) {
      breadCrumbs.push(crumbs[i]);
    }
    return breadCrumbs;
  }

  /**
  * TAKES A PATH STRING AND SPLITS IT INTO AN ARRAY OF STRINGS STARTING FROM FIRST home occurance till end
  * @param path - The path string to be cleaned
  */
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
        data.size,
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
        false,
        data.size
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

  public static getCookie(name: string): string | null {
    const nameEQ = name + "=";
    const ca = document.cookie.split(';');
    for (let i = 0; i < ca.length; i++) {
      let c = ca[i].trim();
      if (c.startsWith(nameEQ)) {
        return decodeURIComponent(c.substring(nameEQ.length));
      }
    }
    return null;
  }

}

export function phoneNumberValidator(control: AbstractControl): ValidationErrors | null {
  if (!control.value) {
    return null; // Allow empty values since phone is optional
  }
  const phoneNumber = control.value.toString().replace(/\s+/g, ''); // Remove spaces
  const phoneRegex = /^[\+]?[1-9][\d]{0,15}$/; // International phone number format
  if (!phoneRegex.test(phoneNumber)) {
    return { invalidPhoneNumber: true };
  }
  return null;
}
