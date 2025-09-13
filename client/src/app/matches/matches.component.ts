import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface Match {
  username: string;
  score: number;
  photoUrl: string;
  age: number;
  knownAs: string;
  city: string;
}

@Component({
  selector: 'app-matches',
  standalone: true,
  templateUrl: './matches.component.html',
  styleUrl: './matches.component.css',
  imports: [CommonModule, FormsModule]
})
export class MatchesComponent implements OnInit {
  matches: Match[] = [];
  filter = {
    gender: '',
    city: ''
  };

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.getMatches();
  }

  getMatches(): void {
    let params = new HttpParams();
    if (this.filter.gender) params = params.set('gender', this.filter.gender);
    if (this.filter.city) params = params.set('city', this.filter.city);

    this.http.get<Match[]>('https://localhost:5001/api/users/matches', { params }).subscribe({
      next: matches => {
        this.matches = matches;
        console.log('Matches:', this.matches);
      },
      error: err => {
        console.error('Error fetching matches:', err);
        if (err.error instanceof SyntaxError) {
          console.error('Non-JSON response received:', err.error.text);
        }
        this.matches = [];
      },
    });
  }
}