import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { HasRoleDirective } from '../_directives/has-role.directive';
import { ButtonWrapperComponent } from "../button-wrapper/button-wrapper/button-wrapper.component";
import { AuthStoreService } from '../_services/AuthStoreService';

@Component({
  selector: 'app-nav',
  standalone: true,
  imports: [
    FormsModule,
    BsDropdownModule,
    RouterLink,
    RouterLinkActive,
    HasRoleDirective,
    ButtonWrapperComponent
  ],
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.css',
})
export class NavComponent {
  private router = inject(Router);
  accountService = inject(AccountService);
  private toastr = inject(ToastrService);
  public authStore = inject(AuthStoreService);
  public user = toSignal(this.authStore.currentUser$, { initialValue: null });
  model: any = {};

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