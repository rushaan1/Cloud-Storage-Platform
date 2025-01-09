import {ArgumentInvalidException} from "./ArgumentInvalidException";
export class HelperMethods{
  public validString(str:string):boolean{
    if (str && str.trim().length > 0){
      return true;
    }
    return false;
  }

  public handleStringInvalidError(str:string, errorMessage:string="Invalid string supplied"):void {
    if (this.validString(str)){
      throw new ArgumentInvalidException(errorMessage);
    }
  }
}
