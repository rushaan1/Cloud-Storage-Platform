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

  public constructFilePathForApi(url:string[]):string{
    let constructedPathForApi = "";
    for (let i = 1; i< url.length; i++) {
      constructedPathForApi = constructedPathForApi + "\\" + url[i];
    }
    return constructedPathForApi;
  }
}
