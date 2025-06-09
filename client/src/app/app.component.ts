import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavComponent } from './nav/nav.component';
import { HomeComponent } from './home/home.component';
import { NgxSpinnerComponent } from 'ngx-spinner';
import { AuthStoreService } from './_services/AuthStoreService';

@Component({
  selector: 'app-root',
  standalone: true,
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
  imports: [RouterOutlet, NavComponent, HomeComponent, NgxSpinnerComponent],
})
export class AppComponent implements OnInit {
  private authStore = inject(AuthStoreService);

  ngOnInit(): void {
   this.authStore.currentUser$.subscribe(user => {
    console.log('Current user:', user);
   });
  }

}
