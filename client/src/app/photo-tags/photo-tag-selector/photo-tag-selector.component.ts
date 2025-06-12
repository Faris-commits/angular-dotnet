import { Component, EventEmitter, Input, Output } from '@angular/core';
import { PhotoTagDto, TagService } from '../../tags/tag.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-photo-tag-selector',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './photo-tag-selector.component.html',
  styleUrl: './photo-tag-selector.component.css'
})
export class PhotoTagSelectorComponent {
  @Input() selectedTagIds: number[] = [];
  @Output() selectedTagIdsChange = new EventEmitter<number[]>();
  tags: PhotoTagDto[] = [];
  newTagName = '';

  constructor(private tagService: TagService) {}

  ngOnInit() {
    this.tagService.getTags().subscribe(tags => this.tags = tags);
  }

  toggleTag(tagId: number) {
    if (this.selectedTagIds.includes(tagId)) {
      this.selectedTagIds = this.selectedTagIds.filter(id => id !== tagId);
    } else {
      this.selectedTagIds = [...this.selectedTagIds, tagId];
    }
    this.selectedTagIdsChange.emit(this.selectedTagIds);
  }

  addTag() {
    const name = this.newTagName.trim();
    if (!name) return;
    this.tagService.createTag(name).subscribe({
      next: (tag: PhotoTagDto) => {
        this.tags.push(tag);
        this.toggleTag(tag.id);
        this.newTagName = '';
      }
    });
  }
}
