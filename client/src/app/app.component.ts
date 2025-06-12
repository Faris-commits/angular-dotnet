import { Component, OnInit, inject, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavComponent } from './nav/nav.component';
import { HomeComponent } from './home/home.component';
import { NgxSpinnerModule } from 'ngx-spinner'; // <-- import module here
import { AuthStoreService } from './_services/AuthStoreService';
import { LoadingService } from './_services/loading.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],  // <-- fixed plural here
  imports: [RouterOutlet, NavComponent, HomeComponent, NgxSpinnerModule, CommonModule], // <-- module here
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent implements OnInit {
  private authStore = inject(AuthStoreService);
  loadingService = inject(LoadingService);
  private cdr = inject(ChangeDetectorRef);

  ngOnInit(): void {
    this.authStore.currentUser$.subscribe(user => {
      console.log('Current user:', user);
    });

    this.loadingService.loading$.subscribe(() => {
      this.cdr.markForCheck(); 
    });
  }
}
