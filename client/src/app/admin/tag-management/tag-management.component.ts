import { Component, OnInit } from '@angular/core';
import { TagService, PhotoTagDto } from '../../tags/tag.service';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-tag-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tag-management.component.html'
})
export class TagManagementComponent implements OnInit {
  tags: PhotoTagDto[] = [];
  newTagName = '';

  constructor(private tagService: TagService, private http: HttpClient) {}

  ngOnInit() {
    this.loadTags();
  }

  loadTags() {
    this.tagService.getTags().subscribe(tags => this.tags = tags);
  }

addTag() {
  if (!this.newTagName.trim()) return;
  this.http.post<PhotoTagDto>('https://localhost:5001/api/admin/photo-tags', { name: this.newTagName }, {
    headers: { 'Content-Type': 'application/json' }
  }).subscribe(() => {
    this.newTagName = '';
    this.loadTags();
  });
}

deleteTag(tag: PhotoTagDto) {
  this.http.delete(`https://localhost:5001/api/users/tags/${tag.id}`)
    .subscribe(() => this.loadTags());
}
}