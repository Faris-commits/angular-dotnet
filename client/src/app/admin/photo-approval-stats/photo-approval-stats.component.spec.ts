import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotoApprovalStatsComponent } from './photo-approval-stats.component';

describe('PhotoApprovalStatsComponent', () => {
  let component: PhotoApprovalStatsComponent;
  let fixture: ComponentFixture<PhotoApprovalStatsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotoApprovalStatsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PhotoApprovalStatsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
