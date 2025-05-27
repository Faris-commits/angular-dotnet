import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface PhotoTagDto {
  id: number;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class TagService {
  baseUrl = 'https://localhost:5001/api/users/photo-tags';

  constructor(private http: HttpClient) {}

  getTags(): Observable<PhotoTagDto[]> {
    return this.http.get<PhotoTagDto[]>(this.baseUrl);
  }

  createTag(name: string): Observable<PhotoTagDto> {
    return this.http.post<PhotoTagDto>(this.baseUrl, { name });
  }
}