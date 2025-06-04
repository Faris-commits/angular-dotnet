import { Component, Output, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { PhotoTagSelectorComponent } from "../../photo-tags/photo-tag-selector/photo-tag-selector.component";

@Component({
  selector: 'app-photo-upload',
  standalone: true,
  imports: [PhotoTagSelectorComponent],
  templateUrl: './photo-upload.component.html',
  styleUrl: './photo-upload.component.css'
})
export class PhotoUploadComponent {
  selectedFile: File | null = null;
  selectedTagIds: number[] = [];

  @Output() photoUploaded = new EventEmitter<any>();

  constructor(private http: HttpClient) {}

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
  }

  uploadPhoto() {
    if (!this.selectedFile) return;
    const formData = new FormData();
    formData.append('file', this.selectedFile);
    formData.append('tagIds', JSON.stringify(this.selectedTagIds));

    this.http.post<any>('https://localhost:5001/api/users/add-photo', formData)
      .subscribe(photo => {
        this.photoUploaded.emit(photo);
        this.selectedFile = null;
        this.selectedTagIds = [];
      });
  }
}