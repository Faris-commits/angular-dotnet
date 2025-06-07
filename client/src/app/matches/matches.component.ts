import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';

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
  imports: [CommonModule]
})
export class MatchesComponent implements OnInit {
  matches: Match[] = [];

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.http.get<Match[]>('/api/users/matches').subscribe({
      next: matches => (this.matches = matches),
    });
  }
}