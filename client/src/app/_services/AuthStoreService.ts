import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AuthStoreService {
  private currentUserSubject: BehaviorSubject<User | null>;
  public currentUser$: Observable<User | null>;

  constructor() {
    const storedUser = localStorage.getItem('user');
    const user = storedUser ? JSON.parse(storedUser) as User : null;
    this.currentUserSubject = new BehaviorSubject<User | null>(user);
    this.currentUser$ = this.currentUserSubject.asObservable();
  }

  public get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  setCurrentUser(user: User): void {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  clearCurrentUser(): void {
    localStorage.removeItem('user');
    this.currentUserSubject.next(null);
  }

  get isLoggedIn$(): Observable<boolean> {
    return new Observable<boolean>(observer => {
      this.currentUser$.subscribe(user => {
        observer.next(!!user);
      });
    });
  }
}