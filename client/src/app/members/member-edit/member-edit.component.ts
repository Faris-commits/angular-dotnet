import {
  Component,
  HostListener,
  OnInit,
  ViewChild,
  inject,
} from '@angular/core';
import { Member } from '../../_models/member';
import { MembersService } from '../../_services/members.service';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { FormsModule, NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { PhotoEditorComponent } from '../photo-editor/photo-editor.component';
import { DatePipe } from '@angular/common';
import { TimeagoModule } from 'ngx-timeago';
import { InputWrapperComponent } from "../../input-wrapper/input-wrapper/input-wrapper.component";
import { AuthStoreService } from '../../_services/AuthStoreService';

@Component({
  selector: 'app-member-edit',
  standalone: true,
  templateUrl: './member-edit.component.html',
  styleUrl: './member-edit.component.css',
  imports: [
    TabsModule,
    FormsModule,
    PhotoEditorComponent,
    DatePipe,
    TimeagoModule,
    InputWrapperComponent
  ],
})
export class MemberEditComponent implements OnInit {
  @ViewChild('editForm') editForm?: NgForm;
  @HostListener('window:beforeunload', ['$event']) notify($event: any) {
    if (this.editForm?.dirty) {
      $event.returnValue = true;
    }
  }

  member?: Member;
  private authStore = inject(AuthStoreService)
  private memberService = inject(MembersService);
  private toastr = inject(ToastrService);

  ngOnInit(): void {
    this.loadMember();
  }

  loadMember() {
    const user = this.authStore.currentUserValue;
    if (!user) return;
    this.memberService.getMember(user.username).subscribe({
      next: member => (this.member = member),
    });
  }

  updateMember() {
    this.memberService.updateMember(this.editForm?.value).subscribe({
      next: _ => {
        this.toastr.success('Profile updated successfully');
        this.editForm?.reset(this.member);
      },
    });
  }

  onMemberChange(event: Member) {
    this.member = event;
  }
}