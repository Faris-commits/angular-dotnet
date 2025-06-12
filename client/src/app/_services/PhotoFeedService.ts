import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { interval, Observable } from 'rxjs';
import { switchMap, map, distinctUntilChanged, shareReplay, startWith } from 'rxjs/operators';
import { Photo } from '../_models/photo';

@Injectable({ providedIn: 'root' })
export class PhotoFeedService {
  constructor(private http: HttpClient) {}

  getApprovedPhotos(): Observable<Photo[]> {
    return interval(10000).pipe(
      startWith(0),
      switchMap(() => this.http.get<Photo[]>('https://localhost:5001/api/users/photos')),
      map(photos => photos.filter(photo => photo.isApproved)), 
      distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b)), 
      shareReplay(1) 
    );
  }
}
