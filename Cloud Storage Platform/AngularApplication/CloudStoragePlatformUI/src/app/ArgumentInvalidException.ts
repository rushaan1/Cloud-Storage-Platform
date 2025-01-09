export class ArgumentInvalidException extends Error {
  constructor(message:string) {
    super(message);
    this.name = "ArgumentInvalidException";
  }
}
