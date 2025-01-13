import {ArgumentInvalidException} from "./ArgumentInvalidException";
import {UrlSegment} from "@angular/router";
export class HelperMethods{
  public validString(str:string):boolean{
    if (str && str.trim().length > 0){
      return true;
    }
    return false;
  }

  public handleStringInvalidError(str:string, errorMessage:string="Invalid string supplied"):void {
    if (!this.validString(str)){
      throw new ArgumentInvalidException(errorMessage);
    }
  }

  // MAIN Purpose is to combine an array of strings into a string containing \\ before every element string
  public constructFilePathForApi(url:string[]):string{
    let constructedPathForApi = "";
    for (let i = url.indexOf("home"); i< url.length; i++) {
      constructedPathForApi = constructedPathForApi + "\\" + url[i];
    }
    return constructedPathForApi;
  }

  public obtainBreadCrumbs(crumbs:string[]):string[] {
    let breadCrumbs:string[] = [];
    for (let i = crumbs.indexOf("home"); i < crumbs.length; i++) {
      breadCrumbs.push(crumbs[i]);
    }
    return breadCrumbs;
  }

  public cleanPath(path:string):string[]{
    let constructedPath = path.split("\\");
    let index = constructedPath.indexOf("home");
    constructedPath = constructedPath.slice(index, constructedPath.length);

    return constructedPath;
  }
}
