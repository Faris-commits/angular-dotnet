import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PhotoTagSelectorComponent } from "../../photo-tags/photo-tag-selector/photo-tag-selector.component";
import { BehaviorSubject, combineLatest, Observable } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';

@Component({
  selector: 'app-photo-list',
  standalone: true,
  imports: [PhotoTagSelectorComponent],
  templateUrl: './photo-list.component.html',
  styleUrl: './photo-list.component.css'
})
export class PhotoListComponent implements OnInit {
  private photos$ = new BehaviorSubject<any[]>([]);
  private selectedTagIds$ = new BehaviorSubject<number[]>([]);
  private approvalStatus$ = new BehaviorSubject<boolean|null>(null);

  filterTagIds: number[] = [];
  approvalStatus: string = '';

  filteredPhotos$: Observable<any[]> = combineLatest([
    this.photos$,
    this.selectedTagIds$,
    this.approvalStatus$
  ]).pipe(
    map(([photos, tagIds, approval]) =>
      photos.filter(photo => {
        const matchesTags = tagIds.length === 0 || (photo.tags ?? []).some((tag: any) => tagIds.includes(tag.id));
        const matchesApproval = approval === null || photo.isApproved === approval;
        return matchesTags && matchesApproval;
      })
    )
  );

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.loadPhotos();
  }

  loadPhotos() {
    this.http.get<any[]>('https://localhost:5001/api/users/photos').subscribe(p => this.photos$.next(p));
  }

  onTagIdsChange(tagIds: number[]) {
    this.filterTagIds = tagIds;
    this.selectedTagIds$.next(tagIds);
  }

  onApprovalStatusChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    const value = select.value;
    this.approvalStatus = value;
    this.approvalStatus$.next(value === '' ? null : value === 'true');
  }

  getTagNames(photo: any): string {
    return photo.tags && photo.tags.length
      ? photo.tags.map((t: any) => t.name).join(', ')
      : 'None';
  }
}