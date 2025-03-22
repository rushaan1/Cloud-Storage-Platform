export class Metadata {
  parentFolderName: string;
  subFoldersCount: number;
  subFilesCount: number;
  creationDate: Date | null | string;
  metadataId: string;
  previousReplacementDate: Date | null | string;
  replaceCount: number;
  previousRenameDate: Date | null | string;
  renameCount: number;
  previousPath: string | null;
  previousMoveDate: Date | null | string;
  moveCount: number;
  lastOpened: Date | null | string;
  openCount: number;
  shareCount: number;
  size: number;

  constructor(data: Partial<Metadata> = {}) {
    this.parentFolderName = data.parentFolderName ?? "";
    this.subFoldersCount = data.subFoldersCount ?? 0;
    this.subFilesCount = data.subFilesCount ?? 0;
    this.creationDate = data.creationDate ? new Date(data.creationDate) : null;
    this.metadataId = data.metadataId ?? "";
    this.previousReplacementDate = data.previousReplacementDate ? new Date(data.previousReplacementDate) : null;
    this.replaceCount = data.replaceCount ?? 0;
    this.previousRenameDate = data.previousRenameDate ? new Date(data.previousRenameDate) : null;
    this.renameCount = data.renameCount ?? 0;
    this.previousPath = data.previousPath ?? null;
    this.previousMoveDate = data.previousMoveDate ? new Date(data.previousMoveDate) : null;
    this.moveCount = data.moveCount ?? 0;
    this.lastOpened = data.lastOpened ? new Date(data.lastOpened) : null;
    this.openCount = data.openCount ?? 0;
    this.shareCount = data.shareCount ?? 0;
    this.size = data.size ?? 0;
  }
}
