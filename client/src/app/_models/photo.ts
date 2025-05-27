export interface Photo {
  id: number;
  url: string;
  isMain: boolean;
  isApproved: boolean;
  username?: string;
  tags?: { id: number; name: string}[];
}
