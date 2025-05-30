import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotoTagSelectorComponent } from './photo-tag-selector.component';

describe('PhotoTagSelectorComponent', () => {
  let component: PhotoTagSelectorComponent;
  let fixture: ComponentFixture<PhotoTagSelectorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotoTagSelectorComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PhotoTagSelectorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
