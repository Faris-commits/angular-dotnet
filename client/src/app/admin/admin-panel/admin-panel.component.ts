import { Component } from '@angular/core';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { UserManagementComponent } from '../user-management/user-management.component';
import { HasRoleDirective } from '../../_directives/has-role.directive';
import { PhotoManagementComponent } from '../photo-management/photo-management.component';
import { PhotoApprovalStatsComponent } from "../photo-approval-stats/photo-approval-stats.component";


@Component({
  selector: 'app-admin-panel',
  standalone: true,
  templateUrl: './admin-panel.component.html',
  styleUrl: './admin-panel.component.css',
  imports: [
    TabsModule,
    UserManagementComponent,
    HasRoleDirective,
    PhotoManagementComponent,
    PhotoApprovalStatsComponent
],
})
export class AdminPanelComponent {}
