export type Genre =
  | 'Action'
  | 'RPG'
  | 'Sports'
  | 'Strategy'
  | 'Adventure'
  | 'Simulation'
  | 'Puzzle'
  | 'Horror';

export type Platform =
  | 'PC'
  | 'PlayStation'
  | 'Xbox'
  | 'Nintendo'
  | 'Mobile';

export const GENRES: Genre[] = [
  'Action', 'RPG', 'Sports', 'Strategy', 'Adventure', 'Simulation', 'Puzzle', 'Horror'
];

export const PLATFORMS: Platform[] = [
  'PC', 'PlayStation', 'Xbox', 'Nintendo', 'Mobile'
];

export interface Game {
  id: string;
  title: string;
  genre: Genre;
  platform: Platform;
  releaseDate: string;
  developer: string;
  rating: number;
  downloadCount: number;
  coverImageUrl: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface SaveGameCommand {
  title: string;
  genre: Genre;
  platform: Platform;
  releaseDate: string;
  developer: string;
  rating: number;
  downloadCount: number;
}
