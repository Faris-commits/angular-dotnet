import { Component } from '@angular/core';
import { LoadingService } from '../_services/loading.service';
import { NgxSpinnerModule } from 'ngx-spinner'; 

@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [NgxSpinnerModule], 
  templateUrl: './loading-spinner.component.html',
  styleUrls: ['./loading-spinner.component.css']
})
export class LoadingSpinnerComponent {
  constructor(public loadingService: LoadingService) {}
}
