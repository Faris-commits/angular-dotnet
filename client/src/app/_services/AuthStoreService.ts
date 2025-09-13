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
    const obj = JSON.parse(storedUser ??  '' );
    const user = storedUser ? this.decodeToken(obj.token) : null;
    this.currentUserSubject = new BehaviorSubject<User | null>(user);
    this.currentUser$ = this.currentUserSubject.asObservable();
  }

  public get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  setCurrentUser(user: User): void {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUserSubject.next(this.decodeToken(user.token));
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


  
 private decodeToken(token: string): User {
  const payload = JSON.parse(atob(token.split('.')[1]));
  return {
    username: payload.unique_name,
    knownAs: payload.knownAs, 
    photoUrl: payload.photoUrl,
    token: token,
    gender: payload.gender,
    roles: payload.role, 
  } as User;
}
}