export class Metadata {
  parentFolderName: string;
  subFoldersCount: number;
  subFilesCount: number;
  creationDate: Date | null;
  metadataId: string;
  previousReplacementDate: Date | null;
  replaceCount: number;
  previousRenameDate: Date | null;
  renameCount: number;
  previousPath: string | null;
  previousMoveDate: Date | null;
  moveCount: number;
  lastOpened: Date | null;
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

  formatDate(date: Date | null, locale: string = 'en-GB'): string {
    return date ? date.toLocaleString(locale, { year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' }) : "Never";
  }

  getFormattedCreationDate(): string {
    return this.formatDate(this.creationDate);
  }

  getFormattedLastOpened(): string {
    return this.formatDate(this.lastOpened);
  }

  getFormattedPreviousMoveDate(): string {
    return this.formatDate(this.previousMoveDate);
  }

  getFormattedPreviousRenameDate(): string {
    return this.formatDate(this.previousRenameDate);
  }

  getFormattedPreviousReplacementDate(): string {
    return this.formatDate(this.previousReplacementDate);
  }
}
