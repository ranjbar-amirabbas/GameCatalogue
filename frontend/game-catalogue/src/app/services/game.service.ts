import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Game, Genre, PagedResult, Platform, SaveGameCommand } from '../models/game.model';

@Injectable({ providedIn: 'root' })
export class GameService {
  private readonly baseUrl = `${environment.apiUrl}/api/v1/games`;

  constructor(private readonly http: HttpClient) {}

  getGames(
    page: number,
    pageSize: number,
    genre?: Genre | null,
    platform?: Platform | null,
    searchTerm?: string | null
  ): Observable<PagedResult<Game>> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    if (genre) {
      params = params.set('genre', genre);
    }
    if (platform) {
      params = params.set('platform', platform);
    }
    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }

    return this.http.get<PagedResult<Game>>(this.baseUrl, { params });
  }

  getGameById(id: string): Observable<Game> {
    return this.http.get<Game>(`${this.baseUrl}/${id}`);
  }

  createGame(command: SaveGameCommand, file?: File | null): Observable<string> {
    const formData = new FormData();
    formData.append('title', command.title);
    formData.append('genre', command.genre);
    formData.append('platform', command.platform);
    formData.append('releaseDate', command.releaseDate);
    formData.append('developer', command.developer);
    formData.append('rating', String(command.rating));
    formData.append('downloadCount', String(command.downloadCount));
    if (file) {
      formData.append('file', file);
    }
    return this.http.post<string>(this.baseUrl, formData);
  }

  updateGame(id: string, command: SaveGameCommand): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, { id, ...command });
  }

  deleteGame(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  uploadCoverImage(id: string, file: File): Observable<{ url: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ url: string }>(`${this.baseUrl}/${id}/cover-image`, formData);
  }
}
