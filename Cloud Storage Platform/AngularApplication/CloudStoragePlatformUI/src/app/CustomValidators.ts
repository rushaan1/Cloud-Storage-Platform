import {AbstractControl, ValidationErrors} from "@angular/forms";

export function invalidCharacter(control:AbstractControl):ValidationErrors|null{
  const inputValue = control?.value;
  if(!inputValue){
    return null;
  }

  const invalidChars = invalidFileNameChars();

  for (const char of inputValue){
    if (invalidChars.has(char)) {
      return {invalidCharacter:true, invalidCharactersString:inputValue.toString().split("").filter((char2:any)=>invalidChars.has(char2)).toString().replaceAll(",","")};
    }
  }
  return null;
}

export function invalidFileNameChars():Set<string>{
  return new Set(['<','.', '>', ':', '"', '/', '\\', '|', '?', '*']);
}
