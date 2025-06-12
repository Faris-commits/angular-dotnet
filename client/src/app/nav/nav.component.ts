import { Component, inject, OnInit } from '@angular/core';
import { map } from 'rxjs/operators';
import { AuthStoreService } from '../_services/AuthStoreService';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { HasRoleDirective } from '../_directives/has-role.directive';
import { ButtonWrapperComponent } from '../button-wrapper/button-wrapper/button-wrapper.component';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-nav',
  standalone: true,
  imports: [
    FormsModule,
    BsDropdownModule,
    RouterLink,
    RouterLinkActive,
    HasRoleDirective,
    ButtonWrapperComponent,
    CommonModule
  ],
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.css',
})
export class NavComponent implements OnInit {
  private router = inject(Router);
  private toastr = inject(ToastrService);
  private authStore = inject(AuthStoreService);
  private accountService = inject(AccountService);

  model: any = {};

  isLoggedIn$ = this.authStore.isLoggedIn$;
  currentUser$ = this.authStore.currentUser$;
  isAdmin$ = this.currentUser$.pipe(
    map(user => {
      if (!user) return false;
      const roles = Array.isArray(user.roles) ? user.roles : [user.roles];
      return roles.some(role => role === 'Admin' || role === 'Moderator');
    })
  );

  ngOnInit(): void {
    const token = localStorage.getItem('token');
    if (token) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      console.log('Decoded Token Payload:', payload);
      console.log('User Roles:', payload.role);
    }

    this.currentUser$.subscribe(user => {
      console.log('Current User:', user);
    });
  }

  login() {
    this.accountService.login(this.model).subscribe({
      next: () => {
        this.router.navigateByUrl('/members');
      },
      error: error => this.toastr.error(error.error),
    });
  }

  logout() {
    this.router.navigateByUrl('/');
    this.accountService.logout();
  }
}