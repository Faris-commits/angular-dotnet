import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable({
  providedIn: 'root',
})
export class LoadingService {
  private loadingSubject = new BehaviorSubject<boolean>(false);
  loading$ = this.loadingSubject.asObservable();

  constructor(private spinner: NgxSpinnerService) {}

  setLoading(isLoading: boolean): void {
    this.loadingSubject.next(isLoading);
    if (isLoading) {
      this.spinner.show();
    } else {
      this.spinner.hide();
    }
  }
}